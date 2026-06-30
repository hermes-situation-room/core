#nullable enable
namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record CreateGroupChatRequestDto(IReadOnlyList<Guid> MemberUids,
    string? Name = null
);
