#nullable enable
namespace Hermes.SituationRoom.Data.Repositories;

using Entities;
using Interface;
using Microsoft.EntityFrameworkCore;
using Shared.BusinessObjects;
using Hermes.SituationRoom.Shared.Exceptions;

public sealed class ActivistRepository(IHermessituationRoomContext context, IUserRepository userRepository)
    : IActivistRepository
{
    public async Task<Guid> AddAsync(ActivistBo activistBo)
    {
        ArgumentNullException.ThrowIfNull(activistBo);

        if (string.IsNullOrWhiteSpace(activistBo.UserName))
        {
            throw new ValidationBusinessException("UserName", "Username is required for activists.");
        }

        var existingActivist = await context.Activists
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Username == activistBo.UserName);

        if (existingActivist is not null)
        {
            throw new DuplicateResourceException("Activist", activistBo.UserName, 
                "An activist with this username already exists.");
        }

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

        return MapToBo(await context.Activists
                           .AsNoTracking()
                           .Include(a => a.UserU)
                           .FirstOrDefaultAsync(a => a.UserUid == activistUid)
                       ?? throw new ResourceNotFoundException("Activist", activistUid.ToString())
        );
    }
    
    public async Task<Guid?> FindActivistIdByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        return await context.Activists
            .AsNoTracking()
            .Where(a => a.Username == username)
            .Select(a => (Guid?)a.UserUid)
            .FirstOrDefaultAsync();
    }

    public async Task<ActivistBo> GetActivistBoByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username must not be empty.", nameof(username));

        return MapToBo(await context.Activists
                           .AsNoTracking()
                           .Include(a => a.UserU)
                           .FirstOrDefaultAsync(a => a.Username == username)
                       ?? throw new UnauthorizedAccessException("Invalid password or username.")
        );
    }

    public async Task<IReadOnlyList<ActivistBo>> GetAllActivistBosAsync() => await context.Activists
        .AsNoTracking()
        .Include(a => a.UserU)
        .Select(a => MapToBo(a))
        .ToListAsync();

    public async Task<ActivistBo> UpdateAsync(ActivistBo updatedActivist)
    {
        ArgumentNullException.ThrowIfNull(updatedActivist);
        if (updatedActivist.Uid == Guid.Empty)
            throw new ArgumentException("UID required.", nameof(updatedActivist));

        var activist = await context.Activists
                           .AsTracking()
                           .Include(a => a.UserU)
                           .FirstOrDefaultAsync(a => a.UserUid == updatedActivist.Uid)
                       ?? throw new ResourceNotFoundException("Activist", updatedActivist.Uid.ToString());

        if (!string.IsNullOrWhiteSpace(updatedActivist.UserName) && 
            activist.Username != updatedActivist.UserName)
        {
            var existingActivist = await context.Activists
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Username == updatedActivist.UserName && a.UserUid != updatedActivist.Uid);

            if (existingActivist is not null)
            {
                throw new DuplicateResourceException("Activist", updatedActivist.UserName, 
                    "An activist with this username already exists.");
            }
        }

        if (!string.IsNullOrWhiteSpace(updatedActivist.EmailAddress) &&
            activist.UserU.EmailAddress != updatedActivist.EmailAddress)
        {
            var existingUserWithEmail = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.EmailAddress == updatedActivist.EmailAddress && u.Uid != updatedActivist.Uid);

            if (existingUserWithEmail is not null)
            {
                throw new DuplicateResourceException("User", updatedActivist.EmailAddress, 
                    "A user with this email address already exists.");
            }
        }

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

    public async Task<ActivistBo> UpdateActivistVisibilityAsync(Guid activistUid, bool isFirstNameVisible, bool isLastNameVisible, bool isEmailVisible)
    {
        if (activistUid == Guid.Empty)
            throw new ArgumentException("UID required.", nameof(activistUid));

        var activist = await context.Activists
                           .AsTracking()
                           .Include(a => a.UserU)
                           .FirstOrDefaultAsync(a => a.UserUid == activistUid)
                       ?? throw new ResourceNotFoundException("Activist", activistUid.ToString());

        activist.IsFirstNameVisible = isFirstNameVisible;
        activist.IsLastNameVisible = isLastNameVisible;
        activist.IsEmailVisible = isEmailVisible;

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
        var user = activist.UserU ?? throw new InvalidOperationException("Expected navigation User to be loaded.");

        return new(user.Uid,
            null,
            user.PasswordHash,
            user.PasswordSalt,
            user.FirstName,
            user.LastName,
            user.EmailAddress,
            activist.Username,
            activist.IsFirstNameVisible,
            activist.IsLastNameVisible,
            activist.IsEmailVisible
        );
    }
}
