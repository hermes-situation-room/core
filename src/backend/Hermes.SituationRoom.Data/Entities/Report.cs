using System;

namespace Hermes.SituationRoom.Data.Entities;

public partial class Report
{
    public Guid Uid { get; set; }

    public Guid ReporterUid { get; set; }

    public string TargetType { get; set; }

    public Guid TargetUid { get; set; }

    public string Reason { get; set; }

    public string Status { get; set; }

    public DateTime Timestamp { get; set; }

    public Guid? ReviewedByUid { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string AdminNotes { get; set; }

    public virtual User ReporterU { get; set; }

    public virtual User ReviewedByU { get; set; }
}

