namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.DataTransferObjects;

public interface IActivistService
{
    Task<ActivistDto> GetActivistAsync(Guid activistUid);

    Task<IReadOnlyList<ActivistDto>> GetActivistsAsync();
    
    Task<Guid?> FindActivistIdByUsernameAsync(string username);

    Task<Guid> CreateActivistAsync(CreateActivistRequestDto createActivistDto);

    Task<ActivistDto> UpdateActivistAsync(UpdateActivistRequestDto updateActivistDto);

    Task<ActivistDto> UpdateActivistVisibilityAsync(Guid activistUid, UpdateActivistPrivacyLevelRequestDto updateActivistPrivacyLevelRequestDto);

    Task DeleteActivistAsync(Guid activistUid);
}
