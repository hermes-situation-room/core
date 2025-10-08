#nullable enable
namespace Hermes.SituationRoom.Data.Interface;

using Shared.BusinessObjects;

public interface IPostRepository
{
    Task<Guid> AddAsync(PostBo postBo);

    Task<PostBo> GetPostBoAsync(Guid postUid);
    
    Task<IReadOnlyList<PostBo>> GetUserPostsAsync(Guid userUid);
    
    Task<IReadOnlyList<PostBo>> GetAllActivistPostsAsync();
    
    Task<IReadOnlyList<PostBo>> GetAllJournalistPostsAsync();

    Task<IReadOnlyList<PostBo>> GetAllPostBosAsync();

    Task<PostBo> UpdateAsync(PostBo updatedPost);

    Task DeleteAsync(Guid postUid);
}
