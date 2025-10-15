namespace Hermes.SituationRoom.Domain.Services;

using Hermes.SituationRoom.Data.Entities;
using Hermes.SituationRoom.Data.Interface;
using Hermes.SituationRoom.Data.Repositories;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Interfaces;

public class JournalistService(IJournalistRepository journalistRepository, IEncryptionService encryptionService) : IJournalistService
{
    public async Task<JournalistBo> GetJournalistAsync(Guid journalistUid)
    {
        var journalist = await journalistRepository.GetJournalistBoAsync(journalistUid);

        return journalist with { Password = null, PasswordHash = null, PasswordSalt = null };
    }

    public async Task<IReadOnlyList<JournalistBo>> GetJournalistsAsync()
    {
        var journalists = await journalistRepository.GetAllJournalistBosAsync();
        return [ ..journalists.Select(journalist => journalist with { Password = null, PasswordHash = null, PasswordSalt = null })];
    }

    public Task<Guid> CreateJournalistAsync(JournalistBo journalistBo) 
    {
        (byte[] hash, byte[] salt) = encryptionService.EncryptPassword(journalistBo.Password);

        journalistBo = journalistBo with { PasswordHash = hash, PasswordSalt = salt };

        return journalistRepository.AddAsync(journalistBo);
    }

    public async Task<JournalistBo> UpdateJournalistAsync(JournalistBo updatedJournalist) {
        var journalist = await journalistRepository.UpdateAsync(updatedJournalist);
        return journalist with { Password = null, PasswordHash = null, PasswordSalt = null };
    }

    public Task DeleteJournalistAsync(Guid journalistUid) => journalistRepository.DeleteAsync(journalistUid);
}
