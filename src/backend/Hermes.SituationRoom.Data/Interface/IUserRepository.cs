#nullable enable
namespace Hermes.SituationRoom.Data.Interface;

using Shared.BusinessObjects;

public interface IUserRepository
{
    Task<Guid> AddAsync(UserBo userBo);

    Task<UserBo> GetUserBoAsync(Guid userUid);

    Task<UserBo> GetUserBoByEmailAsync(string emailAddress);

    Task<bool> EmailExistsAsync(string emailAddress);

    Task<UserProfileBo> GetUserProfileBoAsync(Guid userUid, Guid consumerUid);

    Task<IReadOnlyList<UserBo>> GetAllUserBosAsync();

    Task<UserBo> Update(UserBo updatedUser);

    Task Delete(Guid userUid);
}
