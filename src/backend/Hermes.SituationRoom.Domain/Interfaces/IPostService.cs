namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.BusinessObjects;
using Shared.DataTransferObjects;

public interface IPostService
{
    Task<PostWithTagsBo> GetPostAsync(Guid postUid);
    
    Task<IReadOnlyList<PostWithTagsBo>> GetActivistPostsByTagsAsync(string tags, int limit, int offset);
    
    Task<IReadOnlyList<PostWithTagsBo>> GetJournalistPostsByTagsAsync(string tags, int limit, int offset);
    
    Task<IReadOnlyList<PostWithTagsBo>> GetUserPostsAsync(Guid userUid, int limit, int offset);
    
    Task<IReadOnlyList<PostWithTagsBo>> GetAllActivistPostsAsync(int limit, int offset);
    
    Task<IReadOnlyList<PostWithTagsBo>> GetAllJournalistPostsAsync(int limit, int offset);

    Task<Guid> CreatePostAsync(CreatePostDto createPostBo);

    Task<PostWithTagsBo> UpdatePostAsync(PostWithTagsBo updatedPost);

    Task DeletePostAsync(Guid postUid);
}
