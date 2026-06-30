namespace Hermes.SituationRoom.Domain.Interfaces;

public interface IBreachedPasswordService
{
    Task<bool> IsBreachedAsync(string password);
}
