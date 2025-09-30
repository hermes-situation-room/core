namespace Hermes.SituationRoom.Data.Context;

using Microsoft.EntityFrameworkCore;

public partial class SituationContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}