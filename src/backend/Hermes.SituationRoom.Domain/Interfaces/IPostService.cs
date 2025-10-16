#nullable enable
namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.DataTransferObjects;

public interface IPostService
{
    Task<PostWithTagsDto> GetPostAsync(Guid postUid);

    Task<IReadOnlyList<PostWithTagsDto>> GetActivistPostsByTagsAsync(string tags, Guid userUid, string? userRole, int limit, int offset, string? query = null, string? sortBy = null);

    Task<IReadOnlyList<PostWithTagsDto>> GetJournalistPostsByTagsAsync(string tags, int limit, int offset, string? query = null, string? sortBy = null);

    Task<IReadOnlyList<PostWithTagsDto>> GetUserPostsAsync(Guid userUid, int limit, int offset, string? query = null, string? sortBy = null);

    Task<IReadOnlyList<PostWithTagsDto>> GetAllActivistPostsAsync(Guid userUid, string? userRole, int limit, int offset, string? query = null, string? sortBy = null);

    Task<IReadOnlyList<PostWithTagsDto>> GetAllJournalistPostsAsync(int limit, int offset, string? query = null, string? sortBy = null);

    Task<Guid> CreatePostAsync(CreatePostRequestDto createPostDto);

    Task<PostWithTagsDto> UpdatePostAsync(UpdatePostRequestDto updatePostDto);

    Task DeletePostAsync(Guid postUid);

    Task<IReadOnlyList<string>> GetPostPrivaciesAsync();
}
