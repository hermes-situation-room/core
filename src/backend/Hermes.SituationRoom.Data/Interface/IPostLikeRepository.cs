namespace Hermes.SituationRoom.Data.Interface;

using Shared.BusinessObjects;

public interface IPostLikeRepository
{
    Task<Guid> AddAsync(PostLikeBo postLikeBo);
    Task<PostLikeBo?> GetAsync(Guid postUid, Guid userUid);
    Task<int> GetLikeCountAsync(Guid postUid);
    Task<IReadOnlyList<PostLikeBo>> GetPostLikesAsync(Guid postUid);
    Task DeleteAsync(Guid postUid, Guid userUid);
    Task<bool> HasUserLikedPostAsync(Guid postUid, Guid userUid);
}

