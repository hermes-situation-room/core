#nullable enable
namespace Hermes.SituationRoom.Data.Repositories;

using System.Diagnostics;
using Entities;
using Interface;
using Microsoft.EntityFrameworkCore;
using Shared.BusinessObjects;
using Hermes.SituationRoom.Shared.Exceptions;

public sealed class JournalistRepository(IHermessituationRoomContext context, IUserRepository userRepository)
    : IJournalistRepository
{
    public async Task<Guid> AddAsync(JournalistBo journalistBo)
    {
        ArgumentNullException.ThrowIfNull(journalistBo);

        var validationErrors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(journalistBo.FirstName))
            validationErrors["FirstName"] = new[] { "First name is required for journalists." };

        if (string.IsNullOrWhiteSpace(journalistBo.LastName))
            validationErrors["LastName"] = new[] { "Last name is required for journalists." };

        if (string.IsNullOrWhiteSpace(journalistBo.EmailAddress))
            validationErrors["EmailAddress"] = new[] { "Email address is required for journalists." };

        if (validationErrors.Count > 0)
            throw new ValidationBusinessException(validationErrors);

        var userUid = await userRepository.AddAsync(journalistBo);

        var journalist = new Journalist { UserUid = userUid, Employer = journalistBo.Employer, };

        context.Journalists.Add(journalist);
        await context.SaveChangesAsync();

        return userUid;
    }

    public async Task<JournalistBo> GetJournalistBoAsync(Guid journalistUid)
    {
        if (journalistUid == Guid.Empty)
            throw new ArgumentException("GUID must not be empty.", nameof(journalistUid));

        return MapToBo(await context.Journalists
                           .AsNoTracking()
                           .Include(j => j.UserU)
                           .FirstOrDefaultAsync(j => j.UserUid == journalistUid)
                       ?? throw new ResourceNotFoundException("Journalist", journalistUid.ToString())
        );
    }

    public async Task<IReadOnlyList<JournalistBo>> GetAllJournalistBosAsync() => await context.Journalists
        .AsNoTracking()
        .Include(j => j.UserU)
        .Select(j => MapToBo(j))
        .ToListAsync();

    public async Task<JournalistBo> UpdateAsync(JournalistBo updatedJournalist)
    {
        ArgumentNullException.ThrowIfNull(updatedJournalist);
        if (updatedJournalist.Uid == Guid.Empty)
            throw new ArgumentException("UID required.", nameof(updatedJournalist));

        var journalist = await context.Journalists
                             .AsTracking()
                             .Include(j => j.UserU)
                             .FirstOrDefaultAsync(j => j.UserUid == updatedJournalist.Uid)
                         ?? throw new ResourceNotFoundException("Journalist", updatedJournalist.Uid.ToString());

        var validationErrors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(updatedJournalist.FirstName))
            validationErrors["FirstName"] = new[] { "First name is required for journalists." };

        if (string.IsNullOrWhiteSpace(updatedJournalist.LastName))
            validationErrors["LastName"] = new[] { "Last name is required for journalists." };

        if (string.IsNullOrWhiteSpace(updatedJournalist.EmailAddress))
            validationErrors["EmailAddress"] = new[] { "Email address is required for journalists." };

        if (validationErrors.Count > 0)
            throw new ValidationBusinessException(validationErrors);

        if (journalist.UserU.EmailAddress != updatedJournalist.EmailAddress)
        {
            var existingUserWithEmail = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.EmailAddress == updatedJournalist.EmailAddress && u.Uid != updatedJournalist.Uid);

            if (existingUserWithEmail is not null)
            {
                throw new DuplicateResourceException("User", updatedJournalist.EmailAddress, 
                    "A user with this email address already exists.");
            }
        }

        journalist.UserU.FirstName = updatedJournalist.FirstName;
        journalist.UserU.LastName = updatedJournalist.LastName;
        journalist.UserU.EmailAddress = updatedJournalist.EmailAddress;
        
        if (updatedJournalist.ProfileIcon is not null)
            journalist.UserU.ProfileIcon = updatedJournalist.ProfileIcon;
        if (updatedJournalist.ProfileIconColor is not null)
            journalist.UserU.ProfileIconColor = updatedJournalist.ProfileIconColor;

        journalist.Employer = updatedJournalist.Employer;

        await context.SaveChangesAsync();

        return MapToBo(journalist);
    }

    public async Task DeleteAsync(Guid journalistUid)
    {
        if (journalistUid == Guid.Empty)
            throw new ArgumentException("GUID must not be empty.", nameof(journalistUid));

        var journalist = await context.Journalists.FirstOrDefaultAsync(j => j.UserUid == journalistUid);
        if (journalist is not null)
        {
            context.Journalists.Remove(journalist);
        }

        var user = await context.Users.FirstOrDefaultAsync(u => u.Uid == journalistUid);
        if (user is not null)
        {
            context.Users.Remove(user);
        }

        await context.SaveChangesAsync();
    }

    private static JournalistBo MapToBo(Journalist journalist)
    {
        var user = journalist.UserU ?? throw new InvalidOperationException("Expected navigation UserU to be loaded.");

        return new(user.Uid,
            null,
            user.PasswordHash,
            user.PasswordSalt,
            user.FirstName,
            user.LastName,
            user.EmailAddress,
            user.ProfileIcon,
            user.ProfileIconColor,
            journalist.Employer
        );
    }
}
