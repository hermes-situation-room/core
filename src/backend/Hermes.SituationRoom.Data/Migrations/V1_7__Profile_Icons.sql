-- Add profile icon and color fields to user table
ALTER TABLE dbo.[User]
ADD ProfileIcon NVARCHAR(50) NOT NULL DEFAULT 'User',
    ProfileIconColor NVARCHAR(50) NOT NULL DEFAULT 'Blue';
