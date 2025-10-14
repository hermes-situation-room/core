using System;
using System.Collections.Generic;

namespace Hermes.SituationRoom.Data.Entities;

public partial class Chat
{
    public Guid Uid { get; set; }

    public Guid User1Uid { get; set; }

    public Guid User2Uid { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual User User1U { get; set; }

    public virtual User User2U { get; set; }
}
