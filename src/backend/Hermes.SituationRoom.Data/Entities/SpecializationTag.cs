using System;
using System.Collections.Generic;

namespace Hermes.SituationRoom.Data.Entities;

public partial class SpecializationTag
{
    public Guid Uid { get; set; }

    public string Name { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<JournalistSpecialization> JournalistSpecializations { get; set; } = new List<JournalistSpecialization>();
}

