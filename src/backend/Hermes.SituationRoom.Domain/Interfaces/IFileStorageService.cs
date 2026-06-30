namespace Hermes.SituationRoom.Domain.Interfaces;

/// <summary>
/// The encrypted blob written to / read from local disk together with the
/// metadata needed to decrypt it (E2EE_DESIGN.md §7.1: per-file DEK, AES-256-GCM,
/// DEK wrapped under a server-held KEK, stored apart from the blob).
/// </summary>
public sealed record StoredFile(
    string StorageKey,
    byte[] EncryptionIv,
    byte[] EncryptionTag,
    byte[] WrappedKey,
    byte[] ContentHash,
    long SizeBytes);

public interface IFileStorageService
{
    /// <summary>
    /// Encrypts <paramref name="plaintext"/> with a fresh per-file DEK (AES-256-GCM),
    /// wraps the DEK under the server KEK, writes only the ciphertext to local disk,
    /// and returns the metadata to persist alongside it.
    /// </summary>
    Task<StoredFile> WriteAsync(byte[] plaintext, CancellationToken cancellationToken = default);

    /// <summary>Reads the encrypted blob, unwraps the DEK, and returns decrypted bytes.</summary>
    Task<byte[]> ReadAsync(StoredFile storedFile, CancellationToken cancellationToken = default);

    /// <summary>Deletes the encrypted blob from local disk. Safe if already gone.</summary>
    void Delete(string storageKey);
}
