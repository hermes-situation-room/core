-- ========================
-- Migration V1.12: Admin Role
-- Description: Adds IsAdmin flag to User for admin role gating
-- ========================

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.[User]') AND name = 'IsAdmin')
BEGIN
    ALTER TABLE dbo.[User]
    ADD IsAdmin BIT NOT NULL CONSTRAINT DF_User_IsAdmin DEFAULT 0;
END
GO
