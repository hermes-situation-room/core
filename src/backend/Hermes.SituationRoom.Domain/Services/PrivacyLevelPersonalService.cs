namespace Hermes.SituationRoom.Domain.Services;

using Hermes.SituationRoom.Shared.BusinessObjects;
using Hermes.SituationRoom.Data.Interface;

public interface IPrivacyLevelPersonalService
{
    Task<PrivacyLevelPersonalBo> GetPrivacyLevelPersonalAsync(Guid ownerUid, Guid consumerUid);

    Task<Guid> CreatePrivacyLevelPersonalAsync(PrivacyLevelPersonalBo privacyLevelPersonalBo);

    Task<PrivacyLevelPersonalBo> UpdatePrivacyLevelPersonalAsync(PrivacyLevelPersonalBo updatedPrivacyLevelPersonal);

    Task DeletePrivacyLevelPersonalAsync(Guid privacyLevelPersonalUid);
}

public class PrivacyLevelPersonalService(IPrivacyLevelPersonalRepository privacyLevelPersonalRepository) : IPrivacyLevelPersonalService
{
    public Task<PrivacyLevelPersonalBo> GetPrivacyLevelPersonalAsync(Guid ownerUid, Guid consumerUid) => privacyLevelPersonalRepository.GetPrivacyLevelPersonalBoAsync(ownerUid, consumerUid);

    public Task<Guid> CreatePrivacyLevelPersonalAsync(PrivacyLevelPersonalBo privacyLevelPersonalBo) => privacyLevelPersonalRepository.AddAsync(privacyLevelPersonalBo);

    public Task<PrivacyLevelPersonalBo> UpdatePrivacyLevelPersonalAsync(PrivacyLevelPersonalBo updatedPrivacyLevelPersonal) => privacyLevelPersonalRepository.Update(updatedPrivacyLevelPersonal);

    public Task DeletePrivacyLevelPersonalAsync(Guid privacyLevelPersonalUid)
    {
        privacyLevelPersonalRepository.Delete(privacyLevelPersonalUid);
        return Task.CompletedTask;
    }
}
