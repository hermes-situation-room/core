namespace Hermes.SituationRoom.Data.Context;

using Interface;
using Microsoft.EntityFrameworkCore;

public partial class SituationContext : ISituationContext
{
    public SituationContext(DbContextOptions<SituationContext> options) : base(options)
    {
        this.Database.EnsureCreated();
    }

    public Task<int> SaveChangesAsync() => base.SaveChangesAsync();
}