namespace Hermes.SituationRoom.Domain.Services;

using System.Net.Mail;
using Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Shared.Exceptions;

public class ActivistService(IActivistRepository activistRepository, IUserRepository userRepository, IEncryptionService encryptionService) : IActivistService
{
    public async Task<ActivistBo> GetActivistAsync(Guid activistUid)
    {
        if (activistUid == Guid.Empty)
            throw new ArgumentException("Activist ID cannot be empty.", nameof(activistUid));

        return await activistRepository.GetActivistBoAsync(activistUid);
    }

    public Task<IReadOnlyList<ActivistBo>> GetActivistsAsync() => activistRepository.GetAllActivistBosAsync();

    public async Task<Guid> CreateActivistAsync(ActivistBo activistBo) 
    {
        ArgumentNullException.ThrowIfNull(activistBo, nameof(activistBo));

        if (string.IsNullOrWhiteSpace(activistBo.UserName))
            throw new BusinessValidationException(nameof(activistBo.UserName), "Username is required.");

        if (activistBo.UserName.Length < 3)
            throw new BusinessValidationException(nameof(activistBo.UserName), "Username must be at least 3 characters long.");

        if (string.IsNullOrWhiteSpace(activistBo.Password))
            throw new BusinessValidationException(nameof(activistBo.Password), "Password is required.");

        if (activistBo.Password.Length < 8)
            throw new BusinessValidationException(nameof(activistBo.Password), "Password must be at least 8 characters long.");


        if (await activistRepository.UsernameExistsAsync(activistBo.UserName))
        {
            throw new DuplicateResourceException("Activist", "username", activistBo.UserName);
        }

        if (!string.IsNullOrWhiteSpace(activistBo.EmailAddress) && 
            await userRepository.EmailExistsAsync(activistBo.EmailAddress))
        {
            throw new DuplicateResourceException("User", "EmailAddress", activistBo.EmailAddress);
        }

        (byte[] hash, byte[] salt) = encryptionService.EncryptPassword(activistBo.Password);

        activistBo = activistBo with { PasswordHash = hash, PasswordSalt = salt };

        return await activistRepository.AddAsync(activistBo); 
    }

    public async Task<ActivistBo> UpdateActivistAsync(ActivistBo updatedActivist)
    {
        ArgumentNullException.ThrowIfNull(updatedActivist, nameof(updatedActivist));

        if (updatedActivist.Uid == Guid.Empty)
            throw new ArgumentException("Activist ID cannot be empty.", nameof(updatedActivist.Uid));

        if (string.IsNullOrWhiteSpace(updatedActivist.UserName))
            throw new BusinessValidationException(nameof(updatedActivist.UserName), "Username is required.");

        if (updatedActivist.UserName.Length < 3)
            throw new BusinessValidationException(nameof(updatedActivist.UserName), "Username must be at least 3 characters long.");


        var existingActivist = await activistRepository.GetActivistBoAsync(updatedActivist.Uid);
        if (!string.IsNullOrWhiteSpace(updatedActivist.EmailAddress) && 
            existingActivist.EmailAddress != updatedActivist.EmailAddress)
        {
            if (await userRepository.EmailExistsAsync(updatedActivist.EmailAddress))
                throw new DuplicateResourceException("User", "EmailAddress", updatedActivist.EmailAddress);
        }

        if (existingActivist.UserName != updatedActivist.UserName)
        {
            if (await activistRepository.UsernameExistsAsync(updatedActivist.UserName))
                throw new DuplicateResourceException("Activist", "username", updatedActivist.UserName);
        }

        return await activistRepository.UpdateAsync(updatedActivist);
    }

    public async Task<ActivistBo> UpdateActivistVisibilityAsync(Guid activistUid, UpdateActivistPrivacyLevelDto updateActivistPrivacyLevelDto)
    {
        ArgumentNullException.ThrowIfNull(updateActivistPrivacyLevelDto, nameof(updateActivistPrivacyLevelDto));

        if (activistUid == Guid.Empty)
            throw new ArgumentException("Activist ID cannot be empty.", nameof(activistUid));

        return await activistRepository.UpdateActivistVisibilityAsync(activistUid, updateActivistPrivacyLevelDto);
    }

    public async Task DeleteActivistAsync(Guid activistUid)
    {
        if (activistUid == Guid.Empty)
            throw new ArgumentException("Activist ID cannot be empty.", nameof(activistUid));

        await activistRepository.DeleteAsync(activistUid);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
