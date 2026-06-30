using System;
using System.Collections.Generic;

namespace Hermes.SituationRoom.Data.Entities;

public partial class Chat
{
    public Guid Uid { get; set; }

    public Guid User1Uid { get; set; }

    public Guid User2Uid { get; set; }

    public string Name { get; set; }

    public bool IsGroup { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<ChatMember> Members { get; set; } = new List<ChatMember>();

    public virtual User User1U { get; set; }

    public virtual User User2U { get; set; }

    public virtual ICollection<UserChatReadStatus> UserChatReadStatuses { get; set; } = new List<UserChatReadStatus>();
}
