namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record PostLikeDto(
    Guid Uid,
    Guid PostUid,
    Guid UserUid,
    DateTime Timestamp
);

public record CreatePostLikeDto(
    Guid PostUid,
    Guid UserUid
);

public record PostLikeStatsDto(
    Guid PostUid,
    int LikeCount,
    bool UserHasLiked
);

