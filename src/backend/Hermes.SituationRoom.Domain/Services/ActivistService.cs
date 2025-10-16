namespace Hermes.SituationRoom.Domain.Services;

using AutoMapper;
using Hermes.SituationRoom.Data.Interface;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Hermes.SituationRoom.Shared.DataTransferObjects; 
using Interfaces;

public class ActivistService(IActivistRepository activistRepository, IEncryptionService encryptionService, IMapper mapper) : IActivistService
{
    public async Task<ActivistDto> GetActivistAsync(Guid activistUid)
    {
        var activist = await activistRepository.GetActivistBoAsync(activistUid);
        var activistWithoutSensitiveData = activist with { Password = null, PasswordHash = null, PasswordSalt = null };
        return mapper.Map<ActivistDto>(activistWithoutSensitiveData);
    }

    public async Task<IReadOnlyList<ActivistDto>> GetActivistsAsync()
    {
        var activists = await activistRepository.GetAllActivistBosAsync();
        var activistsWithoutSensitiveData = activists.Select((activist) => activist with { Password = null, PasswordHash = null, PasswordSalt = null }).ToList();
        return mapper.Map<IReadOnlyList<ActivistDto>>(activistsWithoutSensitiveData);
    } 

    public Task<Guid> CreateActivistAsync(CreateActivistRequestDto createActivistDto) 
    {
        var activistBo = mapper.Map<ActivistBo>(createActivistDto);
        (byte[] hash, byte[] salt) = encryptionService.EncryptPassword(createActivistDto.Password);
        activistBo = activistBo with { PasswordHash = hash, PasswordSalt = salt };
        return activistRepository.AddAsync(activistBo); 
    }
    
    public Task<Guid?> FindActivistIdByUsernameAsync(string username) =>
        activistRepository.FindActivistIdByUsernameAsync(username);

    public async Task<ActivistDto> UpdateActivistAsync(UpdateActivistRequestDto updateActivistDto)
    {
        var activistBo = mapper.Map<ActivistBo>(updateActivistDto);
        var activist = await activistRepository.UpdateAsync(activistBo);
        var activistWithoutSensitiveData = activist with { Password = null, PasswordHash = null, PasswordSalt = null };
        return mapper.Map<ActivistDto>(activistWithoutSensitiveData);
    }

    public async Task<ActivistDto> UpdateActivistVisibilityAsync(Guid activistUid, UpdateActivistPrivacyLevelRequestDto updateActivistPrivacyLevelRequestDto)
    {
        var activist = await activistRepository.UpdateActivistVisibilityAsync(
            activistUid, 
            updateActivistPrivacyLevelRequestDto.IsFirstNameVisible,
            updateActivistPrivacyLevelRequestDto.IsLastNameVisible,
            updateActivistPrivacyLevelRequestDto.IsEmailVisible
        );
        var activistWithoutSensitiveData = activist with { Password = null, PasswordHash = null, PasswordSalt = null };
        return mapper.Map<ActivistDto>(activistWithoutSensitiveData);
    }

    public Task DeleteActivistAsync(Guid activistUid) => activistRepository.DeleteAsync(activistUid);
}
