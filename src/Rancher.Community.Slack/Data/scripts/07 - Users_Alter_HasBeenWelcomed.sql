USE [SlackTracker]
GO

ALTER TABLE dbo.Users ADD
	HasBeenWelcomed bit NOT NULL CONSTRAINT DF_Users_HasBeenWelcomed DEFAULT 0
GO

UPDATE dbo.Users
	SET HasBeenWelcomed = e.HasBeenWelcomed
	FROM
	dbo.Users u INNER JOIN
	dbo.UserJoinedGeneralEvents e 
		ON u.Id = e.[User]
GO