using System;

namespace Hermes.SituationRoom.Data.Entities;

public partial class TagRequest
{
    public Guid Uid { get; set; }

    public Guid RequestedByUid { get; set; }

    public string Name { get; set; }

    public string Status { get; set; }

    public Guid? ReviewedByUid { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User RequestedByU { get; set; }

    public virtual User ReviewedByU { get; set; }
}
