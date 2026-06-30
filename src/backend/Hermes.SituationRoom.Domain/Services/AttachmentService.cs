namespace Hermes.SituationRoom.Domain.Services;

using AutoMapper;
using Data.Interface;
using Interfaces;
using Microsoft.Extensions.Options;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Shared.Exceptions;

public sealed class AttachmentService(
    IAttachmentRepository attachmentRepository,
    IFileStorageService fileStorageService,
    IMessageRepository messageRepository,
    IChatService chatService,
    IPostService postService,
    IOptions<FileStorageOptions> options,
    IMapper mapper) : IAttachmentService
{
    // Allowlist of accepted content types: images, video, pdf, common docs.
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/heic", "image/svg+xml",
        "video/mp4", "video/webm", "video/quicktime", "video/x-matroska",
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "text/plain", "text/csv"
    };

    private readonly long maxFileSizeBytes = options.Value.MaxFileSizeBytes;

    public async Task<AttachmentDto> UploadAsync(
        Guid ownerUid,
        string fileName,
        string contentType,
        byte[] bytes,
        Guid? postUid,
        Guid? messageUid,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        if (bytes.LongLength == 0)
        {
            throw new ValidationBusinessException(nameof(bytes), "Uploaded file is empty.");
        }

        if (bytes.LongLength > maxFileSizeBytes)
        {
            throw new ValidationBusinessException(
                nameof(bytes),
                $"File exceeds the maximum allowed size of {maxFileSizeBytes} bytes.");
        }

        if (string.IsNullOrWhiteSpace(contentType) || !AllowedContentTypes.Contains(contentType))
        {
            throw new ValidationBusinessException(nameof(contentType), $"Content type '{contentType}' is not allowed.");
        }

        var storedFile = await fileStorageService.WriteAsync(bytes, cancellationToken);

        var attachmentBo = new AttachmentBo(
            Guid.NewGuid(),
            ownerUid,
            SanitizeFileName(fileName),
            contentType,
            storedFile.SizeBytes,
            storedFile.StorageKey,
            storedFile.EncryptionIv,
            storedFile.EncryptionTag,
            storedFile.WrappedKey,
            storedFile.ContentHash,
            postUid,
            messageUid,
            DateTime.UtcNow);

        var uid = await attachmentRepository.AddAsync(attachmentBo);

        return mapper.Map<AttachmentDto>(attachmentBo with { Uid = uid });
    }

    public async Task<AttachmentContent?> DownloadAsync(
        Guid attachmentUid,
        Guid requesterUid,
        string? requesterRole,
        CancellationToken cancellationToken = default)
    {
        var attachment = await attachmentRepository.GetAsync(attachmentUid);
        if (attachment == null)
        {
            return null;
        }

        if (!await IsAuthorizedToReadAsync(attachment, requesterUid, requesterRole))
        {
            throw new UnauthorizedAccessException("You are not allowed to access this attachment.");
        }

        var storedFile = new StoredFile(
            attachment.StorageKey,
            attachment.EncryptionIv,
            attachment.EncryptionTag,
            attachment.WrappedKey,
            attachment.ContentHash,
            attachment.SizeBytes);

        var bytes = await fileStorageService.ReadAsync(storedFile, cancellationToken);
        return new AttachmentContent(attachment.FileName, attachment.ContentType, bytes);
    }

    public async Task DeleteAsync(Guid attachmentUid, Guid requesterUid)
    {
        var attachment = await attachmentRepository.GetAsync(attachmentUid);
        if (attachment == null)
        {
            return;
        }

        if (attachment.OwnerUid != requesterUid)
        {
            throw new UnauthorizedAccessException("Only the owner can delete this attachment.");
        }

        fileStorageService.Delete(attachment.StorageKey);
        await attachmentRepository.DeleteAsync(attachmentUid);
    }

    public async Task<IReadOnlyList<AttachmentDto>> GetByPostAsync(Guid postUid, Guid requesterUid, string? requesterRole)
    {
        var post = await postService.GetPostAsync(postUid);
        if (post == null || (post.CreatorUid != requesterUid && post.PrivacyLevel > ResolvePrivacyLevel(requesterRole)))
        {
            return [];
        }

        var attachments = await attachmentRepository.GetByPostAsync(postUid);
        return attachments.Select(mapper.Map<AttachmentDto>).ToList();
    }

    public async Task<IReadOnlyList<AttachmentDto>> GetByMessageAsync(Guid messageUid, Guid requesterUid)
    {
        var message = await messageRepository.GetMessageAsync(messageUid);
        if (message == null)
        {
            return [];
        }

        var chat = await chatService.GetChatAsync(message.ChatUid);
        if (chat == null || !chat.MemberUids.Contains(requesterUid))
        {
            return [];
        }

        var attachments = await attachmentRepository.GetByMessageAsync(messageUid);
        return attachments.Select(mapper.Map<AttachmentDto>).ToList();
    }

    public async Task LinkToPostAsync(IReadOnlyList<Guid> attachmentUids, Guid ownerUid, Guid postUid)
    {
        if (attachmentUids == null)
        {
            return;
        }

        foreach (var attachmentUid in attachmentUids)
        {
            await attachmentRepository.LinkToPostAsync(attachmentUid, ownerUid, postUid);
        }
    }

    public async Task LinkToMessageAsync(IReadOnlyList<Guid> attachmentUids, Guid ownerUid, Guid messageUid)
    {
        if (attachmentUids == null)
        {
            return;
        }

        foreach (var attachmentUid in attachmentUids)
        {
            await attachmentRepository.LinkToMessageAsync(attachmentUid, ownerUid, messageUid);
        }
    }

    private async Task<bool> IsAuthorizedToReadAsync(AttachmentBo attachment, Guid requesterUid, string? requesterRole)
    {
        // Owner can always read their own attachment.
        if (attachment.OwnerUid == requesterUid)
        {
            return true;
        }

        // Linked to a message: requester must be a member of the message's chat.
        if (attachment.MessageUid.HasValue)
        {
            var message = await messageRepository.GetMessageAsync(attachment.MessageUid.Value);
            if (message != null)
            {
                var chat = await chatService.GetChatAsync(message.ChatUid);
                if (chat != null && chat.MemberUids.Contains(requesterUid))
                {
                    return true;
                }
            }
        }

        // Linked to a post: requester must be allowed to see the post per its privacy.
        if (attachment.PostUid.HasValue)
        {
            var post = await postService.GetPostAsync(attachment.PostUid.Value);
            if (post != null && (post.CreatorUid == requesterUid || post.PrivacyLevel <= ResolvePrivacyLevel(requesterRole)))
            {
                return true;
            }
        }

        return false;
    }

    // Mirrors PostService: anon=0, Activist=1, Journalist=2. A requester may see a
    // post whose PrivacyLevel is at or below their clearance.
    private static int ResolvePrivacyLevel(string? requesterRole) => requesterRole switch
    {
        "Journalist" => 2,
        "Activist" => 1,
        _ => 0
    };

    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "file";
        }

        // Keep just the leaf name; strip any path components.
        var leaf = Path.GetFileName(fileName);
        return string.IsNullOrWhiteSpace(leaf) ? "file" : leaf;
    }
}
