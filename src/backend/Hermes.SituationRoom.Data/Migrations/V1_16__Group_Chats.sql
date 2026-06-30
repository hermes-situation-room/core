-- ========================
-- Migration V1.16: Group Chats
-- Date: June 30, 2026
-- Description: Adds n:m chat membership (ChatMember) seeded from the existing
--              1:1 Chat.User1UID/User2UID columns, plus optional Chat.Name and
--              Chat.IsGroup. The legacy User1UID/User2UID columns are intentionally
--              KEPT for safe rollback and as a fallback for the 1:1 display path.
-- ========================

-- ========================
-- 1. Chat group metadata (Name + IsGroup)
-- ========================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Chat') AND name = 'Name')
BEGIN
    ALTER TABLE dbo.Chat ADD Name NVARCHAR(200) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Chat') AND name = 'IsGroup')
BEGIN
    ALTER TABLE dbo.Chat ADD IsGroup BIT NOT NULL DEFAULT 0;
END
GO

-- ========================
-- 2. ChatMember (n:m chat participants)
-- ========================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatMember' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.ChatMember
    (
        UID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ChatUID UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_ChatMember_Chat FOREIGN KEY REFERENCES dbo.Chat(UID) ON DELETE CASCADE,
        UserUID UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_ChatMember_User FOREIGN KEY REFERENCES dbo.[User](UID) ON DELETE NO ACTION,
        JoinedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT UQ_ChatMember_Chat_User UNIQUE(ChatUID, UserUID)
    );

    -- Indexes for membership lookups
    CREATE INDEX IX_ChatMember_Chat ON dbo.ChatMember(ChatUID);
    CREATE INDEX IX_ChatMember_User ON dbo.ChatMember(UserUID);
END
GO

-- ========================
-- 3. Seed ChatMember from existing 1:1 chats (only if empty)
--    Every existing chat becomes a 2-member, IsGroup = 0 chat.
-- ========================
IF NOT EXISTS (SELECT 1 FROM dbo.ChatMember)
BEGIN
    INSERT INTO dbo.ChatMember (UID, ChatUID, UserUID)
    SELECT NEWID(), UID, User1UID FROM dbo.Chat
    UNION ALL
    SELECT NEWID(), UID, User2UID FROM dbo.Chat;
END
GO
