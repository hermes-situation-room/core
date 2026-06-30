-- ========================
-- Migration V1.18: File / Media Attachments (encrypted at rest)
-- Description: Adds the Attachment table for file/media storage. File bytes live
--              on the local server filesystem encrypted with a per-file DEK
--              (AES-256-GCM); the DEK is wrapped under a server-held KEK and the
--              wrapped DEK, GCM nonce and integrity hash are stored here. The blob
--              store alone is useless without the KEK (E2EE_DESIGN.md §7.1).
-- ========================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Attachment' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Attachment
    (
        UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        OwnerUID UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_Attachment_Owner FOREIGN KEY REFERENCES dbo.[User](UID) ON DELETE CASCADE,
        FileName NVARCHAR(255) NOT NULL,
        ContentType NVARCHAR(150) NOT NULL,
        SizeBytes BIGINT NOT NULL,
        -- Relative path (storage key) of the encrypted blob under the configured storage root.
        StorageKey NVARCHAR(400) NOT NULL,
        -- AES-256-GCM nonce/IV for the file payload (per file).
        EncryptionIv VARBINARY(64) NOT NULL,
        -- AES-256-GCM authentication tag for the file payload.
        EncryptionTag VARBINARY(64) NOT NULL,
        -- Per-file data key (DEK), itself encrypted (wrapped) under the server-held KEK.
        WrappedKey VARBINARY(256) NOT NULL,
        -- SHA-256 of the plaintext for tamper-evidence / integrity verification.
        ContentHash VARBINARY(64) NOT NULL,
        -- Optional links to a post or a message (either, both, or neither).
        PostUID UNIQUEIDENTIFIER NULL
            CONSTRAINT FK_Attachment_Post FOREIGN KEY REFERENCES dbo.Post(UID) ON DELETE NO ACTION,
        MessageUID UNIQUEIDENTIFIER NULL
            CONSTRAINT FK_Attachment_Message FOREIGN KEY REFERENCES dbo.Message(UID) ON DELETE NO ACTION,
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX IX_Attachment_Owner ON dbo.Attachment(OwnerUID);
    CREATE INDEX IX_Attachment_Post ON dbo.Attachment(PostUID);
    CREATE INDEX IX_Attachment_Message ON dbo.Attachment(MessageUID);
    CREATE INDEX IX_Attachment_Created ON dbo.Attachment(CreatedAt DESC);
END
GO
