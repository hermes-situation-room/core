namespace Hermes.SituationRoom.Domain.Services;

using System.Security.Cryptography;
using Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Local-filesystem file storage encrypted at rest per E2EE_DESIGN.md §7.1:
/// each file gets a random 256-bit data key (DEK); the file is encrypted with
/// AES-256-GCM under the DEK; the DEK is wrapped (also AES-256-GCM) under the
/// server-held KEK. Only the ciphertext blob lands on disk; the wrapped DEK,
/// nonce, tag and integrity hash are returned for storage in the relational DB,
/// apart from the blob. The blob store alone is useless without the KEK.
/// </summary>
public sealed class FileStorageService : IFileStorageService
{
    private const int KeySizeBytes = 32;       // AES-256
    private const int NonceSizeBytes = 12;      // AES-GCM standard nonce
    private const int TagSizeBytes = 16;        // AES-GCM tag

    private readonly string storageDirectory;
    private readonly byte[] serverKey;

    public FileStorageService(IOptions<FileStorageOptions> options, ILogger<FileStorageService> logger)
    {
        var resolvedOptions = options.Value;

        storageDirectory = Path.IsPathRooted(resolvedOptions.StorageDirectory)
            ? resolvedOptions.StorageDirectory
            : Path.Combine(AppContext.BaseDirectory, resolvedOptions.StorageDirectory);

        Directory.CreateDirectory(storageDirectory);

        if (string.IsNullOrWhiteSpace(resolvedOptions.ServerKeyBase64))
        {
            logger.LogWarning(
                "FileStorage:ServerKeyBase64 is not configured — falling back to an INSECURE built-in development key " +
                "for at-rest file encryption. Set a real 32-byte base64 key before any non-local deployment.");
        }

        serverKey = ResolveServerKey(resolvedOptions.ServerKeyBase64);
    }

    public async Task<StoredFile> WriteAsync(byte[] plaintext, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plaintext);

        var dataKey = RandomNumberGenerator.GetBytes(KeySizeBytes);
        var contentHash = SHA256.HashData(plaintext);

        // Encrypt the file payload under the per-file DEK.
        var fileNonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
        var ciphertext = new byte[plaintext.Length];
        var fileTag = new byte[TagSizeBytes];
        using (var aesGcm = new AesGcm(dataKey, TagSizeBytes))
        {
            aesGcm.Encrypt(fileNonce, plaintext, ciphertext, fileTag);
        }

        // Wrap the DEK under the server-held KEK (nonce prepended to wrapped blob).
        var wrappedKey = WrapKey(dataKey);
        CryptographicOperations.ZeroMemory(dataKey);

        var storageKey = BuildStorageKey();
        var absolutePath = ResolveAbsolutePath(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        await File.WriteAllBytesAsync(absolutePath, ciphertext, cancellationToken);

        return new StoredFile(
            storageKey,
            fileNonce,
            fileTag,
            wrappedKey,
            contentHash,
            plaintext.LongLength);
    }

    public async Task<byte[]> ReadAsync(StoredFile storedFile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(storedFile);

        var absolutePath = ResolveAbsolutePath(storedFile.StorageKey);
        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException("Encrypted attachment blob is missing.", storedFile.StorageKey);
        }

        var ciphertext = await File.ReadAllBytesAsync(absolutePath, cancellationToken);

        var dataKey = UnwrapKey(storedFile.WrappedKey);
        try
        {
            var plaintext = new byte[ciphertext.Length];
            using var aesGcm = new AesGcm(dataKey, TagSizeBytes);
            aesGcm.Decrypt(storedFile.EncryptionIv, ciphertext, storedFile.EncryptionTag, plaintext);

            // Integrity / tamper-evidence check against the stored plaintext hash.
            var actualHash = SHA256.HashData(plaintext);
            if (!CryptographicOperations.FixedTimeEquals(actualHash, storedFile.ContentHash))
            {
                throw new CryptographicException("Attachment integrity check failed.");
            }

            return plaintext;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(dataKey);
        }
    }

    public void Delete(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            return;
        }

        var absolutePath = ResolveAbsolutePath(storageKey);
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }
    }

    private byte[] WrapKey(byte[] dataKey)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
        var wrapped = new byte[dataKey.Length];
        var tag = new byte[TagSizeBytes];
        using (var aesGcm = new AesGcm(serverKey, TagSizeBytes))
        {
            aesGcm.Encrypt(nonce, dataKey, wrapped, tag);
        }

        // Layout: nonce || tag || wrapped-key.
        var result = new byte[NonceSizeBytes + TagSizeBytes + wrapped.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSizeBytes);
        Buffer.BlockCopy(tag, 0, result, NonceSizeBytes, TagSizeBytes);
        Buffer.BlockCopy(wrapped, 0, result, NonceSizeBytes + TagSizeBytes, wrapped.Length);
        return result;
    }

    private byte[] UnwrapKey(byte[] wrappedKeyBlob)
    {
        var nonce = new byte[NonceSizeBytes];
        var tag = new byte[TagSizeBytes];
        var wrapped = new byte[wrappedKeyBlob.Length - NonceSizeBytes - TagSizeBytes];
        Buffer.BlockCopy(wrappedKeyBlob, 0, nonce, 0, NonceSizeBytes);
        Buffer.BlockCopy(wrappedKeyBlob, NonceSizeBytes, tag, 0, TagSizeBytes);
        Buffer.BlockCopy(wrappedKeyBlob, NonceSizeBytes + TagSizeBytes, wrapped, 0, wrapped.Length);

        var dataKey = new byte[wrapped.Length];
        using var aesGcm = new AesGcm(serverKey, TagSizeBytes);
        aesGcm.Decrypt(nonce, wrapped, tag, dataKey);
        return dataKey;
    }

    private static byte[] ResolveServerKey(string serverKeyBase64)
    {
        if (!string.IsNullOrWhiteSpace(serverKeyBase64))
        {
            var key = Convert.FromBase64String(serverKeyBase64);
            if (key.Length != KeySizeBytes)
            {
                throw new InvalidOperationException(
                    $"{FileStorageOptions.SectionName}:ServerKeyBase64 must decode to {KeySizeBytes} bytes (got {key.Length}).");
            }

            return key;
        }

        // Safe default for local/dev: a deterministic-but-non-empty key so the app
        // runs out of the box. Production MUST supply a real ServerKeyBase64.
        using var derive = new Rfc2898DeriveBytes(
            "situation-room-dev-file-kek",
            "situation-room-file-storage-salt"u8.ToArray(),
            100_000,
            HashAlgorithmName.SHA256);
        return derive.GetBytes(KeySizeBytes);
    }

    private string BuildStorageKey()
    {
        var now = DateTime.UtcNow;
        // Shard by date to avoid huge flat directories.
        return Path.Combine(
            now.ToString("yyyy"),
            now.ToString("MM"),
            now.ToString("dd"),
            $"{Guid.NewGuid():N}.bin");
    }

    private string ResolveAbsolutePath(string storageKey)
    {
        // Normalize separators and guard against traversal outside the storage root.
        var normalized = storageKey.Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(storageDirectory, normalized));
        var rootPath = Path.GetFullPath(storageDirectory);
        if (!fullPath.StartsWith(rootPath, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Invalid attachment storage key.");
        }

        return fullPath;
    }
}
