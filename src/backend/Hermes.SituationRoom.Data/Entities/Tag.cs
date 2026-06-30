using System;

namespace Hermes.SituationRoom.Data.Entities;

public partial class Tag
{
    public Guid Uid { get; set; }

    public string Name { get; set; }

    public DateTime CreatedAt { get; set; }
}
