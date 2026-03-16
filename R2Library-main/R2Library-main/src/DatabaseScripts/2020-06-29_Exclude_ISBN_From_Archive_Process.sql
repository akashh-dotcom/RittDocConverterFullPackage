



ALTER TABLE tResource
ADD tiExcludeFromAutoArchive tinyint NOT NULL DEFAULT ((0)) 
GO


UPDATE tResource
SET tiExcludeFromAutoArchive = 1
WHERE vchIsbn13 = '9780323661386'

