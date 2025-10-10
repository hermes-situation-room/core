namespace Hermes.SituationRoom.Data.Interface;

using Entities;
using Shared.BusinessObjects;
using Shared.Enums;

public interface ITagRepository
{
    Task AddAsync(TagBo tagBo);

    Task<IReadOnlyList<Tag>> GetAllTagsFromPostAsync(Guid postUid);
    
    Task<Dictionary<Guid, IReadOnlyList<Tag>>> GetAllTagsFromPostsAsync(IEnumerable<Guid> postUids);
    
    IReadOnlyList<Tag> GetAllTags();

    Task DeleteAsync(TagBo tagBo);
}
