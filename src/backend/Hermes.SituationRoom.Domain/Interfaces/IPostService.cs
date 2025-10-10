namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.BusinessObjects;
using Shared.DataTransferObjects;

public interface IPostService
{
    Task<PostWithTagsBo> GetPostAsync(Guid postUid);
    
    Task<IReadOnlyList<PostWithTagsBo>> GetPostsByTagsAsync(string tags);
    
    Task<IReadOnlyList<PostWithTagsBo>> GetActivistPostsByTagsAsync(string tags);
    
    Task<IReadOnlyList<PostWithTagsBo>> GetJournalistPostsByTagsAsync(string tags);
    
    Task<IReadOnlyList<PostWithTagsBo>> GetUserPostsAsync(Guid userUid);
    
    Task<IReadOnlyList<PostWithTagsBo>> GetAllActivistPostsAsync();
    
    Task<IReadOnlyList<PostWithTagsBo>> GetAllJournalistPostsAsync();

    Task<IReadOnlyList<PostWithTagsBo>> GetPostsAsync();

    Task<Guid> CreatePostAsync(CreatePostDto createPostBo);

    Task<PostWithTagsBo> UpdatePostAsync(PostWithTagsBo updatedPost);

    Task DeletePostAsync(Guid postUid);
}
