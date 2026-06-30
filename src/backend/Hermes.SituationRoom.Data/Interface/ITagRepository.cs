namespace Hermes.SituationRoom.Data.Interface;

using Shared.BusinessObjects;

public interface ITagRepository
{
    Task AddAsync(TagBo tagBo);

    Task<IReadOnlyList<string>> GetAllTagsFromPostAsync(Guid postUid);

    Task<Dictionary<Guid, IReadOnlyList<string>>> GetAllTagsFromPostsAsync(IEnumerable<Guid> postUids);

    Task<IReadOnlyList<string>> GetAllNamesAsync();

    Task<bool> ExistsByNameAsync(string name);

    Task AddTagAsync(string name);

    Task DeleteAsync(TagBo tagBo);
}
