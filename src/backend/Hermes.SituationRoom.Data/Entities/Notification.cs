using System;

namespace Hermes.SituationRoom.Data.Entities;

public partial class Notification
{
    public Guid Uid { get; set; }

    public Guid UserUid { get; set; }

    public string Title { get; set; }

    public string Message { get; set; }

    public string Type { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? RelatedEntityUid { get; set; }

    public string RelatedEntityType { get; set; }

    public virtual User UserU { get; set; }
}

