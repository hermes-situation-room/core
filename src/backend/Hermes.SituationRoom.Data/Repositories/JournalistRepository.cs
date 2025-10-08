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

        return MapToBo(await context.Journalists
                           .AsNoTracking()
                           .Include(j => j.UserU)
                           .FirstOrDefaultAsync(j => j.UserUid == journalistUid)
                       ?? throw new KeyNotFoundException($"Journalist with UID {journalistUid} was not found.")
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
                             .Include(j => j.UserU)
                             .FirstOrDefaultAsync(j => j.UserUid == updatedJournalist.Uid)
                         ?? throw new KeyNotFoundException($"Journalist with UID {updatedJournalist.Uid} was not found."
                         );

        journalist.UserU.Password = updatedJournalist.Password;
        journalist.UserU.FirstName = updatedJournalist.FirstName;
        journalist.UserU.LastName = updatedJournalist.LastName;
        journalist.UserU.EmailAddress = updatedJournalist.EmailAddress;

        journalist.Employer = updatedJournalist.Employer;

        context.Journalists.Update(journalist);
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
            user.Password,
            user.FirstName,
            user.LastName,
            user.EmailAddress,
            journalist.Employer
        );
    }
}
