namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.BusinessObjects;
using Shared.DataTransferObjects;

public interface ITagService
{
    Task CreatePostTagAsync(CreateTagDto tagBo);

    Task<IReadOnlyList<string>> GetAllTagsFromPostAsync(Guid postUid);

    Task<Dictionary<Guid, IReadOnlyList<string>>> GetAllTagsFromPostsAsync(IEnumerable<Guid> postUids);

    Task<IReadOnlyList<string>> GetAllTags();

    Task<string> ValidateTagNameAsync(string tag);

    Task DeletePostTagAsync(TagBo tagBo);
}
