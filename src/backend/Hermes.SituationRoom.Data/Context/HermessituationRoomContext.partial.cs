namespace Hermes.SituationRoom.Data.Context;

using System.Collections.Immutable;
using Hermes.SituationRoom.Data.Interface;
using Microsoft.EntityFrameworkCore;

public partial class HermessituationRoomContext : IHermessituationRoomContext
{
    /// <inheritdoc />
    public IReadOnlyCollection<T> GetLocalEntity<T>() where T : class => Set<T>().Local.ToImmutableArray();

    public DbContext Context => this;
    
    public Task<int> SaveChangesAsync() => base.SaveChangesAsync();
}
