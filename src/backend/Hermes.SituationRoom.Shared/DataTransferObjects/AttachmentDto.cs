namespace Hermes.SituationRoom.Shared.DataTransferObjects;

/// <summary>Public-facing metadata for an attachment. Never exposes key material.</summary>
public record AttachmentDto(
    Guid Uid,
    Guid OwnerUid,
    string FileName,
    string ContentType,
    long SizeBytes,
    Guid? PostUid,
    Guid? MessageUid,
    DateTime CreatedAt
);
