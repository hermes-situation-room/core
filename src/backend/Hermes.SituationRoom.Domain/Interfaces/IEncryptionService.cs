namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.BusinessObjects;

public interface IEncryptionService
{
    (byte[] Hash, byte[] Salt) EncryptPassword(string password);

    bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt);
}
