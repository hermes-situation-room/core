using System;
using System.Collections.Generic;

namespace Hermes.SituationRoom.Data.Entities;

public partial class Message
{
    public Guid Uid { get; set; }

    public DateTime Timestamp { get; set; }

    public string Content { get; set; }

    public Guid SenderUid { get; set; }

    public Guid ChatUid { get; set; }

    public virtual Chat ChatU { get; set; }

    public virtual User SenderU { get; set; }
}
