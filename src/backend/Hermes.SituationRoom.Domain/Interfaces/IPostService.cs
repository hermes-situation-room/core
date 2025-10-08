namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.BusinessObjects;
using Shared.DataTransferObjects;

public interface IPostService
{
    Task<PostBo> GetPostAsync(Guid postUid);
    
    Task<IReadOnlyList<PostBo>> GetUserPostsAsync(Guid userUid);
    
    Task<IReadOnlyList<PostBo>> GetAllActivistPostsAsync();
    
    Task<IReadOnlyList<PostBo>> GetAllJournalistPostsAsync();

    Task<IReadOnlyList<PostBo>> GetPostsAsync();

    Task<Guid> CreatePostAsync(CreatePostDto createPostBo);

    Task<PostBo> UpdatePostAsync(PostBo updatedPost);

    Task DeletePostAsync(Guid postUid);
}
