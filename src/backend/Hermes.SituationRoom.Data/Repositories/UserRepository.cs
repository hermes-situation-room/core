#nullable enable
namespace Hermes.SituationRoom.Data.Repositories;

using Entities;
using Interface;
using Microsoft.EntityFrameworkCore;
using Shared.BusinessObjects;
using Hermes.SituationRoom.Shared.Exceptions;

public sealed class UserRepository(IHermessituationRoomContext context) : IUserRepository
{
    public async Task<Guid> AddAsync(UserBo userBo)
    {
        ArgumentNullException.ThrowIfNull(userBo);

        if (!string.IsNullOrWhiteSpace(userBo.EmailAddress))
        {
            if (!IsValidEmail(userBo.EmailAddress))
            {
                throw new ValidationBusinessException("EmailAddress", "Invalid email address format.");
            }

            var existingUserWithEmail = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.EmailAddress == userBo.EmailAddress);

            if (existingUserWithEmail is not null)
            {
                throw new DuplicateResourceException("User", userBo.EmailAddress, 
                    "A user with this email address already exists.");
            }
        }

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
                       ?? throw new ResourceNotFoundException("User", userId.ToString())
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
                       ?? throw new ResourceNotFoundException("User", emailAddress)
        );
    }

    public async Task<UserProfileBo> GetUserProfileBoAsync(Guid userId, Guid consumerId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("GUID must not be empty.", nameof(userId));

        var userProfileBo = MapToUserProfileBo(await context.Users
                                                   .AsNoTracking()
                                                   .FirstOrDefaultAsync(u => u.Uid == userId)
                                               ?? throw new ResourceNotFoundException("User", userId.ToString())
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
                   ?? throw new ResourceNotFoundException("User", userId.ToString());

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
                   ?? throw new ResourceNotFoundException("User", updatedUser.Uid.ToString());

        if (!string.IsNullOrWhiteSpace(updatedUser.EmailAddress) && 
            user.EmailAddress != updatedUser.EmailAddress)
        {
            if (!IsValidEmail(updatedUser.EmailAddress))
            {
                throw new ValidationBusinessException("EmailAddress", "Invalid email address format.");
            }

            var existingUserWithEmail = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.EmailAddress == updatedUser.EmailAddress && u.Uid != updatedUser.Uid);

            if (existingUserWithEmail is not null)
            {
                throw new DuplicateResourceException("User", updatedUser.EmailAddress, 
                    "A user with this email address already exists.");
            }
        }

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
                    FirstName = privacyLevel.IsFirstNameVisible != null ? userProfileBo.FirstName : "[REDACTED]",
                    LastName = privacyLevel.IsLastNameVisible != null ? userProfileBo.LastName : "[REDACTED]",
                    EmailAddress = privacyLevel.IsEmailVisible != null ? userProfileBo.EmailAddress : "[REDACTED]"
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

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
