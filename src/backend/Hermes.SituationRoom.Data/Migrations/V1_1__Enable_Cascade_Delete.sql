SET XACT_ABORT ON;
BEGIN TRAN;

------------------------------------------------------------
-- Drop existing FK constraints that should cascade
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Journalist_User')
ALTER TABLE dbo.Journalist DROP CONSTRAINT FK_Journalist_User;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Activist_User')
ALTER TABLE dbo.Activist DROP CONSTRAINT FK_Activist_User;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Chat_User1')
ALTER TABLE dbo.Chat DROP CONSTRAINT FK_Chat_User1;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Chat_User2')
ALTER TABLE dbo.Chat DROP CONSTRAINT FK_Chat_User2;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Message_Chat')
ALTER TABLE dbo.[Message] DROP CONSTRAINT FK_Message_Chat;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Post_User')
ALTER TABLE dbo.Post DROP CONSTRAINT FK_Post_User;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Comment_Post')
ALTER TABLE dbo.Comment DROP CONSTRAINT FK_Comment_Post;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PrivacyLevelPersonal_Owner')
ALTER TABLE dbo.PrivacyLevelPersonal DROP CONSTRAINT FK_PrivacyLevelPersonal_Owner;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PrivacyLevelPersonal_Consumer')
ALTER TABLE dbo.PrivacyLevelPersonal DROP CONSTRAINT FK_PrivacyLevelPersonal_Consumer;

------------------------------------------------------------
-- Recreate FK constraints with ON DELETE CASCADE
------------------------------------------------------------

-- Journalist -> User (1:1)
ALTER TABLE dbo.Journalist
    ADD CONSTRAINT FK_Journalist_User
        FOREIGN KEY (UserUID) REFERENCES dbo.[User](UID)
ON DELETE CASCADE;

-- Activist -> User (1:1)
ALTER TABLE dbo.Activist
    ADD CONSTRAINT FK_Activist_User
        FOREIGN KEY (UserUID) REFERENCES dbo.[User](UID)
ON DELETE CASCADE;

-- Chat -> User (participants)
ALTER TABLE dbo.Chat
    ADD CONSTRAINT FK_Chat_User1
        FOREIGN KEY (User1UID) REFERENCES dbo.[User](UID)
ON DELETE CASCADE;

ALTER TABLE dbo.Chat
    ADD CONSTRAINT FK_Chat_User2
        FOREIGN KEY (User2UID) REFERENCES dbo.[User](UID)
ON DELETE CASCADE;

-- Message -> Chat (owning parent)
ALTER TABLE dbo.[Message]
    ADD CONSTRAINT FK_Message_Chat
    FOREIGN KEY (ChatUID) REFERENCES dbo.Chat(UID)
    ON DELETE CASCADE;

-- Post -> User (owner)
ALTER TABLE dbo.Post
    ADD CONSTRAINT FK_Post_User
        FOREIGN KEY (CreatorUID) REFERENCES dbo.[User](UID)
ON DELETE CASCADE;

-- Comment -> Post (owning parent)
ALTER TABLE dbo.Comment
    ADD CONSTRAINT FK_Comment_Post
        FOREIGN KEY (PostUID) REFERENCES dbo.Post(UID)
            ON DELETE CASCADE;

-- PrivacyLevelPersonal -> User (both sides)
ALTER TABLE dbo.PrivacyLevelPersonal
    ADD CONSTRAINT FK_PrivacyLevelPersonal_Owner
        FOREIGN KEY (OwnerUID) REFERENCES dbo.[User](UID)
ON DELETE CASCADE;

ALTER TABLE dbo.PrivacyLevelPersonal
    ADD CONSTRAINT FK_PrivacyLevelPersonal_Consumer
        FOREIGN KEY (ConsumerUID) REFERENCES dbo.[User](UID)
ON DELETE CASCADE;

COMMIT;