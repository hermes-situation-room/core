ALTER TABLE dbo.[User]
DROP COLUMN [Password];

ALTER TABLE dbo.[User]
ADD PasswordHash VARBINARY(32) NOT NULL, -- 32 bytes for SHA256-derived hash
    PasswordSalt VARBINARY(16) NOT NULL; -- 16 bytes for random salt