USE [master]
GO

/****** Object:  Database [R2Utilities]    Script Date: 03/26/2012 11:27:17 ******/
CREATE DATABASE [R2Utilities] ON  PRIMARY 
( NAME = N'R2Utilities', FILENAME = N'D:\MSSQL\DATA\R2Utilities.mdf' , SIZE = 3072KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'R2Utilities_log', FILENAME = N'C:\MSSQL\LOGS\R2Utilities_log.ldf' , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO

ALTER DATABASE [R2Utilities] SET COMPATIBILITY_LEVEL = 90
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [R2Utilities].[dbo].[sp_fulltext_database] @action = 'disable'
end
GO

ALTER DATABASE [R2Utilities] SET ANSI_NULL_DEFAULT OFF 
GO

ALTER DATABASE [R2Utilities] SET ANSI_NULLS OFF 
GO

ALTER DATABASE [R2Utilities] SET ANSI_PADDING OFF 
GO

ALTER DATABASE [R2Utilities] SET ANSI_WARNINGS OFF 
GO

ALTER DATABASE [R2Utilities] SET ARITHABORT OFF 
GO

ALTER DATABASE [R2Utilities] SET AUTO_CLOSE OFF 
GO

ALTER DATABASE [R2Utilities] SET AUTO_CREATE_STATISTICS ON 
GO

ALTER DATABASE [R2Utilities] SET AUTO_SHRINK OFF 
GO

ALTER DATABASE [R2Utilities] SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE [R2Utilities] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE [R2Utilities] SET CURSOR_DEFAULT  GLOBAL 
GO

ALTER DATABASE [R2Utilities] SET CONCAT_NULL_YIELDS_NULL OFF 
GO

ALTER DATABASE [R2Utilities] SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE [R2Utilities] SET QUOTED_IDENTIFIER OFF 
GO

ALTER DATABASE [R2Utilities] SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE [R2Utilities] SET  DISABLE_BROKER 
GO

ALTER DATABASE [R2Utilities] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE [R2Utilities] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE [R2Utilities] SET TRUSTWORTHY OFF 
GO

ALTER DATABASE [R2Utilities] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE [R2Utilities] SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE [R2Utilities] SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE [R2Utilities] SET  READ_WRITE 
GO

ALTER DATABASE [R2Utilities] SET RECOVERY SIMPLE 
GO

ALTER DATABASE [R2Utilities] SET  MULTI_USER 
GO

ALTER DATABASE [R2Utilities] SET PAGE_VERIFY CHECKSUM  
GO

ALTER DATABASE [R2Utilities] SET DB_CHAINING OFF 
GO


-- ********************************************************************************
-- CREATE TABLES
-- ********************************************************************************
USE [R2Utilities]
GO

/****** Object:  Table [dbo].[ExtractedResource]    Script Date: 03/26/2012 11:28:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[ExtractedResource](
	[extractedResourceId] [int] IDENTITY(1,1) NOT NULL,
	[resourceId] [int] NULL,
	[isbn] [varchar](20) NOT NULL,
	[dateCompleted] [datetime] NOT NULL,
	[successful] [bit] NOT NULL,
	[results] [varchar](max) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[extractedResourceId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO


CREATE TABLE [dbo].[ModifiedResource](
	[modifiedResourceId] [int] IDENTITY(1,1) NOT NULL,
	[resourceId] [int] NULL,
	[isbn] [varchar](20) NOT NULL,
	[dateCompleted] [datetime] NOT NULL,
	[successful] [bit] NOT NULL,
	[results] [varchar](max) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[modifiedResourceId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[IndexedResource](
	[indexedResourceId] [int] IDENTITY(1,1) NOT NULL,
	[resourceId] [int] NULL,
	[isbn] [varchar](20) NOT NULL,
	[dateCompleted] [datetime] NOT NULL,
	[successful] [bit] NOT NULL,
	[results] [varchar](max) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[indexedResourceId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO



/****** Object:  Table [dbo].[TaskResult]    Script Date: 03/26/2012 11:29:09 ******/
CREATE TABLE [dbo].[TaskResult](
	[taskResultId] [int] IDENTITY(1,1) NOT NULL,
	[taskName] [varchar](255) NOT NULL,
	[taskStartTime] [datetime] NOT NULL,
	[taskEndTime] [datetime] NULL,
	[taskCompletedSuccessfully] [bit] NOT NULL,
	[taskResults] [varchar](max) NULL,
 CONSTRAINT [PK__TaskResult__66603565] PRIMARY KEY CLUSTERED 
(
	[taskResultId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[TaskResult] ADD  DEFAULT ((0)) FOR [taskCompletedSuccessfully]
GO






/****** Object:  Table [dbo].[TaskResultStep]    Script Date: 03/26/2012 11:29:29 ******/
CREATE TABLE [dbo].[TaskResultStep](
	[taskResultStepId] [int] IDENTITY(1,1) NOT NULL,
	[taskResultId] [int] NOT NULL,
	[stepName] [varchar](255) NOT NULL,
	[stepStartTime] [datetime] NOT NULL,
	[stepEndTime] [datetime] NULL,
	[stepCompletedSuccessfully] [bit] NOT NULL,
	[stepResults] [varchar](max) NULL,
 CONSTRAINT [PK__TaskResultStep__693CA210] PRIMARY KEY CLUSTERED 
(
	[taskResultStepId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 80) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[TaskResultStep]  WITH CHECK ADD  CONSTRAINT [FK_TaskResultStep_TaskResult] FOREIGN KEY([taskResultId])
REFERENCES [dbo].[TaskResult] ([taskResultId])
GO

ALTER TABLE [dbo].[TaskResultStep] CHECK CONSTRAINT [FK_TaskResultStep_TaskResult]
GO

ALTER TABLE [dbo].[TaskResultStep] ADD  DEFAULT ((0)) FOR [stepCompletedSuccessfully]
GO


