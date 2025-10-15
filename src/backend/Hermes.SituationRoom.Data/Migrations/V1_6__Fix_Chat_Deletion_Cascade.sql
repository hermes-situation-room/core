-- ========================
-- Fix Chat Deletion Cascade
-- ========================

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserChatReadStatus_Chat')
ALTER TABLE [dbo].[UserChatReadStatus] DROP CONSTRAINT FK_UserChatReadStatus_Chat;

ALTER TABLE [dbo].[UserChatReadStatus]
    ADD CONSTRAINT FK_UserChatReadStatus_Chat 
    FOREIGN KEY (ChatID) REFERENCES [dbo].[Chat](UID)
    ON DELETE CASCADE;
