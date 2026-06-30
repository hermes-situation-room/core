namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record ContentFilterRuleBo(
    Guid Uid,
    string Pattern,
    string Action,
    bool Enabled,
    DateTime CreatedAt
);
