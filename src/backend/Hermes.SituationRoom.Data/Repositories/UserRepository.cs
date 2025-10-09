#nullable enable
namespace Hermes.SituationRoom.Data.Repositories;

using Entities;
using Interface;
using Microsoft.EntityFrameworkCore;
using Shared.BusinessObjects;

public sealed class UserRepository(IHermessituationRoomContext context, IPrivacyLevelPersonalRepository privacyLevelPersonalRepository) : IUserRepository
{
    public async Task<Guid> AddAsync(UserBo userBo)
    {
        ArgumentNullException.ThrowIfNull(userBo);

        var newUser = new User
        {
            Uid = Guid.NewGuid(),
            Password = userBo.Password,
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

    public async Task<UserBo> GetUserProfileBoAsync(Guid userId, Guid consumerId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("GUID must not be empty.", nameof(userId));

        var userBo = MapToBo(await context.Users
                           .AsNoTracking()
                           .FirstOrDefaultAsync(u => u.Uid == userId)
                       ?? throw new KeyNotFoundException($"User with UID {userId} was not found."));

        userBo = await ApplyUserPrivacyLevel(userBo, userId, consumerId);

        return userBo;
    }

    public async Task<IReadOnlyList<UserBo>> GetAllUserBosAsync() => await context.Users
        .AsNoTracking()
        .Select(u => MapToBo(u))
        .ToListAsync();

    public async Task<UserBo> Update(UserBo updatedUser)
    {
        ArgumentNullException.ThrowIfNull(updatedUser);
        if (updatedUser.Uid == Guid.Empty)
            throw new ArgumentException("UID required.", nameof(updatedUser));

        var user = await context.Users.FirstOrDefaultAsync(u => u.Uid == updatedUser.Uid)
                   ?? throw new KeyNotFoundException($"User with UID {updatedUser.Uid} was not found.");

        user.Password = updatedUser.Password;
        user.FirstName = updatedUser.FirstName;
        user.LastName = updatedUser.LastName;
        user.EmailAddress = updatedUser.EmailAddress;

        context.Users.Update(user);
        await context.SaveChangesAsync();

        return MapToBo(user);
    }

    public Task Delete(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("GUID must not be empty.", nameof(userId));

        var user = context.Users.FirstOrDefault(u => u.Uid == userId);
        if (user is null)
            return Task.CompletedTask;

        context.Users.Remove(user);
        context.SaveChanges();

        return Task.CompletedTask;
    }

    private static UserBo MapToBo(User user) => new(user.Uid,
        user.Password,
        user.FirstName,
        user.LastName,
        user.EmailAddress
    );

    private async Task<UserBo> ApplyUserPrivacyLevel(UserBo userBo, Guid userId, Guid consumerId)
    {
        if (consumerId != Guid.Empty)
        {
            var privacyLevel = await privacyLevelPersonalRepository.GetPrivacyLevelPersonalBoAsync(userId, consumerId);

            if (!privacyLevel.IsFirstNameVisible) userBo = userBo with { FirstName = null };
            if (!privacyLevel.IsLastNameVisible) userBo = userBo with { LastName = null };
            if (!privacyLevel.IsEmailVisible) userBo = userBo with { EmailAddress = null };
        }
        else
        {
            var activist = await context.Activists
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UserUid == userId);

            if (activist is not null)
            {
                if (!activist.IsFirstNameVisible) userBo = userBo with { FirstName = null };
                if (!activist.IsLastNameVisible) userBo = userBo with { LastName = null };
                if (!activist.IsEmailVisible) userBo = userBo with { EmailAddress = null };
            }
        }

        return userBo;
    }
}
