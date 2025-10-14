using System;
using System.Collections.Generic;

namespace Hermes.SituationRoom.Data.Entities;

public partial class Journalist
{
    public Guid UserUid { get; set; }

    public string Employer { get; set; }

    public virtual User UserU { get; set; }
}
