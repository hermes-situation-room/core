namespace Hermes.SituationRoom.Domain.Services;

using Hermes.SituationRoom.Data.Entities;
using Hermes.SituationRoom.Data.Interface;
using Hermes.SituationRoom.Data.Repositories;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Interfaces;

public class JournalistService(IJournalistRepository journalistRepository, IEncryptionService encryptionService) : IJournalistService
{
    public Task<JournalistBo> GetJournalistAsync(Guid journalistUid) =>
        journalistRepository.GetJournalistBoAsync(journalistUid);

    public Task<IReadOnlyList<JournalistBo>> GetJournalistsAsync() => journalistRepository.GetAllJournalistBosAsync();

    public Task<Guid> CreateJournalistAsync(JournalistBo journalistBo) 
    {
        (byte[] hash, byte[] salt) = encryptionService.EncryptPassword(journalistBo.Password);

        journalistBo = journalistBo with { PasswordHash = hash, PasswordSalt = salt };

        return journalistRepository.AddAsync(journalistBo);
    }

    public Task<JournalistBo> UpdateJournalistAsync(JournalistBo updatedJournalist) =>
        journalistRepository.UpdateAsync(updatedJournalist);

    public Task DeleteJournalistAsync(Guid journalistUid) => journalistRepository.DeleteAsync(journalistUid);
}
