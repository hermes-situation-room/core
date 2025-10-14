namespace Hermes.SituationRoom.Domain.Services;

using Data.Interface;
using System.Security.Cryptography;
using Hermes.SituationRoom.Data.Interface;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Hermes.SituationRoom.Shared.DataTransferObjects; 
using Interfaces;
using Shared.BusinessObjects;
using Shared.Exceptions;

public class ActivistService(IActivistRepository activistRepository, IEncryptionService encryptionService) : IActivistService
{
    public Task<ActivistBo> GetActivistAsync(Guid activistUid) => activistRepository.GetActivistBoAsync(activistUid);

    public Task<IReadOnlyList<ActivistBo>> GetActivistsAsync() => activistRepository.GetAllActivistBosAsync();

    public async Task<Guid> CreateActivistAsync(ActivistBo activistBo) 
    {
        if (await activistRepository.UsernameExistsAsync(activistBo.UserName))
        {
            throw new DuplicateResourceException("Activist", "username", activistBo.UserName);
        }

        (byte[] hash, byte[] salt) = encryptionService.EncryptPassword(activistBo.Password);

        activistBo = activistBo with { PasswordHash = hash, PasswordSalt = salt };

        return await activistRepository.AddAsync(activistBo); 
    }

    public Task<ActivistBo> UpdateActivistAsync(ActivistBo updatedActivist) =>
        activistRepository.UpdateAsync(updatedActivist);

    public Task<ActivistBo> UpdateActivistVisibilityAsync(Guid activistUid, UpdateActivistPrivacyLevelDto updateActivistPrivacyLevelDto) =>
        activistRepository.UpdateActivistVisibilityAsync(activistUid, updateActivistPrivacyLevelDto);

    public Task DeleteActivistAsync(Guid activistUid) => activistRepository.DeleteAsync(activistUid);
}
