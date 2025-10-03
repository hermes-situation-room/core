using System;
using System.Collections.Generic;

namespace Hermes.SituationRoom.Data.Entities;

public partial class Activist
{
    public Guid UserUid { get; set; }

    public string Username { get; set; }

    public bool IsFirstNameVisible { get; set; }

    public bool IsLastNameVisible { get; set; }

    public bool IsEmailVisible { get; set; }

    public virtual User UserU { get; set; }
}
