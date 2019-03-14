USE [SlackTracker]
GO

CREATE TABLE [dbo].[MessageHistory] (
  Id nvarchar(255) NOT NULL,
  Channel nvarchar(255) NOT NULL,
  [User] nvarchar(255) NOT NULL,
  Text ntext NOT NULL,
  Timestamp datetime2 NOT NULL,
  CONSTRAINT PK_MessageHistory PRIMARY KEY (Id),
  -- CONSTRAINT FK_Channels_Messages FOREIGN KEY (Channel)
  --REFERENCES dbo.Channels(Id),
  --CONSTRAINT FK_Channels_User FOREIGN KEY ([User])
  --REFERENCES dbo.Users(Id)
)
GO

CREATE INDEX IDX_Messages_User  ON [dbo].[Messages] ([User])
GO

CREATE INDEX IDX_Messages_Channel ON [dbo].[Messages] (Channel)
GO



  
  
  