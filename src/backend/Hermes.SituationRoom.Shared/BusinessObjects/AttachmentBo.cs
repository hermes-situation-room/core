namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record AttachmentBo(
    Guid Uid,
    Guid OwnerUid,
    string FileName,
    string ContentType,
    long SizeBytes,
    string StorageKey,
    byte[] EncryptionIv,
    byte[] EncryptionTag,
    byte[] WrappedKey,
    byte[] ContentHash,
    Guid? PostUid,
    Guid? MessageUid,
    DateTime CreatedAt
);
