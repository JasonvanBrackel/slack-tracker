USE [SlackTracker]

CREATE TABLE dbo.UserJoinedGeneralEvents (
  [User] nvarchar(255),
  Timestamp datetime2,
  HasBeenWelcomed bit DEFAULT (0),
  CONSTRAINT PK_UserJoinedGeneralEvents PRIMARY KEY ([User])
)
GO