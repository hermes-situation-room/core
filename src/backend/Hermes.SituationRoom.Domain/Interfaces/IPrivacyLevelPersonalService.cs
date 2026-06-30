namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.DataTransferObjects;

public interface IPrivacyLevelPersonalService
{
    Task<PrivacyLevelPersonalDto> GetPrivacyLevelPersonalAsync(Guid ownerUid, Guid consumerUid);

    Task<Guid> CreatePrivacyLevelPersonalAsync(CreatePrivacyLevelPersonalRequestDto createPrivacyLevelPersonalDto);

    Task<PrivacyLevelPersonalDto> UpdatePrivacyLevelPersonalAsync(Guid callerUid, UpdatePrivacyLevelPersonalRequestDto updatePrivacyLevelPersonalDto);

    Task DeletePrivacyLevelPersonalAsync(Guid callerUid, Guid privacyLevelPersonalUid);
}
