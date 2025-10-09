namespace Hermes.SituationRoom.Domain.Services;

using Hermes.SituationRoom.Shared.BusinessObjects;
using Hermes.SituationRoom.Data.Interface;
using Interfaces;

public class UserService(IUserRepository userRepository) : IUserService
{
    public Task<UserBo> GetUserAsync(Guid userUid) => userRepository.GetUserBoAsync(userUid);

    public Task<UserBo> GetUserProfileAsync(Guid userUid, Guid consumerUid) => userRepository.GetUserProfileBoAsync(userUid, consumerUid);

    public Task<IReadOnlyList<UserBo>> GetUsersAsync() => userRepository.GetAllUserBosAsync();

    public Task<Guid> CreateUserAsync(UserBo userBo) => userRepository.AddAsync(userBo);

    public Task<UserBo> UpdateUserAsync(UserBo updatedUser) => userRepository.Update(updatedUser);

    public Task DeleteUserAsync(Guid userUid)
    {
        userRepository.Delete(userUid);
        return Task.CompletedTask;
    }
}
