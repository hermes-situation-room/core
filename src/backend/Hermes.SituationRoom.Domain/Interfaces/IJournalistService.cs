namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.DataTransferObjects;

public interface IJournalistService
{
    Task<JournalistDto> GetJournalistAsync(Guid journalistUid);

    Task<IReadOnlyList<JournalistDto>> GetJournalistsAsync();

    Task<Guid> CreateJournalistAsync(CreateJournalistRequestDto createJournalistDto);

    Task<JournalistDto> UpdateJournalistAsync(UpdateJournalistRequestDto updateJournalistDto);

    Task DeleteJournalistAsync(Guid journalistUid);
}
