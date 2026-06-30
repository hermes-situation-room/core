namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record ReportBo(
    Guid Uid,
    Guid ReporterUid,
    string TargetType,
    Guid TargetUid,
    string Reason,
    string Status,
    DateTime Timestamp,
    Guid? ReviewedByUid,
    DateTime? ReviewedAt,
    string AdminNotes
);

