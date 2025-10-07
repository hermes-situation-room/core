#nullable enable
namespace Hermes.SituationRoom.Data.Interface;

using Shared.BusinessObjects;

public interface IPostRepository
{
    Task<Guid> AddAsync(PostBo postBo);

    Task<PostBo> GetPostBoAsync(Guid postUid);
    
    Task<IReadOnlyList<PostBo>> GetUserPostsAsync(Guid userUid);

    Task<IReadOnlyList<PostBo>> GetAllPostBosAsync();

    Task<PostBo> Update(PostBo updatedPost);

    Task Delete(Guid postUid);
}
