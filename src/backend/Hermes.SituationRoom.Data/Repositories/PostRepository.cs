namespace Hermes.SituationRoom.Data.Repositories;

using Entities;
using Interface;
using Microsoft.EntityFrameworkCore;
using Shared.BusinessObjects;
using Shared.Enums;

public sealed class PostRepository(IHermessituationRoomContext context) : IPostRepository
{
    public async Task<Guid> AddAsync(PostBo postBo)
    {
        ArgumentNullException.ThrowIfNull(postBo);

        var newPost = new Post
        {
            Uid = Guid.NewGuid(),
            Timestamp = postBo.Timestamp,
            CreatorUid = postBo.CreatorUid,
            Title = postBo.Title,
            Description = postBo.Description,
            Content = postBo.Content,
        };

        context.Posts.Add(newPost);
        await context.SaveChangesAsync();

        return newPost.Uid;
    }

    public async Task<PostBo> GetPostBoAsync(Guid postId)
    {
        if (postId == Guid.Empty)
            throw new ArgumentException("GUID must not be empty.", nameof(postId));

        return MapToBo(await context.Posts
                           .AsNoTracking()
                           .FirstOrDefaultAsync(u => u.Uid == postId)
                       ?? throw new KeyNotFoundException($"Post with UID {postId} was not found.")
        );
    }

    public async Task<IReadOnlyList<PostWithTagsBo>> GetPostsByTagsAsync(IReadOnlyList<Tag> tags)
    {
        var tagSet = tags.Distinct().Select(t => t.ToString()).ToList();

        var posts = await context.Posts
            .AsNoTracking()
            .Where(p => p.PostTags.Any(pt => tagSet.Contains(pt.Tag)))
            .Include(p => p.PostTags)
            .ToListAsync();

        return posts.Select(MapToBoWithTags).ToList();
    }

    public async Task<IReadOnlyList<PostWithTagsBo>> GetActivistPostsByTagsAsync(IReadOnlyList<Tag> tags)
    {
        var tagSet = tags.Distinct().Select(t => t.ToString()).ToList();

        var posts = await context.Posts
            .AsNoTracking()
            .Where(p => p.PostTags.Any(pt => tagSet.Contains(pt.Tag)) &&
                       context.Activists.Any(a => a.UserUid == p.CreatorUid))
            .Include(p => p.PostTags)
            .ToListAsync();

        return posts.Select(MapToBoWithTags).ToList();
    }

    public async Task<IReadOnlyList<PostWithTagsBo>> GetJournalistPostsByTagsAsync(IReadOnlyList<Tag> tags)
    {
        var tagSet = tags.Distinct().Select(t => t.ToString()).ToList();

        var posts = await context.Posts
            .AsNoTracking()
            .Where(p => p.PostTags.Any(pt => tagSet.Contains(pt.Tag)) &&
                       context.Journalists.Any(j => j.UserUid == p.CreatorUid))
            .Include(p => p.PostTags)
            .ToListAsync();

        return posts.Select(MapToBoWithTags).ToList();
    }

    public async Task<IReadOnlyList<PostBo>> GetUserPostsAsync(Guid userUid) => await context.Posts
        .AsNoTracking()
        .Where(u => u.CreatorUid == userUid)
        .Select(u => MapToBo(u))
        .ToListAsync();

    public async Task<IReadOnlyList<PostBo>> GetAllActivistPostsAsync() => await context.Posts
        .AsNoTracking()
        .Where(p => context.Activists.Any(a => a.UserUid == p.CreatorUid))
        .OrderByDescending(p => p.Timestamp)
        .Select(p => MapToBo(p))
        .ToListAsync();

    public async Task<IReadOnlyList<PostBo>> GetAllJournalistPostsAsync() => await context.Posts
        .AsNoTracking()
        .Where(p => context.Journalists.Any(j => j.UserUid == p.CreatorUid))
        .OrderByDescending(p => p.Timestamp)
        .Select(p => MapToBo(p))
        .ToListAsync();

    public async Task<IReadOnlyList<PostBo>> GetAllPostBosAsync() => await context.Posts
        .AsNoTracking()
        .Select(u => MapToBo(u))
        .ToListAsync();

    public async Task<PostBo> UpdateAsync(PostBo updatedPost)
    {
        ArgumentNullException.ThrowIfNull(updatedPost);
        if (updatedPost.Uid == Guid.Empty)
            throw new ArgumentException("UID required.", nameof(updatedPost));

        var post = await context.Posts.FirstOrDefaultAsync(u => u.Uid == updatedPost.Uid)
                   ?? throw new KeyNotFoundException($"Post with UID {updatedPost.Uid} was not found.");

        post.Title = updatedPost.Title;
        post.Description = updatedPost.Description;
        post.Content = updatedPost.Content;

        context.Posts.Update(post);
        await context.SaveChangesAsync();

        return MapToBo(post);
    }

    public async Task DeleteAsync(Guid postId)
    {
        if (postId == Guid.Empty)
            throw new ArgumentException("GUID must not be empty.", nameof(postId));

        var post = context.Posts.FirstOrDefault(u => u.Uid == postId);
        if (post is null)
            return;

        context.Posts.Remove(post);
        await context.SaveChangesAsync();
    }

    private static PostBo MapToBo(Post post) => new(post.Uid,
        post.Timestamp,
        post.Title,
        post.Description,
        post.Content,
        post.CreatorUid
    );

    private static PostWithTagsBo MapToBoWithTags(Post post) => new(post.Uid,
        post.Timestamp,
        post.Title,
        post.Description,
        post.Content,
        post.CreatorUid,
        post.PostTags.Select(pt => pt.Tag).Distinct().ToList()
    );
}
