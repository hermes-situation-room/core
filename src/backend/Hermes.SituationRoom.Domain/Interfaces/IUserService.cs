namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.DataTransferObjects;

public interface IUserService
{
    Task<UserDto> GetUserAsync(Guid userUid);

    Task<UserProfileDto> GetUserProfileAsync(Guid userUid, Guid consumerUid);

    Task<string> GetDisplayNameAsync(Guid userUid);

    Task<IReadOnlyList<UserDto>> GetUsersAsync();
    
    Task<Guid> GetUserIdByEmailOrUsernameAsync(string identificationString);

    Task<Guid> CreateUserAsync(CreateUserRequestDto createUserDto);

    Task<UserDto> UpdateUserAsync(UpdateUserRequestDto updateUserDto);

    Task DeleteUserAsync(Guid userUid);
}
