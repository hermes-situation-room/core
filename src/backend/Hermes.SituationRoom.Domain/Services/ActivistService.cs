namespace Hermes.SituationRoom.Domain.Services;

using Hermes.SituationRoom.Data.Interface;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Hermes.SituationRoom.Shared.DataTransferObjects; 
using Interfaces;

public class ActivistService(IActivistRepository activistRepository, IEncryptionService encryptionService) : IActivistService
{
    public async Task<ActivistBo> GetActivistAsync(Guid activistUid)
    {
        var activist = await activistRepository.GetActivistBoAsync(activistUid);

        return activist with { Password = null, PasswordHash = null, PasswordSalt = null };
    }

    public async Task<IReadOnlyList<ActivistBo>> GetActivistsAsync()
    {
        var activists = await activistRepository.GetAllActivistBosAsync();

        return [.. activists.Select((activist) => activist with { Password = null, PasswordHash = null, PasswordSalt = null })];
    } 

    public Task<Guid> CreateActivistAsync(ActivistBo activistBo) 
    {
        (byte[] hash, byte[] salt) = encryptionService.EncryptPassword(activistBo.Password);

        activistBo = activistBo with { PasswordHash = hash, PasswordSalt = salt };

        return activistRepository.AddAsync(activistBo); 
    }
    
    public Task<Guid?> FindActivistIdByUsernameAsync(string username) =>
        activistRepository.FindActivistIdByUsernameAsync(username);

    public async Task<ActivistBo> UpdateActivistAsync(ActivistBo updatedActivist)
    {
        var activist = await activistRepository.UpdateAsync(updatedActivist);

        return activist with { Password = null, PasswordHash = null, PasswordSalt = null };
    }

    public async Task<ActivistBo> UpdateActivistVisibilityAsync(Guid activistUid, UpdateActivistPrivacyLevelDto updateActivistPrivacyLevelDto)
    {
        var activist = await activistRepository.UpdateActivistVisibilityAsync(activistUid, updateActivistPrivacyLevelDto);

        return activist with { Password = null, PasswordHash = null, PasswordSalt = null };
    }

    public Task DeleteActivistAsync(Guid activistUid) => activistRepository.DeleteAsync(activistUid);
}
