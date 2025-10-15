namespace Hermes.SituationRoom.Domain.Services;

using System.Net.Mail;
using Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.Exceptions;

public class UserService(IUserRepository userRepository, IEncryptionService encryptionService) : IUserService
{
    public async Task<UserBo> GetUserAsync(Guid userUid)
    {
        if (userUid == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userUid));

        return await userRepository.GetUserBoAsync(userUid);
    }

    public async Task<UserProfileBo> GetUserProfileAsync(Guid userUid, Guid consumerUid)
    {
        if (userUid == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userUid));

        return await userRepository.GetUserProfileBoAsync(userUid, consumerUid);
    }

    public async Task<string> GetDisplayNameAsync(Guid userUid)
    {
        if (userUid == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userUid));

        return await userRepository.GetDisplayNameAsync(userUid);
    }

    public Task<IReadOnlyList<UserBo>> GetUsersAsync() => userRepository.GetAllUserBosAsync();

    public async Task<Guid> CreateUserAsync(UserBo userBo) 
    {
        ArgumentNullException.ThrowIfNull(userBo, nameof(userBo));

        if (string.IsNullOrWhiteSpace(userBo.FirstName))
            throw new BusinessValidationException(nameof(userBo.FirstName), "First name is required.");

        if (string.IsNullOrWhiteSpace(userBo.LastName))
            throw new BusinessValidationException(nameof(userBo.LastName), "Last name is required.");

        if (string.IsNullOrWhiteSpace(userBo.EmailAddress))
            throw new BusinessValidationException(nameof(userBo.EmailAddress), "Email address is required.");

        if (!IsValidEmail(userBo.EmailAddress))
            throw new BusinessValidationException(nameof(userBo.EmailAddress), "Email address format is invalid.");

        if (string.IsNullOrWhiteSpace(userBo.Password))
            throw new BusinessValidationException(nameof(userBo.Password), "Password is required.");

        if (userBo.Password.Length < 8)
            throw new BusinessValidationException(nameof(userBo.Password), "Password must be at least 8 characters long.");

        if (await userRepository.EmailExistsAsync(userBo.EmailAddress))
            throw new DuplicateResourceException("User", "email", userBo.EmailAddress);

        (byte[] hash, byte[] salt) = encryptionService.EncryptPassword(userBo.Password);

        userBo = userBo with { PasswordHash = hash, PasswordSalt = salt };

        return await userRepository.AddAsync(userBo);
    }

    public async Task<UserBo> UpdateUserAsync(UserBo updatedUser)
    {
        ArgumentNullException.ThrowIfNull(updatedUser, nameof(updatedUser));

        if (updatedUser.Uid == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(updatedUser.Uid));

        if (string.IsNullOrWhiteSpace(updatedUser.FirstName))
            throw new BusinessValidationException(nameof(updatedUser.FirstName), "First name is required.");

        if (string.IsNullOrWhiteSpace(updatedUser.LastName))
            throw new BusinessValidationException(nameof(updatedUser.LastName), "Last name is required.");

        if (string.IsNullOrWhiteSpace(updatedUser.EmailAddress))
            throw new BusinessValidationException(nameof(updatedUser.EmailAddress), "Email address is required.");

        if (!IsValidEmail(updatedUser.EmailAddress))
            throw new BusinessValidationException(nameof(updatedUser.EmailAddress), "Email address format is invalid.");

        var existingUser = await userRepository.GetUserBoAsync(updatedUser.Uid);
        if (existingUser.EmailAddress != updatedUser.EmailAddress)
        {
            if (await userRepository.EmailExistsAsync(updatedUser.EmailAddress))
                throw new DuplicateResourceException("User", "EmailAddress", updatedUser.EmailAddress);
        }

        return await userRepository.Update(updatedUser);
    }

    public async Task DeleteUserAsync(Guid userUid)
    {
        if (userUid == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userUid));

        await userRepository.Delete(userUid);
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
