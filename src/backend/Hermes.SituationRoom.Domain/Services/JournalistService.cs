namespace Hermes.SituationRoom.Domain.Services;

using Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.Exceptions;

public class JournalistService(IJournalistRepository journalistRepository, IUserRepository userRepository, IEncryptionService encryptionService) : IJournalistService
{
    public Task<JournalistBo> GetJournalistAsync(Guid journalistUid) =>
        journalistRepository.GetJournalistBoAsync(journalistUid);

    public Task<IReadOnlyList<JournalistBo>> GetJournalistsAsync() => journalistRepository.GetAllJournalistBosAsync();

    public async Task<Guid> CreateJournalistAsync(JournalistBo journalistBo) 
    {
        if (await userRepository.EmailExistsAsync(journalistBo.EmailAddress))
        {
            throw new DuplicateResourceException("Journalist", "email", journalistBo.EmailAddress);
        }

        (byte[] hash, byte[] salt) = encryptionService.EncryptPassword(journalistBo.Password);

        journalistBo = journalistBo with { PasswordHash = hash, PasswordSalt = salt };

        return await journalistRepository.AddAsync(journalistBo);
    }

    public Task<JournalistBo> UpdateJournalistAsync(JournalistBo updatedJournalist) =>
        journalistRepository.UpdateAsync(updatedJournalist);

    public Task DeleteJournalistAsync(Guid journalistUid) => journalistRepository.DeleteAsync(journalistUid);
}
