namespace Hermes.SituationRoom.Data.Interface;

using Shared.BusinessObjects;

public interface ITagRequestRepository
{
    Task<Guid> AddAsync(TagRequestBo tagRequestBo);
    Task<TagRequestBo?> GetByIdAsync(Guid uid);
    Task<IReadOnlyList<TagRequestBo>> GetByRequesterAsync(Guid requestedByUid);
    Task<IReadOnlyList<TagRequestBo>> GetByStatusAsync(string status);
    Task<IReadOnlyList<string>> GetApprovedNamesAsync();
    Task<bool> ExistsByNameAsync(string name);
    Task<TagRequestBo> UpdateAsync(TagRequestBo tagRequestBo);
}
