namespace Hermes.SituationRoom.Domain.Services;

using AutoMapper;
using Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;

public sealed class PostLikeService(IPostLikeRepository postLikeRepository, IMapper mapper, INotificationService notificationService, IUserService userService, IPostService postService) : IPostLikeService
{
    public async Task<PostLikeStatsDto> GetPostLikeStatsAsync(Guid postUid, Guid? userUid)
    {
        var likeCount = await postLikeRepository.GetLikeCountAsync(postUid);
        var userHasLiked = userUid.HasValue && await postLikeRepository.HasUserLikedPostAsync(postUid, userUid.Value);

        return new PostLikeStatsDto(postUid, likeCount, userHasLiked);
    }

    public async Task<Guid> ToggleLikeAsync(Guid postUid, Guid userUid)
    {
        var existingLike = await postLikeRepository.GetAsync(postUid, userUid);

        if (existingLike != null)
        {
            // Unlike
            await postLikeRepository.DeleteAsync(postUid, userUid);
            return Guid.Empty; // Indicates unliked
        }
        else
        {
            // Like
            var postLikeBo = new PostLikeBo(
                Guid.NewGuid(),
                postUid,
                userUid,
                DateTime.UtcNow
            );

            var likeUid = await postLikeRepository.AddAsync(postLikeBo);

            // Create notification for post owner (if not the liker)
            try
            {
                var post = await postService.GetPostAsync(postUid);
                if (post.CreatorUid != userUid)
                {
                    var likerProfile = await userService.GetUserProfileAsync(userUid, post.CreatorUid);
                    var likerName = GetDisplayName(likerProfile);
                    await notificationService.CreateLikeNotificationAsync(post.CreatorUid, likerName, postUid);
                }
            }
            catch (Exception)
            {
                // Don't fail like creation if notification fails
            }

            return likeUid;
        }
    }

    public async Task<IReadOnlyList<PostLikeDto>> GetPostLikesAsync(Guid postUid)
    {
        var likes = await postLikeRepository.GetPostLikesAsync(postUid);
        return mapper.Map<IReadOnlyList<PostLikeDto>>(likes);
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

