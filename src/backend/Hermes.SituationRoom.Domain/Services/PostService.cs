#nullable enable
namespace Hermes.SituationRoom.Domain.Services;

using AutoMapper;
using Data.Entities;
using Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Shared.Exceptions;
using Hubs;
using Microsoft.AspNetCore.SignalR;

public class PostService(IPostRepository postRepository, ITagService tagService, IMapper mapper, IContentFilterService contentFilterService, IHubContext<ChatHub> hubContext, IAttachmentService attachmentService) : IPostService
{
    public async Task<PostWithTagsDto> GetPostAsync(Guid postUid)
    {
        var post = await postRepository.GetPostBoAsync(postUid);
        var tags = await tagService.GetAllTagsFromPostAsync(postUid);
        var postWithTags = MapToBo(post, tags);
        return mapper.Map<PostWithTagsDto>(postWithTags);
    }

    public async Task<IReadOnlyList<PostWithTagsDto>> GetActivistPostsByTagsAsync(string tags, Guid userUid, string? userRole, int limit, int offset, string? query = null, string? sortBy = null)
    {
        int privacyLevel = 0;

        if (userRole == "Journalist")
            privacyLevel = 2;
        else if (userRole == "Activist")
            privacyLevel = 1;

        var posts = await postRepository.GetActivistPostsByTagsAsync(await ValidateTagsAsync(tags), privacyLevel, userUid, limit, offset, query, sortBy);
        return mapper.Map<IReadOnlyList<PostWithTagsDto>>(posts);
    }

    public async Task<IReadOnlyList<PostWithTagsDto>> GetJournalistPostsByTagsAsync(string tags, int limit, int offset, string? query = null, string? sortBy = null)
    {
        var posts = await postRepository.GetJournalistPostsByTagsAsync(await ValidateTagsAsync(tags), limit, offset, query, sortBy);
        return mapper.Map<IReadOnlyList<PostWithTagsDto>>(posts);
    }

    public async Task<IReadOnlyList<PostWithTagsDto>> GetUserPostsAsync(Guid userUid, Guid loggedInUserUid, string? loggedInUserRole, int limit, int offset, string? query = null, string? sortBy = null)
    {
        int privacyLevel = 0;

        if (loggedInUserRole == "Journalist")
            privacyLevel = 2;
        else if (loggedInUserRole == "Activist")
            privacyLevel = 1;

        var posts = await GetTagsFromPosts(await postRepository.GetUserPostsAsync(userUid, privacyLevel, loggedInUserUid, limit, offset, query, sortBy));
        return mapper.Map<IReadOnlyList<PostWithTagsDto>>(posts);
    }

    public async Task<IReadOnlyList<PostWithTagsDto>> GetAllActivistPostsAsync(Guid userUid, string? userRole, int limit, int offset, string? query = null, string? sortBy = null)
    {
        int privacyLevel = 0;

        if (userRole == "Journalist")
            privacyLevel = 2;
        else if (userRole == "Activist")
            privacyLevel = 1;

        var posts = await GetTagsFromPosts(await postRepository.GetAllActivistPostsAsync(privacyLevel, userUid, limit, offset, query, sortBy));
        return mapper.Map<IReadOnlyList<PostWithTagsDto>>(posts);
    }

    public async Task<IReadOnlyList<PostWithTagsDto>> GetAllJournalistPostsAsync(int limit, int offset, string? query = null, string? sortBy = null)
    {
        var posts = await GetTagsFromPosts(await postRepository.GetAllJournalistPostsAsync(limit, offset, query, sortBy));
        return mapper.Map<IReadOnlyList<PostWithTagsDto>>(posts);
    }

    public async Task<Guid> CreatePostAsync(CreatePostRequestDto createPostDto)
    {
        var (filteredTitle, titleBlocked) = await contentFilterService.ApplyAsync(createPostDto.Title);
        var (filteredDescription, descriptionBlocked) = await contentFilterService.ApplyAsync(createPostDto.Description);
        var (filteredContent, contentBlocked) = await contentFilterService.ApplyAsync(createPostDto.Content);

        if (titleBlocked || descriptionBlocked || contentBlocked)
        {
            throw new ValidationBusinessException(nameof(createPostDto), "Post contains content that is not allowed.");
        }

        var postWithTags = mapper.Map<PostWithTagsBo>(createPostDto);
        postWithTags = postWithTags with
        {
            Timestamp = DateTime.UtcNow,
            Title = filteredTitle,
            Description = filteredDescription,
            Content = filteredContent
        };

        var guid = await postRepository.AddAsync(MapToBo(postWithTags));

        foreach (var tag in createPostDto.Tags)
        {
            await tagService.CreatePostTagAsync(new(guid, tag));
        }

        // Link any uploaded attachments to the new post (owner-scoped).
        if (createPostDto.AttachmentIds is { Count: > 0 })
        {
            await attachmentService.LinkToPostAsync(createPostDto.AttachmentIds, createPostDto.CreatorUid, guid);
        }

        // Broadcast the new post to everyone viewing the posts board
        try
        {
            var createdPost = await GetPostAsync(guid);
            await hubContext.Clients.Group("posts").SendAsync("PostCreated", createdPost);
        }
        catch (Exception)
        {
            // Don't fail post creation if SignalR fails
        }

        return guid;
    }

