namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record ReportDto(
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

public record CreateReportDto(
    Guid ReporterUid,
    string TargetType,
    Guid TargetUid,
    string Reason
);

public record ReviewReportDto(
    Guid ReviewedByUid,
    string Status,
    string AdminNotes
);

