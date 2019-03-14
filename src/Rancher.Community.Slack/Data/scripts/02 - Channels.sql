USE [SlackTracker]
GO

CREATE TABLE [dbo].[Channels]
(
  Id   nvarchar(255),
  Name nvarchar(255),
  CONSTRAINT PK_Channels PRIMARY KEY (Id)
)
GO

CREATE INDEX IDX_Channels_Name ON dbo.Channels (Name)
GO