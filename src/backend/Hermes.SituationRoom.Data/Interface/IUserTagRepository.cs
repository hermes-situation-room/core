namespace Hermes.SituationRoom.Data.Interface;

using Shared.BusinessObjects;

public interface IUserTagRepository
{
    Task<Guid> AddAsync(UserTagBo userTagBo);
    Task<UserTagBo?> GetByUidAsync(Guid uid);
    Task<IReadOnlyList<UserTagBo>> GetByOwnerAsync(Guid ownerUid);
    Task<IReadOnlyList<UserTagBo>> GetByOwnerAndTaggedAsync(Guid ownerUid, Guid taggedUserUid);
    Task<IReadOnlyList<UserTagBo>> SearchByTagNameAsync(Guid ownerUid, string tagName);
    Task UpdateAsync(UserTagBo userTagBo);
    Task DeleteAsync(Guid uid, Guid ownerUid);
}
