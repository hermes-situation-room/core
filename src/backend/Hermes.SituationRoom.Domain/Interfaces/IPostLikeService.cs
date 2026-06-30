namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.DataTransferObjects;

public interface IPostLikeService
{
    Task<PostLikeStatsDto> GetPostLikeStatsAsync(Guid postUid, Guid? userUid);
    Task<Guid> ToggleLikeAsync(Guid postUid, Guid userUid);
    Task<IReadOnlyList<PostLikeDto>> GetPostLikesAsync(Guid postUid);
}

