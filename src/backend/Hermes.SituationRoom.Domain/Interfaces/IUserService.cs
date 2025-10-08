namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.BusinessObjects;

public interface IUserService
{
    Task<UserBo> GetUserAsync(Guid userUid);

    Task<IReadOnlyList<UserBo>> GetUsersAsync();

    Task<Guid> CreateUserAsync(UserBo userBo);

    Task<UserBo> UpdateUserAsync(UserBo updatedUser);

    Task DeleteUserAsync(Guid userUid);
}
