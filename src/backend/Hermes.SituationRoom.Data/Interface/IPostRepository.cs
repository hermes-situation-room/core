#nullable enable
namespace Hermes.SituationRoom.Data.Interface;

using Shared.BusinessObjects;
using Shared.EnumClasses;

public interface IPostRepository
{
    Task<Guid> AddAsync(PostBo postBo);

    Task<PostBo> GetPostBoAsync(Guid postUid);
    
    Task<IReadOnlyList<PostWithTagsBo>> GetPostsByTagsAsync(IReadOnlyList<Tag> tags);
    
    Task<IReadOnlyList<PostWithTagsBo>> GetActivistPostsByTagsAsync(IReadOnlyList<Tag> tags);
    
    Task<IReadOnlyList<PostWithTagsBo>> GetJournalistPostsByTagsAsync(IReadOnlyList<Tag> tags);
    
    Task<IReadOnlyList<PostBo>> GetUserPostsAsync(Guid userUid);
    
    Task<IReadOnlyList<PostBo>> GetAllActivistPostsAsync();
    
    Task<IReadOnlyList<PostBo>> GetAllJournalistPostsAsync();

    Task<IReadOnlyList<PostBo>> GetAllPostBosAsync();

    Task<PostBo> UpdateAsync(PostBo updatedPost);

    Task DeleteAsync(Guid postUid);
}
