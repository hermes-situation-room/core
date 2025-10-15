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
    public async Task<PrivacyLevelPersonalBo> GetPrivacyLevelPersonalAsync(Guid ownerUid, Guid consumerUid)
    {
        if (ownerUid == Guid.Empty)
            throw new ArgumentException("Owner ID cannot be empty.", nameof(ownerUid));

        if (consumerUid == Guid.Empty)
            throw new ArgumentException("Consumer ID cannot be empty.", nameof(consumerUid));

        return await privacyLevelPersonalRepository.GetPrivacyLevelPersonalBoAsync(ownerUid, consumerUid);
    }

    public async Task<Guid> CreatePrivacyLevelPersonalAsync(PrivacyLevelPersonalBo privacyLevelPersonalBo)
    {
        ArgumentNullException.ThrowIfNull(privacyLevelPersonalBo, nameof(privacyLevelPersonalBo));

        if (privacyLevelPersonalBo.OwnerUid == Guid.Empty)
            throw new ArgumentException("Owner ID cannot be empty.", nameof(privacyLevelPersonalBo.OwnerUid));

        if (privacyLevelPersonalBo.ConsumerUid == Guid.Empty)
            throw new ArgumentException("Consumer ID cannot be empty.", nameof(privacyLevelPersonalBo.ConsumerUid));

        return await privacyLevelPersonalRepository.AddAsync(privacyLevelPersonalBo);
    }

    public async Task<PrivacyLevelPersonalBo> UpdatePrivacyLevelPersonalAsync(PrivacyLevelPersonalBo updatedPrivacyLevelPersonal)
    {
        ArgumentNullException.ThrowIfNull(updatedPrivacyLevelPersonal, nameof(updatedPrivacyLevelPersonal));

        if (updatedPrivacyLevelPersonal.Uid == Guid.Empty)
            throw new ArgumentException("Privacy level ID cannot be empty.", nameof(updatedPrivacyLevelPersonal.Uid));

        return await privacyLevelPersonalRepository.UpdateAsync(updatedPrivacyLevelPersonal);
    }

    public async Task DeletePrivacyLevelPersonalAsync(Guid privacyLevelPersonalUid)
    {
        if (privacyLevelPersonalUid == Guid.Empty)
            throw new ArgumentException("Privacy level ID cannot be empty.", nameof(privacyLevelPersonalUid));

        await privacyLevelPersonalRepository.DeleteAsync(privacyLevelPersonalUid);
    }
}
