-- ========================
-- add post privacy level
-- ========================
ALTER TABLE dbo.[Post]
ADD PrivacyLevel INT NOT NULL DEFAULT 0;
