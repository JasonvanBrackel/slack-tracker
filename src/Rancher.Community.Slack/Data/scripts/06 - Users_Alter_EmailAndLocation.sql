USE [SlackTracker]
GO

ALTER TABLE dbo.Users 
  ADD EmailAddress nvarchar(1000) NULL,
      Timezone nvarchar(255) NULL,
      TimezoneLabel nvarchar(255) NULL,
      ImagePath nvarchar(2083) NULL
GO
