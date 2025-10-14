namespace Hermes.SituationRoom.Domain.Services;

using System.Security.Cryptography;
using Hermes.SituationRoom.Data.Interface;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Interfaces;

public class ActivistService(IActivistRepository activistRepository, IEncryptionService encryptionService) : IActivistService
{
    public Task<ActivistBo> GetActivistAsync(Guid activistUid) => activistRepository.GetActivistBoAsync(activistUid);

    public Task<IReadOnlyList<ActivistBo>> GetActivistsAsync() => activistRepository.GetAllActivistBosAsync();

    public Task<Guid> CreateActivistAsync(ActivistBo activistBo) 
    {
        (byte[] hash, byte[] salt) = encryptionService.EncryptPassword(activistBo.Password);

        activistBo = activistBo with { PasswordHash = hash, PasswordSalt = salt };

        return activistRepository.AddAsync(activistBo); 
    }

    public Task<ActivistBo> UpdateActivistAsync(ActivistBo updatedActivist) =>
        activistRepository.UpdateAsync(updatedActivist);

    public Task DeleteActivistAsync(Guid activistUid) => activistRepository.DeleteAsync(activistUid);
}
