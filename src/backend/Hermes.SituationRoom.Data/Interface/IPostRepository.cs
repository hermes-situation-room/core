#nullable enable
namespace Hermes.SituationRoom.Data.Interface;

using Shared.BusinessObjects;
using Shared.Enums;

public interface IPostRepository
{
    Task<Guid> AddAsync(PostBo postBo);

    Task<PostBo> GetPostBoAsync(Guid postUid);

    Task<IReadOnlyList<PostWithTagsBo>> GetActivistPostsByTagsAsync(IReadOnlyList<Tag> tags, int privacyLevel, Guid userUid, int limit, int offset, string? query = null, string? sortBy = null);

    Task<IReadOnlyList<PostWithTagsBo>> GetJournalistPostsByTagsAsync(IReadOnlyList<Tag> tags, int limit, int offset, string? query = null, string? sortBy = null);

    Task<IReadOnlyList<PostBo>> GetUserPostsAsync(Guid userUid, int privacyLevel, Guid loggedInUserUid, int limit, int offset, string? query = null, string? sortBy = null);

    Task<IReadOnlyList<PostBo>> GetAllActivistPostsAsync(int privacyLevel, Guid userUid, int limit, int offset, string? query = null, string? sortBy = null);

    Task<IReadOnlyList<PostBo>> GetAllJournalistPostsAsync(int limit, int offset, string? query = null, string? sortBy = null);

    Task<PostBo> UpdateAsync(PostBo updatedPost);

    Task DeleteAsync(Guid postUid);

    Task<IReadOnlyList<string>> GetPostPrivaciesAsync();
}
