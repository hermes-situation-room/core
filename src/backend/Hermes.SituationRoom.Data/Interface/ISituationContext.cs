namespace Hermes.SituationRoom.Data.Interface;

public interface ISituationContext
{
    int SaveChanges();
    Task<int> SaveChangesAsync();
}