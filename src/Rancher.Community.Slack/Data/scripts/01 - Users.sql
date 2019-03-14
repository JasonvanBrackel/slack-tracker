USE [SlackTracker]
GO

CREATE TABLE dbo.Users (
  Id nvarchar(255),
  Username nvarchar(255),
  Name nvarchar(255), 
  CONSTRAINT PK_Users PRIMARY KEY (Id) 
)

CREATE INDEX IDX_Users_Name ON dbo.Users (Name)