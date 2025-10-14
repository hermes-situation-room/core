namespace Hermes.SituationRoom.Domain.Services;

using Hermes.SituationRoom.Data.Entities;
using Hermes.SituationRoom.Data.Interface;
using Hermes.SituationRoom.Data.Repositories;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Interfaces;

public class UserService(IUserRepository userRepository, IEncryptionService encryptionService) : IUserService
{
    public Task<UserBo> GetUserAsync(Guid userUid) => userRepository.GetUserBoAsync(userUid);

    public Task<UserBo> GetUserProfileAsync(Guid userUid, Guid consumerUid) => userRepository.GetUserProfileBoAsync(userUid, consumerUid);

    public Task<IReadOnlyList<UserBo>> GetUsersAsync() => userRepository.GetAllUserBosAsync();

    public Task<Guid> CreateUserAsync(UserBo userBo) 
    {
        (byte[] hash, byte[] salt) = encryptionService.EncryptPassword(userBo.Password);

        userBo = userBo with { PasswordHash = hash, PasswordSalt = salt };

        return userRepository.AddAsync(userBo);
    }

    public Task<UserBo> UpdateUserAsync(UserBo updatedUser) => userRepository.Update(updatedUser);

    public Task DeleteUserAsync(Guid userUid)
    {
        userRepository.Delete(userUid);
        return Task.CompletedTask;
    }
}
