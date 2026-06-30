namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.DataTransferObjects;

/// <summary>Decrypted attachment payload streamed back to an authorized requester.</summary>
public sealed record AttachmentContent(
    string FileName,
    string ContentType,
    byte[] Bytes);

public interface IAttachmentService
{
    /// <summary>
    /// Validates size + content-type allowlist, encrypts at rest, persists metadata,
    /// and returns the public attachment metadata. Throws ValidationBusinessException
    /// for oversized / disallowed uploads.
    /// </summary>
    Task<AttachmentDto> UploadAsync(
        Guid ownerUid,
        string fileName,
        string contentType,
        byte[] bytes,
        Guid? postUid,
        Guid? messageUid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Authorizes the requester (owner, or visibility of the linked post per its
    /// privacy, or membership of the linked message's chat), then returns decrypted
    /// bytes. Returns null when not found; throws UnauthorizedAccessException when denied.
    /// </summary>
    Task<AttachmentContent?> DownloadAsync(
        Guid attachmentUid,
        Guid requesterUid,
        string? requesterRole,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes the attachment (blob + metadata). Owner only.</summary>
    Task DeleteAsync(Guid attachmentUid, Guid requesterUid);

    /// <summary>
    /// Returns the metadata (no key material) of the attachments linked to a post, but only
    /// when the requester is allowed to see that post per its privacy. Empty when not allowed
    /// or the post has none.
    /// </summary>
    Task<IReadOnlyList<AttachmentDto>> GetByPostAsync(Guid postUid, Guid requesterUid, string? requesterRole);

    /// <summary>
    /// Returns the metadata (no key material) of the attachments linked to a message, but only
    /// when the requester is a member of the message's chat. Empty otherwise.
    /// </summary>
    Task<IReadOnlyList<AttachmentDto>> GetByMessageAsync(Guid messageUid, Guid requesterUid);

    /// <summary>Links the given owned attachments to a post (used on post creation).</summary>
    Task LinkToPostAsync(IReadOnlyList<Guid> attachmentUids, Guid ownerUid, Guid postUid);

    /// <summary>Links the given owned attachments to a message (used on message send).</summary>
    Task LinkToMessageAsync(IReadOnlyList<Guid> attachmentUids, Guid ownerUid, Guid messageUid);
}
