using System;
using System.Collections.Generic;

namespace Hermes.SituationRoom.Data.Entities;

public partial class UserChatReadStatus
{
    public Guid UserChatReadStatusId { get; set; }

    public Guid UserId { get; set; }

    public Guid ChatId { get; set; }

    public DateTime ReadTime { get; set; }

    public virtual Chat Chat { get; set; }

    public virtual User User { get; set; }
}
