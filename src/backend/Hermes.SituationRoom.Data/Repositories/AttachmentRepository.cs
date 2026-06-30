namespace Hermes.SituationRoom.Data.Repositories;

using Entities;
using Interface;
using Microsoft.EntityFrameworkCore;
using Shared.BusinessObjects;

public sealed class AttachmentRepository(IHermessituationRoomContext context) : IAttachmentRepository
{
    public async Task<Guid> AddAsync(AttachmentBo attachmentBo)
    {
        ArgumentNullException.ThrowIfNull(attachmentBo);

        var newAttachment = new Attachment
        {
            Uid = attachmentBo.Uid == Guid.Empty ? Guid.NewGuid() : attachmentBo.Uid,
            OwnerUid = attachmentBo.OwnerUid,
            FileName = attachmentBo.FileName,
            ContentType = attachmentBo.ContentType,
            SizeBytes = attachmentBo.SizeBytes,
            StorageKey = attachmentBo.StorageKey,
            EncryptionIv = attachmentBo.EncryptionIv,
            EncryptionTag = attachmentBo.EncryptionTag,
            WrappedKey = attachmentBo.WrappedKey,
            ContentHash = attachmentBo.ContentHash,
            PostUid = attachmentBo.PostUid,
            MessageUid = attachmentBo.MessageUid,
            CreatedAt = attachmentBo.CreatedAt == default ? DateTime.UtcNow : attachmentBo.CreatedAt
        };

        context.Attachments.Add(newAttachment);
        await context.SaveChangesAsync();

        return newAttachment.Uid;
    }

    public async Task<AttachmentBo?> GetAsync(Guid uid)
    {
        var attachment = await context.Attachments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Uid == uid);

        return attachment == null ? null : MapToBo(attachment);
    }

    public async Task<IReadOnlyList<AttachmentBo>> GetByPostAsync(Guid postUid)
    {
        return await context.Attachments
            .AsNoTracking()
            .Where(a => a.PostUid == postUid)
            .OrderBy(a => a.CreatedAt)
            .Select(a => MapToBo(a))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<AttachmentBo>> GetByMessageAsync(Guid messageUid)
    {
        return await context.Attachments
            .AsNoTracking()
            .Where(a => a.MessageUid == messageUid)
            .OrderBy(a => a.CreatedAt)
            .Select(a => MapToBo(a))
            .ToListAsync();
    }

    public async Task LinkToPostAsync(Guid attachmentUid, Guid ownerUid, Guid postUid)
    {
        var attachment = await context.Attachments
            .FirstOrDefaultAsync(a => a.Uid == attachmentUid && a.OwnerUid == ownerUid);

        if (attachment != null)
        {
            attachment.PostUid = postUid;
            await context.SaveChangesAsync();
        }
    }

    public async Task LinkToMessageAsync(Guid attachmentUid, Guid ownerUid, Guid messageUid)
    {
        var attachment = await context.Attachments
            .FirstOrDefaultAsync(a => a.Uid == attachmentUid && a.OwnerUid == ownerUid);

        if (attachment != null)
        {
            attachment.MessageUid = messageUid;
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(Guid uid)
    {
        var attachment = await context.Attachments
            .FirstOrDefaultAsync(a => a.Uid == uid);

        if (attachment != null)
        {
            context.Attachments.Remove(attachment);
            await context.SaveChangesAsync();
        }
    }

    private static AttachmentBo MapToBo(Attachment attachment) => new(
        attachment.Uid,
        attachment.OwnerUid,
        attachment.FileName,
        attachment.ContentType,
        attachment.SizeBytes,
        attachment.StorageKey,
        attachment.EncryptionIv,
        attachment.EncryptionTag,
        attachment.WrappedKey,
        attachment.ContentHash,
        attachment.PostUid,
        attachment.MessageUid,
        attachment.CreatedAt
    );
}
