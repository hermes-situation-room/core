using System;

namespace Hermes.SituationRoom.Data.Entities;

public partial class ContentFilterRule
{
    public Guid Uid { get; set; }

    public string Pattern { get; set; }

    public string Action { get; set; }

    public bool Enabled { get; set; }

    public DateTime CreatedAt { get; set; }
}
