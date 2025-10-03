#nullable enable
namespace Hermes.SituationRoom.Data.Repositories;

using Entities;
using Interface;
using Microsoft.EntityFrameworkCore;
using Shared.BusinessObjects;

public sealed class JournalistRepository(IHermessituationRoomContext context, IUserRepository userRepository)
    : IJournalistRepository
{
    public async Task<Guid> AddAsync(JournalistBo journalistBo)
    {
        ArgumentNullException.ThrowIfNull(journalistBo);

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

        var journalist = await context.Journalists
                             .AsNoTracking()
                             .Include(j => j.UserU)
                             .FirstOrDefaultAsync(j => j.UserUid == journalistUid)
                         ?? throw new KeyNotFoundException($"Journalist with UID {journalistUid} was not found.");

        return MapToBo(journalist);
    }

    public async Task<IReadOnlyList<JournalistBo>> GetAllJournalistBosAsync()
    {
        var list = await context.Journalists
            .AsNoTracking()
            .Include(j => j.UserU)
            .Select(j => MapToBo(j))
            .ToListAsync();

        return list;
    }

    public async Task<JournalistBo> UpdateAsync(JournalistBo updatedJournalist)
    {
        ArgumentNullException.ThrowIfNull(updatedJournalist);
        if (updatedJournalist.Uid == Guid.Empty)
            throw new ArgumentException("UID required.", nameof(updatedJournalist));

        var journalist = await context.Journalists
                             .Include(j => j.UserU)
                             .FirstOrDefaultAsync(j => j.UserUid == updatedJournalist.Uid)
                         ?? throw new KeyNotFoundException($"Journalist with UID {updatedJournalist.Uid} was not found."
                         );

        journalist.UserU.Password = updatedJournalist.Password;
        journalist.UserU.FirstName = updatedJournalist.FirstName;
        journalist.UserU.LastName = updatedJournalist.LastName;
        journalist.UserU.EmailAddress = updatedJournalist.EmailAddress;

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
        var u = journalist.UserU ?? throw new InvalidOperationException("Expected navigation UserU to be loaded.");

        return new(u.Uid,
            u.Password,
            u.FirstName,
            u.LastName,
            u.EmailAddress,
            journalist.Employer
        );
    }
}
