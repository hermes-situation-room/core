namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record CreatePostRequestDto(string Title,
    string Description,
    string Content,
    Guid CreatorUid,
    int PrivacyLevel,
    IReadOnlyList<string> Tags,
    // Optional ids of attachments (already uploaded by this user) to link to the post.
    IReadOnlyList<Guid>? AttachmentIds = null
);
