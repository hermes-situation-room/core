namespace Hermes.SituationRoom.Data.Repositories;

using Entities;
using Interface;
using Microsoft.EntityFrameworkCore;
using Shared.BusinessObjects;

public sealed class PostLikeRepository(IHermessituationRoomContext context) : IPostLikeRepository
{
    public async Task<Guid> AddAsync(PostLikeBo postLikeBo)
    {
        ArgumentNullException.ThrowIfNull(postLikeBo);

        var newPostLike = new PostLike
        {
            Uid = Guid.NewGuid(),
            PostUid = postLikeBo.PostUid,
            UserUid = postLikeBo.UserUid,
            Timestamp = DateTime.UtcNow
        };

        context.PostLikes.Add(newPostLike);
        await context.SaveChangesAsync();

        return newPostLike.Uid;
    }

    public async Task<PostLikeBo?> GetAsync(Guid postUid, Guid userUid)
    {
        var postLike = await context.PostLikes
            .AsNoTracking()
            .FirstOrDefaultAsync(pl => pl.PostUid == postUid && pl.UserUid == userUid);

        return postLike == null ? null : MapToBo(postLike);
    }

    public async Task<int> GetLikeCountAsync(Guid postUid)
    {
        return await context.PostLikes
            .Where(pl => pl.PostUid == postUid)
            .CountAsync();
    }

    public async Task<IReadOnlyList<PostLikeBo>> GetPostLikesAsync(Guid postUid)
    {
        return await context.PostLikes
            .AsNoTracking()
            .Where(pl => pl.PostUid == postUid)
            .OrderByDescending(pl => pl.Timestamp)
            .Select(pl => MapToBo(pl))
            .ToListAsync();
    }

    public async Task DeleteAsync(Guid postUid, Guid userUid)
    {
        var postLike = await context.PostLikes
            .FirstOrDefaultAsync(pl => pl.PostUid == postUid && pl.UserUid == userUid);

        if (postLike != null)
        {
            context.PostLikes.Remove(postLike);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> HasUserLikedPostAsync(Guid postUid, Guid userUid)
    {
        return await context.PostLikes
            .AnyAsync(pl => pl.PostUid == postUid && pl.UserUid == userUid);
    }

    private static PostLikeBo MapToBo(PostLike postLike) => new(
        postLike.Uid,
        postLike.PostUid,
        postLike.UserUid,
        postLike.Timestamp
    );
}

