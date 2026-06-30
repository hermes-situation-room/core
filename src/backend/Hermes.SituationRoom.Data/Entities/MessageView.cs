using System;

namespace Hermes.SituationRoom.Data.Entities;

public partial class MessageView
{
    public Guid Uid { get; set; }

    public Guid MessageUid { get; set; }

    public Guid UserUid { get; set; }

    public DateTime ViewedAt { get; set; }

    public virtual Message MessageU { get; set; }

    public virtual User UserU { get; set; }
}
