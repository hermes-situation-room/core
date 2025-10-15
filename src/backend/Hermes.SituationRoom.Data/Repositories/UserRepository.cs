#nullable enable
namespace Hermes.SituationRoom.Data.Repositories;

using Entities;
using Interface;
using Microsoft.EntityFrameworkCore;
using Shared.BusinessObjects;

public sealed class UserRepository(IHermessituationRoomContext context) : IUserRepository
{
    public async Task<Guid> AddAsync(UserBo userBo)
    {
        ArgumentNullException.ThrowIfNull(userBo);

        var newUser = new User
        {
            Uid = Guid.NewGuid(),
            PasswordHash = userBo.PasswordHash,
            PasswordSalt = userBo.PasswordSalt,
            FirstName = userBo.FirstName,
            LastName = userBo.LastName,
            EmailAddress = userBo.EmailAddress
        };

        context.Users.Add(newUser);
        await context.SaveChangesAsync();

        return newUser.Uid;
    }

    public async Task<UserBo> GetUserBoAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("GUID must not be empty.", nameof(userId));

        return MapToBo(await context.Users
                           .AsNoTracking()
                           .FirstOrDefaultAsync(u => u.Uid == userId)
                       ?? throw new KeyNotFoundException($"User with UID {userId} was not found.")
        );
    }
    
    public async Task<Guid?> FindJournalistIdByEmailAsync(string emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress))
            return null;

        return await context.Users
            .AsNoTracking()
            .Where(u => u.EmailAddress == emailAddress && context.Journalists.Any(j => j.UserUid == u.Uid))
            .Select(u => (Guid?)u.Uid)
            .FirstOrDefaultAsync();
    }

    public async Task<UserBo> GetUserBoByEmailAsync(string emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress))
            throw new ArgumentException("Email address must not be empty.", nameof(emailAddress));

        return MapToBo(await context.Users
                           .AsNoTracking()
                           .FirstOrDefaultAsync(u => u.EmailAddress == emailAddress)
                       ?? throw new UnauthorizedAccessException("Invalid password or email address.")
        );
    }

    public async Task<UserProfileBo> GetUserProfileBoAsync(Guid userId, Guid consumerId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("GUID must not be empty.", nameof(userId));

        var userProfileBo = MapToUserProfileBo(await context.Users
                                                   .AsNoTracking()
                                                   .FirstOrDefaultAsync(u => u.Uid == userId)
                                               ?? throw new KeyNotFoundException(
                                                   $"User with UID {userId} was not found."
                                               )
        );

        userProfileBo = await ApplyUserPrivacyLevel(userProfileBo, userId, consumerId);

        return userProfileBo;
    }

    public async Task<string> GetDisplayNameAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("GUID must not be empty.", nameof(userId));

        var user = await context.Users
                       .AsNoTracking()
                       .FirstOrDefaultAsync(u => u.Uid == userId)
                   ?? throw new KeyNotFoundException($"User with UID {userId} was not found.");

        var activist = await context.Activists
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserUid == userId);

        return activist != null ? activist.Username : $"{user.FirstName} {user.LastName}";
    }

    public async Task<IReadOnlyList<UserBo>> GetAllUserBosAsync() => await context.Users
        .AsNoTracking()
        .Select(u => MapToBo(u))
        .ToListAsync();

    public async Task<UserBo> UpdateAsync(UserBo updatedUser)
    {
        ArgumentNullException.ThrowIfNull(updatedUser);
        if (updatedUser.Uid == Guid.Empty)
            throw new ArgumentException("UID required.", nameof(updatedUser));

        var user = await context.Users.AsTracking().FirstOrDefaultAsync(u => u.Uid == updatedUser.Uid)
                   ?? throw new KeyNotFoundException($"User with UID {updatedUser.Uid} was not found.");

        user.FirstName = updatedUser.FirstName;
        user.LastName = updatedUser.LastName;
        user.EmailAddress = updatedUser.EmailAddress;

        await context.SaveChangesAsync();

        return MapToBo(user);
    }
    
    public async Task DeleteAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("GUID must not be empty.", nameof(userId));

        var user = await context.Users
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Uid == userId);

        if (user is not null)
        {
            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }
    }

    private static UserBo MapToBo(User user) => new(user.Uid,
        null,
        user.PasswordHash,
        user.PasswordSalt,
        user.FirstName,
        user.LastName,
        user.EmailAddress
    );

    private static UserProfileBo MapToUserProfileBo(User user) => new(user.Uid,
        user.FirstName,
        user.LastName,
        user.EmailAddress,
        null,
        null
    );

    private async Task<UserProfileBo> ApplyUserPrivacyLevel(UserProfileBo userProfileBo, Guid userId, Guid consumerId)
    {
        var activist = await context.Activists
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserUid == userId);

        if (activist is null)
        {
            var journalist = await context.Journalists
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.UserUid == userId);

            if (journalist is not null) { userProfileBo = userProfileBo with { Employer = journalist.Employer }; }

            return userProfileBo;
        }

        if (userId == consumerId)
        {
            userProfileBo = userProfileBo with
            {
                UserName = activist.Username,
                FirstName = userProfileBo.FirstName,
                LastName = userProfileBo.LastName,
                EmailAddress = userProfileBo.EmailAddress
            };
        }
        else
        {
            var privacyLevel = await context.PrivacyLevelPersonals
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.OwnerUid == userId && p.ConsumerUid == consumerId);

            if (privacyLevel is not null)
                userProfileBo = userProfileBo with
                {
                    UserName = activist.Username,
                    FirstName = privacyLevel.IsFirstNameVisible ? userProfileBo.FirstName : "[REDACTED]",
                    LastName = privacyLevel.IsLastNameVisible ? userProfileBo.LastName : "[REDACTED]",
                    EmailAddress = privacyLevel.IsEmailVisible ? userProfileBo.EmailAddress : "[REDACTED]"
                };
            else
                userProfileBo = userProfileBo with
                {
                    UserName = activist.Username,
                    FirstName = activist.IsFirstNameVisible ? userProfileBo.FirstName : "[REDACTED]",
                    LastName = activist.IsLastNameVisible ? userProfileBo.LastName : "[REDACTED]",
                    EmailAddress = activist.IsEmailVisible ? userProfileBo.EmailAddress : "[REDACTED]"
                };
        }

        return userProfileBo;
    }
}
