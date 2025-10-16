#nullable enable
namespace Hermes.SituationRoom.Domain.Services;

using AutoMapper;
using Data.Entities;
using Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Shared.Enums;

public class PostService(IPostRepository postRepository, ITagService tagService, IMapper mapper) : IPostService
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

        var posts = await postRepository.GetActivistPostsByTagsAsync(tags.Split(',').Select(TagService.MapToTag).ToList(), privacyLevel, userUid, limit, offset, query, sortBy);
        return mapper.Map<IReadOnlyList<PostWithTagsDto>>(posts);
    }

    public async Task<IReadOnlyList<PostWithTagsDto>> GetJournalistPostsByTagsAsync(string tags, int limit, int offset, string? query = null, string? sortBy = null)
    {
        var posts = await postRepository.GetJournalistPostsByTagsAsync(tags.Split(',').Select(TagService.MapToTag).ToList(), limit, offset, query, sortBy);
        return mapper.Map<IReadOnlyList<PostWithTagsDto>>(posts);
    }

    public async Task<IReadOnlyList<PostWithTagsDto>> GetUserPostsAsync(Guid userUid, int limit, int offset, string? query = null, string? sortBy = null)
    {
        var posts = await GetTagsFromPosts(await postRepository.GetUserPostsAsync(userUid, limit, offset, query, sortBy));
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
        var postWithTags = mapper.Map<PostWithTagsBo>(createPostDto);
        postWithTags = postWithTags with { Timestamp = DateTime.UtcNow };

        var guid = await postRepository.AddAsync(MapToBo(postWithTags));

        foreach (var tag in createPostDto.Tags)
        {
            await tagService.CreatePostTagAsync(new(guid, tag));
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

        var existingSet = existingTags?.ToHashSet() ?? new HashSet<Tag>();
        var updatedSet = updatePostDto.Tags.Select(TagService.MapToTag).ToList();

        var tagsToAdd = updatedSet.Except(existingSet).ToList();
        var tagsToRemove = existingSet.Except(updatedSet).ToList();

        foreach (var tag in tagsToAdd)
        {
            await tagService.CreatePostTagAsync(new(updatePostDto.Uid, tag.ToString()));
        }

        foreach (var tag in tagsToRemove)
        {
            await tagService.DeletePostTagAsync(new(updatePostDto.Uid, tag));
        }

        var finalTags = await tagService.GetAllTagsFromPostAsync(updatePostDto.Uid);
        var finalPostWithTags = postWithTags with { Tags = finalTags.Select(t => t.ToString()).ToList() };

        return mapper.Map<PostWithTagsDto>(finalPostWithTags);
    }

    public async Task DeletePostAsync(Guid postUid)
    {
        await postRepository.DeleteAsync(postUid);
    }

    private static PostBo MapToBo(PostWithTagsBo postWithTagsBo) => new(postWithTagsBo.Uid,
        postWithTagsBo.Timestamp,
        postWithTagsBo.Title,
        postWithTagsBo.Description,
        postWithTagsBo.Content,
        postWithTagsBo.CreatorUid,
        postWithTagsBo.PrivacyLevel
    );

    private static PostWithTagsBo MapToBo(PostBo postPo, IReadOnlyList<Tag> tags) => new(postPo.Uid,
        postPo.Timestamp,
        postPo.Title,
        postPo.Description,
        postPo.Content,
        postPo.CreatorUid,
        postPo.PrivacyLevel,
        tags.Select(t => t.ToString()).ToList()
    );

    public async Task<IReadOnlyList<string>> GetPostPrivaciesAsync()
    {
        return await postRepository.GetPostPrivaciesAsync();
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
                        : new List<Tag>();
                    return MapToBo(post, tags);
                }
            )
            .ToList();
    }
}
