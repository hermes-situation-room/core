using System;
using System.Collections.Generic;

namespace Hermes.SituationRoom.Data.Entities;

public partial class PostTag
{
    public Guid PostUid { get; set; }

    public string Tag { get; set; }

    public virtual Post PostU { get; set; }
}
