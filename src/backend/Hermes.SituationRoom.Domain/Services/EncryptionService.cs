namespace Hermes.SituationRoom.Domain.Services;

using System.Security.Cryptography;
using Hermes.SituationRoom.Data.Entities;
using Hermes.SituationRoom.Data.Interface;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Interfaces;

public class EncryptionService() : IEncryptionService
{
    private const int SaltLength = 16;
    private const int HashLength = 32;
    private const int Iterations = 100_000;

    public (byte[] Hash, byte[] Salt) EncryptPassword(string password)
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] salt = new byte[SaltLength];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256);

            byte[] hash = pbkdf2.GetBytes(HashLength);

            return (hash, salt);
        }
    }

    public bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            storedSalt,
            Iterations,
            HashAlgorithmName.SHA256);

        byte[] computedHash = pbkdf2.GetBytes(HashLength);

        return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
    }
}
