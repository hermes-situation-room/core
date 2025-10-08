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
}
