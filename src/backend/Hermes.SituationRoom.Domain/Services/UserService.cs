namespace Hermes.SituationRoom.Domain.Services;

using AutoMapper;
using Hermes.SituationRoom.Data.Interface;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Hermes.SituationRoom.Shared.DataTransferObjects;
using Interfaces;

public class UserService(IUserRepository userRepository, IActivistRepository activistRepository, IEncryptionService encryptionService, IMapper mapper) : IUserService
{
    public async Task<UserDto> GetUserAsync(Guid userUid) 
    {
        var user = await userRepository.GetUserBoAsync(userUid);
        var userWithoutSensitiveData = user with { Password = null, PasswordHash = null, PasswordSalt = null };
        return mapper.Map<UserDto>(userWithoutSensitiveData);
    }

    public async Task<UserProfileDto> GetUserProfileAsync(Guid userUid, Guid consumerUid)
    {
        var userProfile = await userRepository.GetUserProfileBoAsync(userUid, consumerUid);
        return mapper.Map<UserProfileDto>(userProfile);
    }

    public Task<string> GetDisplayNameAsync(Guid userUid) => userRepository.GetDisplayNameAsync(userUid);

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync()
    {
        var users = await userRepository.GetAllUserBosAsync();
        var usersWithoutSensitiveData = users.Select(user => user with {Password = null, PasswordHash = null, PasswordSalt = null}).ToList();
        return mapper.Map<IReadOnlyList<UserDto>>(usersWithoutSensitiveData);
    }

    public Task<Guid> CreateUserAsync(CreateUserRequestDto createUserDto) 
    {
        var userBo = mapper.Map<UserBo>(createUserDto);
        (byte[] hash, byte[] salt) = encryptionService.EncryptPassword(createUserDto.Password);
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

    public async Task<UserDto> UpdateUserAsync(UpdateUserRequestDto updateUserDto)
    {
        var userBo = mapper.Map<UserBo>(updateUserDto);
        var user = await userRepository.UpdateAsync(userBo);
        var userWithoutSensitiveData = user with { Password = null, PasswordHash = null, PasswordSalt = null };
        return mapper.Map<UserDto>(userWithoutSensitiveData);
    }

    public Task DeleteUserAsync(Guid userUid) =>
        userRepository.DeleteAsync(userUid);
}
