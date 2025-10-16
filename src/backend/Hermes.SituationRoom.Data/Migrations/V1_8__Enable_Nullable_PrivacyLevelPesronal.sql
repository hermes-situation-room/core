ALTER TABLE dbo.[PrivacyLevelPersonal]
ALTER COLUMN IsFirstNameVisible BIT NULL;

ALTER TABLE dbo.[PrivacyLevelPersonal]
ALTER COLUMN IsLastNameVisible BIT NULL;

ALTER TABLE dbo.[PrivacyLevelPersonal]
ALTER COLUMN IsEmailVisible BIT NULL;