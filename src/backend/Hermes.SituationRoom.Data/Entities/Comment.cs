using System;
using System.Collections.Generic;

namespace Hermes.SituationRoom.Data.Entities;

public partial class Comment
{
    public Guid Uid { get; set; }

    public DateTime Timestamp { get; set; }

    public string Content { get; set; }

    public Guid CreatorUid { get; set; }

    public Guid PostUid { get; set; }

    public virtual User CreatorU { get; set; }

    public virtual Post PostU { get; set; }
}
