namespace Hermes.SituationRoom.Domain.Services;

using Hermes.SituationRoom.Data.Entities;
using Hermes.SituationRoom.Data.Interface;
using Hermes.SituationRoom.Data.Repositories;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Interfaces;

public class UserService(IUserRepository userRepository, IActivistRepository activistRepository, IEncryptionService encryptionService) : IUserService
{
    public async Task<UserBo> GetUserAsync(Guid userUid) {
        var user = await userRepository.GetUserBoAsync(userUid);
        return user with { Password = null, PasswordHash = null, PasswordSalt = null };
    }

    public Task<UserProfileBo> GetUserProfileAsync(Guid userUid, Guid consumerUid) => userRepository.GetUserProfileBoAsync(userUid, consumerUid);

    public Task<string> GetDisplayNameAsync(Guid userUid) => userRepository.GetDisplayNameAsync(userUid);

    public async Task<IReadOnlyList<UserBo>> GetUsersAsync()
    {
        var users = await userRepository.GetAllUserBosAsync();
        return [ ..users.Select(user => user with {Password = null, PasswordHash = null, PasswordSalt = null})];
    }

    public Task<Guid> CreateUserAsync(UserBo userBo) 
    {
        (byte[] hash, byte[] salt) = encryptionService.EncryptPassword(userBo.Password);

        userBo = userBo with { PasswordHash = hash, PasswordSalt = salt };

        return userRepository.AddAsync(userBo);
    }

    public async Task<Guid> GetUserIdByEmailOrUsernameAsync(string usernameOrEmail)
    {
        Guid? userId = await activistRepository.FindActivistIdByUsernameAsync(usernameOrEmail);
        if (userId.HasValue)
        {
            return userId.Value;
        }
        
        userId = await userRepository.FindJournalistIdByEmailAsync(usernameOrEmail);
        if (userId.HasValue)
        {
            return userId.Value;
        }

        throw new KeyNotFoundException($"No user with the username or email '{usernameOrEmail}' was found.");
    }

    public async Task<UserBo> UpdateUserAsync(UserBo updatedUser)
    {
        var user = await userRepository.UpdateAsync(updatedUser);
        return user with { Password = null, PasswordHash = null, PasswordSalt = null };
    }

    public Task DeleteUserAsync(Guid userUid) =>
        userRepository.DeleteAsync(userUid);
}
