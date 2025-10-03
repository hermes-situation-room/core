#nullable enable
namespace Hermes.SituationRoom.Data.Repositories;

using Entities;
using Interface;
using Microsoft.EntityFrameworkCore;
using Shared.BusinessObjects;

public sealed class ActivistRepository(IHermessituationRoomContext context, IUserRepository userRepository)
    : IActivistRepository
{
    public async Task<Guid> AddAsync(ActivistBo activistBo)
    {
        ArgumentNullException.ThrowIfNull(activistBo);

        var userUid = await userRepository.AddAsync(activistBo);

        var activist = new Activist
        {
            UserUid = userUid,
            Username = activistBo.UserName,
            IsFirstNameVisible = activistBo.IsFirstNameVisible,
            IsLastNameVisible = activistBo.IsLastNameVisible,
            IsEmailVisible = activistBo.IsEmailVisible
        };

        context.Activists.Add(activist);
        await context.SaveChangesAsync();

        return userUid;
    }

    public async Task<ActivistBo> GetActivistBoAsync(Guid activistUid)
    {
        if (activistUid == Guid.Empty)
            throw new ArgumentException("GUID must not be empty.", nameof(activistUid));

        var activist = await context.Activists
                           .AsNoTracking()
                           .Include(a => a.UserU)
                           .FirstOrDefaultAsync(a => a.UserUid == activistUid)
                       ?? throw new KeyNotFoundException($"Activist with UID {activistUid} was not found.");

        return MapToBo(activist);
    }

    public async Task<IReadOnlyList<ActivistBo>> GetAllActivistBosAsync()
    {
        var items = await context.Activists
            .AsNoTracking()
            .Include(a => a.UserU)
            .Select(a => MapToBo(a))
            .ToListAsync();

        return items;
    }

    public async Task<ActivistBo> UpdateAsync(ActivistBo updatedActivist)
    {
        ArgumentNullException.ThrowIfNull(updatedActivist);
        if (updatedActivist.Uid == Guid.Empty)
            throw new ArgumentException("UID required.", nameof(updatedActivist));

        var activist = await context.Activists
                           .Include(a => a.UserU)
                           .FirstOrDefaultAsync(a => a.UserUid == updatedActivist.Uid)
                       ?? throw new KeyNotFoundException($"Activist with UID {updatedActivist.Uid} was not found.");

        activist.UserU.Password = updatedActivist.Password;
        activist.UserU.FirstName = updatedActivist.FirstName;
        activist.UserU.LastName = updatedActivist.LastName;
        activist.UserU.EmailAddress = updatedActivist.EmailAddress;

        activist.Username = updatedActivist.UserName;
        activist.IsFirstNameVisible = updatedActivist.IsFirstNameVisible;
        activist.IsLastNameVisible = updatedActivist.IsLastNameVisible;
        activist.IsEmailVisible = updatedActivist.IsEmailVisible;

        await context.SaveChangesAsync();

        return MapToBo(activist);
    }

    public async Task DeleteAsync(Guid activistUid)
    {
        if (activistUid == Guid.Empty)
            throw new ArgumentException("GUID must not be empty.", nameof(activistUid));

        var activist = await context.Activists.FirstOrDefaultAsync(a => a.UserUid == activistUid);
        if (activist is not null)
        {
            context.Activists.Remove(activist);
        }

        var user = await context.Users.FirstOrDefaultAsync(u => u.Uid == activistUid);
        if (user is not null)
        {
            context.Users.Remove(user);
        }

        await context.SaveChangesAsync();
    }

    private static ActivistBo MapToBo(Activist activist)
    {
        var u = activist.UserU ?? throw new InvalidOperationException("Expected navigation User to be loaded.");

        return new(u.Uid,
            u.Password,
            u.FirstName,
            u.LastName,
            u.EmailAddress,
            activist.Username,
            activist.IsFirstNameVisible,
            activist.IsLastNameVisible,
            activist.IsEmailVisible
        );
    }
}
