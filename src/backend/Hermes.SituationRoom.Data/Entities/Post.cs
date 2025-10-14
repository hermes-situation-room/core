using System;
using System.Collections.Generic;

namespace Hermes.SituationRoom.Data.Entities;

public partial class Post
{
    public Guid Uid { get; set; }

    public DateTime Timestamp { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public string Content { get; set; }

    public Guid CreatorUid { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual User CreatorU { get; set; }
}
