-- ========================
-- Migration V1.17: Disappearing & View-Once Messages (§2.2.3.4)
-- Date: June 30, 2026
-- Description: Adds disappearing-message support (Message.ExpiresAt) and
--              view-once support (Message.IsViewOnce) plus a per-recipient
--              MessageView table so a view-once message can be marked seen
--              independently for each chat member in n:m group chats.
-- ========================

-- ========================
-- 1. Message.ExpiresAt (nullable) for disappearing messages
-- ========================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Message') AND name = 'ExpiresAt')
BEGIN
    ALTER TABLE dbo.Message ADD ExpiresAt DATETIME NULL;
END
GO

-- ========================
-- 2. Message.IsViewOnce (bit, default 0)
-- ========================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Message') AND name = 'IsViewOnce')
BEGIN
    ALTER TABLE dbo.Message ADD IsViewOnce BIT NOT NULL DEFAULT 0;
END
GO

-- Index to make the cleanup-on-read expiry filter cheap.
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Message_ExpiresAt' AND object_id = OBJECT_ID('dbo.Message'))
BEGIN
    CREATE INDEX IX_Message_ExpiresAt ON dbo.Message(ExpiresAt) WHERE ExpiresAt IS NOT NULL;
END
GO

-- ========================
-- 3. MessageView (per-recipient view-once tracking)
-- ========================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MessageView' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.MessageView
    (
        UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        MessageUID UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_MessageView_Message FOREIGN KEY REFERENCES dbo.Message(UID) ON DELETE CASCADE,
        UserUID UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_MessageView_User FOREIGN KEY REFERENCES dbo.[User](UID) ON DELETE NO ACTION,
        ViewedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT UQ_MessageView_Message_User UNIQUE(MessageUID, UserUID)
    );

    CREATE INDEX IX_MessageView_Message ON dbo.MessageView(MessageUID);
    CREATE INDEX IX_MessageView_User ON dbo.MessageView(UserUID);
END
GO
