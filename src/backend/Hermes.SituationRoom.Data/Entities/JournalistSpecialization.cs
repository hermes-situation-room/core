using System;

namespace Hermes.SituationRoom.Data.Entities;

public partial class JournalistSpecialization
{
    public Guid JournalistUid { get; set; }

    public Guid SpecializationTagUid { get; set; }

    public virtual Journalist JournalistU { get; set; }

    public virtual SpecializationTag SpecializationTagU { get; set; }
}

