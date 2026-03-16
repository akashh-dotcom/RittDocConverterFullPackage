USE [RIT001_2012-03-21]
GO

/****** Object:  FullTextCatalog [[UserSearch]    Script Date: 6/28/2012 11:52:08 AM ******/
	CREATE FULLTEXT CATALOG [UserSearch]WITH ACCENT_SENSITIVITY = OFF

GO

	CREATE FULLTEXT INDEX ON [dbo].[tUser]
	(
		[vchLastName] Language 1033,
		[vchFirstName] Language 1033,
		[vchUserEmail] Language 1033
	)
	KEY INDEX [tUser_PK] on [UserSearch]
GO

	CREATE FULLTEXT INDEX ON [dbo].[tInstitution]
	(
		[vchInstitutionName] Language 1033
	)

	KEY INDEX [tInstitution_PK] on [UserSearch]
GO




