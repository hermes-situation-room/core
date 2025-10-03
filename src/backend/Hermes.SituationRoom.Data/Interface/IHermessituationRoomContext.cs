namespace Hermes.SituationRoom.Data.Interface;

using Hermes.SituationRoom.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;


public interface IHermessituationRoomContext : ICurrentDbContext
{

    DbSet<Activist> Activists { get; set; }

    DbSet<Chat> Chats { get; set; }

    DbSet<Comment> Comments { get; set; }

    DbSet<Journalist> Journalists { get; set; }

    DbSet<Message> Messages { get; set; }

    DbSet<Post> Posts { get; set; }

    DbSet<PrivacyLevelPersonal> PrivacyLevelPersonals { get; set; }

    DbSet<User> Users { get; set; }

    /// <summary>
    /// We use this method and interface to wrap the DbSet.Local property. Local is based on the EF Core internal type
    /// LocalView which is nearly impossible to mock. Henceforth, we shall use this method from now on to allow us to
    /// test the repositories operating on the DbSets in memory.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>A read-only collection of the tracked or local entities respectively of type T of the DbContext.</returns>
    IReadOnlyCollection<T> GetLocalEntity<T>() where T : class;

    Task<int> SaveChangesAsync();

    int SaveChanges();
}