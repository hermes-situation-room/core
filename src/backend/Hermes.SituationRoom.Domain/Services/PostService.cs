namespace Hermes.SituationRoom.Domain.Services;

using Data.Entities;
using Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Shared.Enums;

public class PostService(IPostRepository postRepository, ITagService tagService) : IPostService
{
    public async Task<PostWithTagsBo> GetPostAsync(Guid postUid)
    {
        var post = await postRepository.GetPostBoAsync(postUid);
        var tags = await tagService.GetAllTagsFromPostAsync(postUid);
        return MapToBo(post, tags);
    }

    public Task<IReadOnlyList<PostWithTagsBo>> GetActivistPostsByTagsAsync(string tags, int limit, int offset, string? query = null, string? sortBy = null) =>
        postRepository.GetActivistPostsByTagsAsync(tags.Split(',').Select(TagService.MapToTag).ToList(), limit, offset, query, sortBy);

    public Task<IReadOnlyList<PostWithTagsBo>> GetJournalistPostsByTagsAsync(string tags, int limit, int offset, string? query = null, string? sortBy = null) =>
        postRepository.GetJournalistPostsByTagsAsync(tags.Split(',').Select(TagService.MapToTag).ToList(), limit, offset, query, sortBy);

    public async Task<IReadOnlyList<PostWithTagsBo>> GetUserPostsAsync(Guid userUid, int limit, int offset, string? query = null, string? sortBy = null) =>
        await GetTagsFromPosts(await postRepository.GetUserPostsAsync(userUid, limit, offset, query, sortBy));

    public async Task<IReadOnlyList<PostWithTagsBo>> GetAllActivistPostsAsync(int limit, int offset, string? query = null, string? sortBy = null) =>
        await GetTagsFromPosts(await postRepository.GetAllActivistPostsAsync(limit, offset, query, sortBy));

    public async Task<IReadOnlyList<PostWithTagsBo>> GetAllJournalistPostsAsync(int limit, int offset, string? query = null, string? sortBy = null) =>
        await GetTagsFromPosts(await postRepository.GetAllJournalistPostsAsync(limit, offset, query, sortBy));

    public async Task<Guid> CreatePostAsync(CreatePostDto createPostDto)
    {
        var guid = await postRepository.AddAsync(MapToBo(createPostDto, DateTime.UtcNow));
        
        foreach (var tag in createPostDto.Tags)
        {
            await tagService.CreatePostTagAsync(new(guid, tag));
        }
        
        return guid;
    }

    public async Task<PostWithTagsBo> UpdatePostAsync(PostWithTagsBo updatedPost)
    {
        await postRepository.UpdateAsync(MapToBo(updatedPost));

        var existingTags = await tagService.GetAllTagsFromPostAsync(updatedPost.Uid);

        var existingSet = existingTags?.ToHashSet() ?? new HashSet<Tag>();
        var updatedSet = updatedPost.Tags.Select(TagService.MapToTag).ToList();

        var tagsToAdd = updatedSet.Except(existingSet).ToList();
        var tagsToRemove = existingSet.Except(updatedSet).ToList();

        foreach (var tag in tagsToAdd)
        {
            await tagService.CreatePostTagAsync(new(updatedPost.Uid, tag.ToString()));
        }

        foreach (var tag in tagsToRemove)
        {
            await tagService.DeletePostTagAsync(new(updatedPost.Uid, tag));
        }

        var finalTags = await tagService.GetAllTagsFromPostAsync(updatedPost.Uid);

        return updatedPost with { Tags = finalTags.Select(t => t.ToString()).ToList() };
    }

    public async Task DeletePostAsync(Guid postUid)
    {
        await postRepository.DeleteAsync(postUid);
    }

    private static PostBo MapToBo(CreatePostDto createPostDto, DateTime timestamp) => new(createPostDto.Uid,
        timestamp,
        createPostDto.Title,
        createPostDto.Description,
        createPostDto.Content,
        createPostDto.CreatorUid
    );

    private static PostBo MapToBo(PostWithTagsBo postWithTagsPo) => new(postWithTagsPo.Uid,
        postWithTagsPo.Timestamp,
        postWithTagsPo.Title,
        postWithTagsPo.Description,
        postWithTagsPo.Content,
        postWithTagsPo.CreatorUid
    );

    private static PostWithTagsBo MapToBo(PostBo postPo, IReadOnlyList<Tag> tags) => new(postPo.Uid,
        postPo.Timestamp,
        postPo.Title,
        postPo.Description,
        postPo.Content,
        postPo.CreatorUid,
        tags.Select(t => t.ToString()).ToList()
    );

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
