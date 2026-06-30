namespace Hermes.SituationRoom.Data.Repositories;

using Entities;
using Interface;
using Microsoft.EntityFrameworkCore;
using Shared.BusinessObjects;

public sealed class UserTagRepository(IHermessituationRoomContext context) : IUserTagRepository
{
    public async Task<Guid> AddAsync(UserTagBo userTagBo)
    {
        ArgumentNullException.ThrowIfNull(userTagBo);

        var newUserTag = new UserTag
        {
            Uid = userTagBo.Uid == Guid.Empty ? Guid.NewGuid() : userTagBo.Uid,
            OwnerUid = userTagBo.OwnerUid,
            TaggedUserUid = userTagBo.TaggedUserUid,
            TagName = userTagBo.TagName,
            Notes = userTagBo.Notes,
            CreatedAt = userTagBo.CreatedAt == default ? DateTime.UtcNow : userTagBo.CreatedAt
        };

        context.UserTags.Add(newUserTag);
        await context.SaveChangesAsync();

        return newUserTag.Uid;
    }

    public async Task<UserTagBo?> GetByUidAsync(Guid uid)
    {
        var userTag = await context.UserTags
            .AsNoTracking()
            .FirstOrDefaultAsync(ut => ut.Uid == uid);

        return userTag == null ? null : MapToBo(userTag);
    }

    public async Task<IReadOnlyList<UserTagBo>> GetByOwnerAsync(Guid ownerUid)
    {
        return await context.UserTags
            .AsNoTracking()
            .Where(ut => ut.OwnerUid == ownerUid)
            .OrderByDescending(ut => ut.CreatedAt)
            .Select(ut => MapToBo(ut))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<UserTagBo>> GetByOwnerAndTaggedAsync(Guid ownerUid, Guid taggedUserUid)
    {
        return await context.UserTags
            .AsNoTracking()
            .Where(ut => ut.OwnerUid == ownerUid && ut.TaggedUserUid == taggedUserUid)
            .OrderByDescending(ut => ut.CreatedAt)
            .Select(ut => MapToBo(ut))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<UserTagBo>> SearchByTagNameAsync(Guid ownerUid, string tagName)
    {
        return await context.UserTags
            .AsNoTracking()
            .Where(ut => ut.OwnerUid == ownerUid && ut.TagName.Contains(tagName))
            .OrderByDescending(ut => ut.CreatedAt)
            .Select(ut => MapToBo(ut))
            .ToListAsync();
    }

    public async Task UpdateAsync(UserTagBo userTagBo)
    {
        ArgumentNullException.ThrowIfNull(userTagBo);

        var userTag = await context.UserTags
            .FirstOrDefaultAsync(ut => ut.Uid == userTagBo.Uid && ut.OwnerUid == userTagBo.OwnerUid);

        if (userTag != null)
        {
            userTag.TagName = userTagBo.TagName;
            userTag.Notes = userTagBo.Notes;
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(Guid uid, Guid ownerUid)
    {
        var userTag = await context.UserTags
            .FirstOrDefaultAsync(ut => ut.Uid == uid && ut.OwnerUid == ownerUid);

        if (userTag != null)
        {
            context.UserTags.Remove(userTag);
            await context.SaveChangesAsync();
        }
    }

    private static UserTagBo MapToBo(UserTag userTag) => new(
        userTag.Uid,
        userTag.OwnerUid,
        userTag.TaggedUserUid,
        userTag.TagName,
        userTag.Notes,
        userTag.CreatedAt
    );
}
