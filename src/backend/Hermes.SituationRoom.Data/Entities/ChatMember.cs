using System;

namespace Hermes.SituationRoom.Data.Entities;

public partial class ChatMember
{
    public Guid Uid { get; set; }

    public Guid ChatUid { get; set; }

    public Guid UserUid { get; set; }

    public DateTime JoinedAt { get; set; }

    public virtual Chat ChatU { get; set; }

    public virtual User UserU { get; set; }
}
