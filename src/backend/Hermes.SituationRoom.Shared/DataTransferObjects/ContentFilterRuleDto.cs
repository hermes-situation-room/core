namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record ContentFilterRuleDto(
    Guid Uid,
    string Pattern,
    string Action,
    bool Enabled,
    DateTime CreatedAt
);

public record CreateContentFilterRuleDto(
    string Pattern,
    string Action
);

public record UpdateContentFilterRuleDto(
    string Pattern,
    string Action,
    bool Enabled
);
