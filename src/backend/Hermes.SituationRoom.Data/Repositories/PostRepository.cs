#nullable enable
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
            PrivacyLevel = postBo.PrivacyLevel,
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

    public async Task<IReadOnlyList<PostWithTagsBo>> GetActivistPostsByTagsAsync(IReadOnlyList<Tag> tags, int privacyLevel, Guid userUid, int limit, int offset, string? query = null, string? sortBy = null)
    {
        var tagSet = tags.Distinct().Select(t => t.ToString()).ToList();

        var queryable = context.Posts
            .AsNoTracking()
            .Where(p => p.PostTags.Any(pt => tagSet.Contains(pt.Tag)) &&
                        p.PrivacyLevel <= privacyLevel &&
                        context.Activists.Any(a => a.UserUid == p.CreatorUid) ||
                        p.CreatorUid == userUid);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowerQuery = query.ToLower();
            queryable = queryable.Where(p =>
                p.Title.ToLower().Contains(lowerQuery) ||
                p.Description.ToLower().Contains(lowerQuery));
        }

        queryable = ApplySorting(queryable, sortBy);

        var posts = await queryable
            .Include(p => p.PostTags)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        return posts.Select(MapToBoWithTags).ToList();
    }

    public async Task<IReadOnlyList<PostWithTagsBo>> GetJournalistPostsByTagsAsync(IReadOnlyList<Tag> tags, int limit, int offset, string? query = null, string? sortBy = null)
    {
        var tagSet = tags.Distinct().Select(t => t.ToString()).ToList();

        var queryable = context.Posts
            .AsNoTracking()
            .Where(p => p.PostTags.Any(pt => tagSet.Contains(pt.Tag)) &&
                       context.Journalists.Any(j => j.UserUid == p.CreatorUid));

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowerQuery = query.ToLower();
            queryable = queryable.Where(p =>
                p.Title.ToLower().Contains(lowerQuery) ||
                p.Description.ToLower().Contains(lowerQuery));
        }

        queryable = ApplySorting(queryable, sortBy);

        var posts = await queryable
            .Include(p => p.PostTags)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        return posts.Select(MapToBoWithTags).ToList();
    }

    public async Task<IReadOnlyList<PostBo>> GetUserPostsAsync(Guid userUid, int limit, int offset, string? query = null, string? sortBy = null)
    {
        var queryable = context.Posts
            .AsNoTracking()
            .Where(u => u.CreatorUid == userUid);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowerQuery = query.ToLower();
            queryable = queryable.Where(p =>
                p.Title.ToLower().Contains(lowerQuery) ||
                p.Description.ToLower().Contains(lowerQuery));
        }

        queryable = ApplySorting(queryable, sortBy);

        return await queryable
            .Skip(offset)
            .Take(limit)
            .Select(u => MapToBo(u))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PostBo>> GetAllActivistPostsAsync(int privacyLevel, Guid userUid, int limit, int offset, string? query = null, string? sortBy = null)
    {
        var queryable = context.Posts
            .AsNoTracking()
            .Where(p => context.Activists.Any(a => a.UserUid == p.CreatorUid) && p.PrivacyLevel <= privacyLevel || p.CreatorUid == userUid);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowerQuery = query.ToLower();
            queryable = queryable.Where(p =>
                p.Title.ToLower().Contains(lowerQuery) || p.Description.ToLower().Contains(lowerQuery)
            );
        }

        queryable = ApplySorting(queryable, sortBy);

        return await queryable
            .Skip(offset)
            .Take(limit)
            .Select(p => MapToBo(p))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PostBo>> GetAllJournalistPostsAsync(int limit, int offset, string? query = null, string? sortBy = null)
    {
        var queryable = context.Posts
            .AsNoTracking()
            .Where(p => context.Journalists.Any(j => j.UserUid == p.CreatorUid));

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowerQuery = query.ToLower();
            queryable = queryable.Where(p =>
                p.Title.ToLower().Contains(lowerQuery) ||
                p.Description.ToLower().Contains(lowerQuery));
        }

        queryable = ApplySorting(queryable, sortBy);

        return await queryable
            .Skip(offset)
            .Take(limit)
            .Select(p => MapToBo(p))
            .ToListAsync();
    }

    public async Task<PostBo> UpdateAsync(PostBo updatedPost)
    {
        ArgumentNullException.ThrowIfNull(updatedPost);
        if (updatedPost.Uid == Guid.Empty)
            throw new ArgumentException("UID required.", nameof(updatedPost));

        var post = await context.Posts.AsTracking().FirstOrDefaultAsync(u => u.Uid == updatedPost.Uid)
                   ?? throw new KeyNotFoundException($"Post with UID {updatedPost.Uid} was not found.");

        post.Title = updatedPost.Title;
        post.Description = updatedPost.Description;
        post.Content = updatedPost.Content;
        post.PrivacyLevel = updatedPost.PrivacyLevel;

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

    public async Task<IReadOnlyList<string>> GetPostPrivaciesAsync()
    {
        return [.. Enum.GetNames<PostPrivacy>()];
    }

    private static PostBo MapToBo(Post post) => new(post.Uid,
        post.Timestamp,
        post.Title,
        post.Description,
        post.Content,
        post.CreatorUid,
        post.PrivacyLevel
    );

    private static PostWithTagsBo MapToBoWithTags(Post post) => new(post.Uid,
        post.Timestamp,
        post.Title,
        post.Description,
        post.Content,
        post.CreatorUid,
        post.PrivacyLevel,
        post.PostTags.Select(pt => pt.Tag).Distinct().ToList()
    );

    private static IQueryable<Post> ApplySorting(IQueryable<Post> queryable, string? sortBy)
    {
        return sortBy switch
        {
            "oldest" => queryable.OrderBy(p => p.Timestamp),
            "title-asc" => queryable.OrderBy(p => p.Title),
            "title-desc" => queryable.OrderByDescending(p => p.Title),
            _ => queryable.OrderByDescending(p => p.Timestamp) // "newest" or default
        };
    }
}
