using System;

namespace Hermes.SituationRoom.Data.Entities;

public partial class UserTag
{
    public Guid Uid { get; set; }

    public Guid OwnerUid { get; set; }

    public Guid TaggedUserUid { get; set; }

    public string TagName { get; set; }

    public string Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User OwnerU { get; set; }

    public virtual User TaggedUserU { get; set; }
}

