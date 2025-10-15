using System;
using System.Collections.Generic;

namespace Hermes.SituationRoom.Data.Entities;

public partial class PrivacyLevelPersonal
{
    public Guid Uid { get; set; }

    public bool IsFirstNameVisible { get; set; }

    public bool IsLastNameVisible { get; set; }

    public bool IsEmailVisible { get; set; }

    public Guid OwnerUid { get; set; }

    public Guid ConsumerUid { get; set; }

    public virtual User ConsumerU { get; set; }

    public virtual User OwnerU { get; set; }
}
