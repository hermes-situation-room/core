-- ========================
-- PosTag Table
-- ========================
CREATE TABLE [dbo].[UserChatReadStatus] (
    UserChatReadStatusID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    UserID UNIQUEIDENTIFIER NOT NULL,
    ChatID UNIQUEIDENTIFIER NOT NULL,
    ReadTime DATETIME NOT NULL,
    CONSTRAINT FK_UserChatReadStatus_User FOREIGN KEY (UserID) REFERENCES [dbo].[User](UID),
    CONSTRAINT FK_UserChatReadStatus_Chat FOREIGN KEY (ChatID) REFERENCES [dbo].[Chat](UID)
)

-- ========================
-- Indexes for performance
-- ========================
CREATE INDEX IX_UserChatReadStatus_User ON [dbo].[UserChatReadStatus] (UserID);
CREATE INDEX IX_UserChatReadStatus_Chat ON [dbo].[UserChatReadStatus] (ChatID);
