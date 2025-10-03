namespace Hermes.SituationRoom.Domain.Services;

using Hermes.SituationRoom.Shared.BusinessObjects;
using Hermes.SituationRoom.Data.Interface;

public interface IUserService
{
    Task<UserBo> GetUserAsync(Guid userUid);

    Task<IReadOnlyList<UserBo>> GetUsersAsync();

    Task<Guid> CreateUserAsync(UserBo userBo);

    Task<UserBo> UpdateUserAsync(UserBo updatedUser);

    Task DeleteUserAsync(Guid userUid);
}

public class UserService(IUserRepository userRepository) : IUserService
{
    public Task<UserBo> GetUserAsync(Guid userUid) => userRepository.GetUserBoAsync(userUid);

    public Task<IReadOnlyList<UserBo>> GetUsersAsync() => userRepository.GetAllUserBosAsync();

    public Task<Guid> CreateUserAsync(UserBo userBo) => userRepository.AddAsync(userBo);

    public Task<UserBo> UpdateUserAsync(UserBo updatedUser) => userRepository.Update(updatedUser);

    public Task DeleteUserAsync(Guid userUid)
    {
        userRepository.Delete(userUid);
        return Task.CompletedTask;
    }
}
