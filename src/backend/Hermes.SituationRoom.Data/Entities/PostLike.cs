using System;

namespace Hermes.SituationRoom.Data.Entities;

public partial class PostLike
{
    public Guid Uid { get; set; }

    public Guid PostUid { get; set; }

    public Guid UserUid { get; set; }

    public DateTime Timestamp { get; set; }

    public virtual Post PostU { get; set; }

    public virtual User UserU { get; set; }
}

