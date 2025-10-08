#nullable enable
namespace Hermes.SituationRoom.Data.Repositories;

using Entities;
using Interface;
using Microsoft.EntityFrameworkCore;
using Shared.BusinessObjects;

public sealed class PrivacyLevelPersonalRepository(IHermessituationRoomContext context) : IPrivacyLevelPersonalRepository
{
    public async Task<Guid> AddAsync(PrivacyLevelPersonalBo privacyLevelPersonalBo)
    {
        ArgumentNullException.ThrowIfNull(privacyLevelPersonalBo);

        var newPrivacyLevelPersonal = new PrivacyLevelPersonal
        {
            Uid = Guid.NewGuid(),
            IsFirstNameVisible = privacyLevelPersonalBo.IsFirstNameVisible,
            IsLastNameVisible = privacyLevelPersonalBo.IsLastNameVisible,
            IsEmailVisible = privacyLevelPersonalBo.IsEmailVisible,
            OwnerUid = privacyLevelPersonalBo.OwnerUid,
            ConsumerUid = privacyLevelPersonalBo.ConsumerUid
        };

        context.PrivacyLevelPersonals.Add(newPrivacyLevelPersonal);
        await context.SaveChangesAsync();

        return newPrivacyLevelPersonal.Uid;
    }

    public async Task<PrivacyLevelPersonalBo> GetPrivacyLevelPersonalBoAsync(Guid ownerUid, Guid consumerUid)
    {
        if (ownerUid == Guid.Empty || consumerUid == Guid.Empty)
            throw new ArgumentException("GUID must not be empty.");

        var privacyLevelPersonal = await context.PrivacyLevelPersonals
                       .AsNoTracking()
                       .FirstOrDefaultAsync(p => p.OwnerUid == ownerUid && p.ConsumerUid == consumerUid)
                   ?? throw new KeyNotFoundException("PrivacyLevelPersonal was not found.");

        return MapToBo(privacyLevelPersonal);
    }

    public async Task<IReadOnlyList<PrivacyLevelPersonalBo>> GetAllPrivacyLevelPersonalBosAsync()
    {
        var list = await context.PrivacyLevelPersonals
            .AsNoTracking()
            .Select(p => MapToBo(p))
            .ToListAsync();

        return list;
    }

    public async Task<PrivacyLevelPersonalBo> UpdateAsync(PrivacyLevelPersonalBo updatedPrivacyLevelPersonal)
    {
        ArgumentNullException.ThrowIfNull(updatedPrivacyLevelPersonal);
        if (updatedPrivacyLevelPersonal.Uid == Guid.Empty)
            throw new ArgumentException("UID required.", nameof(updatedPrivacyLevelPersonal));

        var privacyLevelPersonal = await context.PrivacyLevelPersonals.FirstOrDefaultAsync(u => u.Uid == updatedPrivacyLevelPersonal.Uid)
                   ?? throw new KeyNotFoundException($"PrivacyLevelPersonal with UID {updatedPrivacyLevelPersonal.Uid} was not found.");

        privacyLevelPersonal.IsFirstNameVisible = updatedPrivacyLevelPersonal.IsFirstNameVisible;
        privacyLevelPersonal.IsLastNameVisible = updatedPrivacyLevelPersonal.IsLastNameVisible;
        privacyLevelPersonal.IsEmailVisible = updatedPrivacyLevelPersonal.IsEmailVisible;

        context.PrivacyLevelPersonals.Update(privacyLevelPersonal);
        await context.SaveChangesAsync();

        return MapToBo(privacyLevelPersonal);
    }

    public Task DeleteAsync(Guid privacyLevelPersonalId)
    {
        if (privacyLevelPersonalId == Guid.Empty)
            throw new ArgumentException("GUID must not be empty.", nameof(privacyLevelPersonalId));

        var privacyLevelPersonal = context.PrivacyLevelPersonals.FirstOrDefault(p => p.Uid == privacyLevelPersonalId);
        if (privacyLevelPersonal is null)
            return Task.CompletedTask;

        context.PrivacyLevelPersonals.Remove(privacyLevelPersonal);
        context.SaveChanges();

        return Task.CompletedTask;
    }

    private static PrivacyLevelPersonalBo MapToBo(PrivacyLevelPersonal privacyLevelPersonal) => new(privacyLevelPersonal.Uid,
        privacyLevelPersonal.IsFirstNameVisible,
        privacyLevelPersonal.IsLastNameVisible,
        privacyLevelPersonal.IsEmailVisible,
        privacyLevelPersonal.OwnerUid,
        privacyLevelPersonal.ConsumerUid
    );
}
