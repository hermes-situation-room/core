#nullable enable
namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record ChatBo(Guid Uid,
    Guid User1Uid,
    Guid User2Uid,
    IReadOnlyList<Guid> MemberUids,
    string? Name = null,
    bool IsGroup = false)
{
    // Backward-compatible 1:1 constructor: derives MemberUids from the two participants.
    public ChatBo(Guid uid, Guid user1Uid, Guid user2Uid)
        : this(uid, user1Uid, user2Uid, new[] { user1Uid, user2Uid }, null, false)
    {
    }
}
