namespace Hermes.SituationRoom.Domain.Services;

using AutoMapper;
using Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Shared.Exceptions;
using Hubs;
using Microsoft.AspNetCore.SignalR;

public class CommentService(ICommentRepository commentRepository, IMapper mapper, INotificationService notificationService, IUserService userService, IPostService postService, IContentFilterService contentFilterService, IHubContext<ChatHub> hubContext) : ICommentService
{
    public async Task<IReadOnlyList<CommentDto>> GetPostCommentsAsync(Guid postUid)
    {
        var comments = await commentRepository.GetPostCommentsAsync(postUid);
        return mapper.Map<IReadOnlyList<CommentDto>>(comments);
    }
    
    public async Task<Guid> CreateCommentAsync(CreateCommentRequestDto createCommentDto)
    {
        var (filteredContent, blocked) = await contentFilterService.ApplyAsync(createCommentDto.Content);

        if (blocked)
        {
            throw new ValidationBusinessException(nameof(createCommentDto.Content), "Comment contains content that is not allowed.");
        }

        var commentBo = mapper.Map<CommentBo>(createCommentDto);
        commentBo = commentBo with { Timestamp = DateTime.UtcNow, Content = filteredContent };
        var commentUid = await commentRepository.AddAsync(commentBo);

        // Create notification for post owner (if not the commenter)
        try
        {
            var post = await postService.GetPostAsync(createCommentDto.PostUid);
            if (post.CreatorUid != createCommentDto.CreatorUid)
            {
                var commenterProfile = await userService.GetUserProfileAsync(createCommentDto.CreatorUid, post.CreatorUid);
                var commenterName = GetDisplayName(commenterProfile);
                await notificationService.CreateCommentNotificationAsync(post.CreatorUid, commenterName, createCommentDto.PostUid);
            }

            // If this is a reply, notify the parent comment owner
            if (createCommentDto.ParentCommentUid.HasValue)
            {
                var parentComment = await commentRepository.GetCommentAsync(createCommentDto.ParentCommentUid.Value);
                if (parentComment.CreatorUid != createCommentDto.CreatorUid)
                {
                    var replierProfile = await userService.GetUserProfileAsync(createCommentDto.CreatorUid, parentComment.CreatorUid);
                    var replierName = GetDisplayName(replierProfile);
                    await notificationService.CreateReplyNotificationAsync(parentComment.CreatorUid, replierName, createCommentDto.ParentCommentUid.Value, createCommentDto.PostUid);
                }
            }
        }
        catch (Exception)
        {
            // Don't fail comment creation if notification fails
        }

        // Broadcast the new comment (or reply) to everyone viewing the post
        try
        {
            var createdComment = await commentRepository.GetCommentAsync(commentUid);
            var createdCommentDto = mapper.Map<CommentDto>(createdComment);
            var eventName = createCommentDto.ParentCommentUid.HasValue ? "CommentReplyAdded" : "CommentCreated";
            await hubContext.Clients.Group($"post-comments-{createCommentDto.PostUid}")
                .SendAsync(eventName, createdCommentDto);
        }
        catch (Exception)
        {
            // Don't fail comment creation if SignalR fails
        }

        return commentUid;
    }

    public async Task<CommentDto> UpdateCommentAsync(UpdateCommentRequestDto updateCommentDto)
    {
        var existingComment = await commentRepository.GetCommentAsync(updateCommentDto.Uid);
        var commentBo = existingComment with { Uid = updateCommentDto.Uid, Content = updateCommentDto.Content };
        var updatedComment = await commentRepository.UpdateAsync(commentBo);
        var updatedCommentDto = mapper.Map<CommentDto>(updatedComment);

        // Broadcast the updated comment to everyone viewing the post
        try
        {
            await hubContext.Clients.Group($"post-comments-{updatedCommentDto.PostUid}")
                .SendAsync("CommentUpdated", updatedCommentDto);
        }
        catch (Exception)
        {
            // Don't fail comment update if SignalR fails
        }

        return updatedCommentDto;
    }

    public async Task DeleteCommentAsync(Guid commentUid)
    {
        Guid postUid = Guid.Empty;
        try
        {
            var existingComment = await commentRepository.GetCommentAsync(commentUid);
            postUid = existingComment.PostUid;
        }
        catch (Exception)
        {
            // If we can't resolve the post we simply won't broadcast below
        }

        await commentRepository.DeleteAsync(commentUid);

        // Broadcast the deletion to everyone viewing the post
        if (postUid != Guid.Empty)
        {
            try
            {
                await hubContext.Clients.Group($"post-comments-{postUid}")
                    .SendAsync("CommentDeleted", commentUid);
            }
            catch (Exception)
            {
                // Don't fail comment deletion if SignalR fails
            }
        }
    }

    private static string GetDisplayName(UserProfileDto? profile)
    {
        if (profile == null) return "Someone";
        
        if (!string.IsNullOrEmpty(profile.FirstName) && !string.IsNullOrEmpty(profile.LastName))
            return $"{profile.FirstName} {profile.LastName}";
        
        if (!string.IsNullOrEmpty(profile.UserName))
            return profile.UserName;
        
        if (!string.IsNullOrEmpty(profile.FirstName))
            return profile.FirstName;
        
        return "Someone";
    }
}
