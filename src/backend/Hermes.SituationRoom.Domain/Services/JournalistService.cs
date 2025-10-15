namespace Hermes.SituationRoom.Domain.Services;

using Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.Exceptions;

public class JournalistService(IJournalistRepository journalistRepository, IUserRepository userRepository, IEncryptionService encryptionService) : IJournalistService
{
    public async Task<JournalistBo> GetJournalistAsync(Guid journalistUid)
    {
        if (journalistUid == Guid.Empty)
            throw new ArgumentException("Journalist ID cannot be empty.", nameof(journalistUid));

        return await journalistRepository.GetJournalistBoAsync(journalistUid);
    }

    public Task<IReadOnlyList<JournalistBo>> GetJournalistsAsync() => journalistRepository.GetAllJournalistBosAsync();

    public async Task<Guid> CreateJournalistAsync(JournalistBo journalistBo) 
    {
        ArgumentNullException.ThrowIfNull(journalistBo, nameof(journalistBo));

        if (string.IsNullOrWhiteSpace(journalistBo.FirstName))
            throw new BusinessValidationException(nameof(journalistBo.FirstName), "First name is required.");

        if (string.IsNullOrWhiteSpace(journalistBo.LastName))
            throw new BusinessValidationException(nameof(journalistBo.LastName), "Last name is required.");

        if (string.IsNullOrWhiteSpace(journalistBo.EmailAddress))
            throw new BusinessValidationException(nameof(journalistBo.EmailAddress), "Email address is required.");

        if (!IsValidEmail(journalistBo.EmailAddress))
            throw new BusinessValidationException(nameof(journalistBo.EmailAddress), "Email address format is invalid.");

        if (string.IsNullOrWhiteSpace(journalistBo.Password))
            throw new BusinessValidationException(nameof(journalistBo.Password), "Password is required.");

        if (journalistBo.Password.Length < 8)
            throw new BusinessValidationException(nameof(journalistBo.Password), "Password must be at least 8 characters long.");

        if (await userRepository.EmailExistsAsync(journalistBo.EmailAddress))
        {
            throw new DuplicateResourceException("Journalist", "email", journalistBo.EmailAddress);
        }

        (byte[] hash, byte[] salt) = encryptionService.EncryptPassword(journalistBo.Password);

        journalistBo = journalistBo with { PasswordHash = hash, PasswordSalt = salt };

        return await journalistRepository.AddAsync(journalistBo);
    }

    public async Task<JournalistBo> UpdateJournalistAsync(JournalistBo updatedJournalist)
    {
        ArgumentNullException.ThrowIfNull(updatedJournalist, nameof(updatedJournalist));

        if (updatedJournalist.Uid == Guid.Empty)
            throw new ArgumentException("Journalist ID cannot be empty.", nameof(updatedJournalist.Uid));

        if (string.IsNullOrWhiteSpace(updatedJournalist.FirstName))
            throw new BusinessValidationException(nameof(updatedJournalist.FirstName), "First name is required.");

        if (string.IsNullOrWhiteSpace(updatedJournalist.LastName))
            throw new BusinessValidationException(nameof(updatedJournalist.LastName), "Last name is required.");

        if (string.IsNullOrWhiteSpace(updatedJournalist.EmailAddress))
            throw new BusinessValidationException(nameof(updatedJournalist.EmailAddress), "Email address is required.");

        if (!IsValidEmail(updatedJournalist.EmailAddress))
            throw new BusinessValidationException(nameof(updatedJournalist.EmailAddress), "Email address format is invalid.");

        var existingJournalist = await journalistRepository.GetJournalistBoAsync(updatedJournalist.Uid);
        if (existingJournalist.EmailAddress != updatedJournalist.EmailAddress)
        {
            if (await userRepository.EmailExistsAsync(updatedJournalist.EmailAddress))
                throw new DuplicateResourceException("User", "EmailAddress", updatedJournalist.EmailAddress);
        }

        return await journalistRepository.UpdateAsync(updatedJournalist);
    }

    public async Task DeleteJournalistAsync(Guid journalistUid)
    {
        if (journalistUid == Guid.Empty)
            throw new ArgumentException("Journalist ID cannot be empty.", nameof(journalistUid));

        await journalistRepository.DeleteAsync(journalistUid);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
