namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Shared.EnumClasses;

public interface ITagService
{
    Task CreatePostTagAsync(CreateTagDto tagBo);

    Task<IReadOnlyList<Tag>> GetAllTagsFromPostAsync(Guid postUid);
    
    Task<Dictionary<Guid, IReadOnlyList<Tag>>> GetAllTagsFromPostsAsync(IEnumerable<Guid> postUids);
    
    IReadOnlyList<string> GetAllTags();

    Task DeletePostTagAsync(TagBo tagBo);
}
