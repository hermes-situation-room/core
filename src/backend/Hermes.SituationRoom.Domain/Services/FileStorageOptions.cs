namespace Hermes.SituationRoom.Domain.Services;

/// <summary>Bound from the "FileStorage" configuration section (appsettings).</summary>
public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>Local directory where encrypted blobs are stored. Created if missing.</summary>
    public string StorageDirectory { get; set; } = "App_Data/attachments";

    /// <summary>
    /// Base64-encoded 32-byte (256-bit) server-held key-encryption key (KEK).
    /// Wraps each per-file DEK. MUST be replaced with a real secret in production
    /// (configuration / secret store), never committed.
    /// </summary>
    public string ServerKeyBase64 { get; set; } = string.Empty;

    /// <summary>Maximum accepted upload size in bytes. Default 25 MB.</summary>
    public long MaxFileSizeBytes { get; set; } = 25 * 1024 * 1024;
}