    public async Task<PostWithTagsDto> UpdatePostAsync(UpdatePostRequestDto updatePostDto)
    {
        var existingPost = await postRepository.GetPostBoAsync(updatePostDto.Uid);
        var postWithTags = mapper.Map<PostWithTagsBo>(updatePostDto);
        postWithTags = postWithTags with { Timestamp = existingPost.Timestamp, CreatorUid = existingPost.CreatorUid };

        await postRepository.UpdateAsync(MapToBo(postWithTags));

        var existingTags = await tagService.GetAllTagsFromPostAsync(updatePostDto.Uid);

        var existingSet = existingTags?.ToHashSet() ?? new HashSet<string>();
        var updatedSet = await ValidateTagsAsync(updatePostDto.Tags);

        var tagsToAdd = updatedSet.Except(existingSet).ToList();
        var tagsToRemove = existingSet.Except(updatedSet).ToList();

        foreach (var tag in tagsToAdd)
        {
            await tagService.CreatePostTagAsync(new(updatePostDto.Uid, tag));
        }

        foreach (var tag in tagsToRemove)
        {
            await tagService.DeletePostTagAsync(new(updatePostDto.Uid, tag));
        }

        var finalTags = await tagService.GetAllTagsFromPostAsync(updatePostDto.Uid);
        var finalPostWithTags = postWithTags with { Tags = finalTags.ToList() };

        var updatedPostDto = mapper.Map<PostWithTagsDto>(finalPostWithTags);

        // Broadcast the updated post to everyone viewing the posts board
        try
        {
            await hubContext.Clients.Group("posts").SendAsync("PostUpdated", updatedPostDto);
        }
        catch (Exception)
        {
            // Don't fail post update if SignalR fails
        }

        return updatedPostDto;
    }

    public async Task DeletePostAsync(Guid postUid)
    {
        await postRepository.DeleteAsync(postUid);

        // Broadcast the deletion to everyone viewing the posts board
        try
        {
            await hubContext.Clients.Group("posts").SendAsync("PostDeleted", postUid);
        }
        catch (Exception)
        {
            // Don't fail post deletion if SignalR fails
        }
    }

    private static PostBo MapToBo(PostWithTagsBo postWithTagsBo) => new(postWithTagsBo.Uid,
        postWithTagsBo.Timestamp,
        postWithTagsBo.Title,
        postWithTagsBo.Description,
        postWithTagsBo.Content,
        postWithTagsBo.CreatorUid,
        postWithTagsBo.PrivacyLevel
    );

    private static PostWithTagsBo MapToBo(PostBo postPo, IReadOnlyList<string> tags) => new(postPo.Uid,
        postPo.Timestamp,
        postPo.Title,
        postPo.Description,
        postPo.Content,
        postPo.CreatorUid,
        postPo.PrivacyLevel,
        tags.ToList()
    );

    public async Task<IReadOnlyList<string>> GetPostPrivaciesAsync()
    {
        return await postRepository.GetPostPrivaciesAsync();
    }

    private Task<List<string>> ValidateTagsAsync(string tags) =>
        ValidateTagsAsync(tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    private async Task<List<string>> ValidateTagsAsync(IEnumerable<string> tags)
    {
        var validated = new List<string>();
        foreach (var tag in tags)
        {
            validated.Add(await tagService.ValidateTagNameAsync(tag));
        }

        return validated;
    }

    private async Task<IReadOnlyList<PostWithTagsBo>> GetTagsFromPosts(IReadOnlyList<PostBo> posts)
    {
        if (!posts.Any())
            return [];

        var postUids = posts.Select(p => p.Uid).ToList();
        var tagsByPostUid = await tagService.GetAllTagsFromPostsAsync(postUids);

        return posts.Select(post =>
                {
                    var tags = tagsByPostUid.TryGetValue(post.Uid, out var postTags)
                        ? postTags
                        : new List<string>();
                    return MapToBo(post, tags);
                }
            )
            .ToList();
    }
}
