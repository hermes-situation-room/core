namespace Hermes.SituationRoom.Domain.Services;

using AutoMapper;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Hermes.SituationRoom.Shared.DataTransferObjects;
using Hermes.SituationRoom.Data.Interface;
using Interfaces;

public class PrivacyLevelPersonalService(IPrivacyLevelPersonalRepository privacyLevelPersonalRepository, IMapper mapper) : IPrivacyLevelPersonalService
{
    public async Task<PrivacyLevelPersonalDto> GetPrivacyLevelPersonalAsync(Guid ownerUid, Guid consumerUid)
    {
        var privacyLevel = await privacyLevelPersonalRepository.GetPrivacyLevelPersonalBoAsync(ownerUid, consumerUid);
        return mapper.Map<PrivacyLevelPersonalDto>(privacyLevel);
    }

    public async Task<Guid> CreatePrivacyLevelPersonalAsync(CreatePrivacyLevelPersonalRequestDto createPrivacyLevelPersonalDto)
    {
        var privacyLevelBo = mapper.Map<PrivacyLevelPersonalBo>(createPrivacyLevelPersonalDto);
        return await privacyLevelPersonalRepository.AddAsync(privacyLevelBo);
    }

    public async Task<PrivacyLevelPersonalDto> UpdatePrivacyLevelPersonalAsync(UpdatePrivacyLevelPersonalRequestDto updatePrivacyLevelPersonalDto)
    {
        var existingPrivacyLevel = await privacyLevelPersonalRepository.GetPrivacyLevelPersonalBoAsync(updatePrivacyLevelPersonalDto.Uid);
        var privacyLevelBo = new PrivacyLevelPersonalBo(
            updatePrivacyLevelPersonalDto.Uid,
            updatePrivacyLevelPersonalDto.IsFirstNameVisible,
            updatePrivacyLevelPersonalDto.IsLastNameVisible,
            updatePrivacyLevelPersonalDto.IsEmailVisible,
            existingPrivacyLevel.OwnerUid,
            existingPrivacyLevel.ConsumerUid
        );
        var updatedPrivacyLevel = await privacyLevelPersonalRepository.UpdateAsync(privacyLevelBo);
        return mapper.Map<PrivacyLevelPersonalDto>(updatedPrivacyLevel);
    }

    public Task DeletePrivacyLevelPersonalAsync(Guid privacyLevelPersonalUid) =>
        privacyLevelPersonalRepository.DeleteAsync(privacyLevelPersonalUid);
}
