CREATE TABLE [dbo].[tConfigurationSettingAudit](
	[iConfigurationSettingAuditId] [int] IDENTITY(1,1) NOT NULL,
	[iConfigurationSettingId] [int],
	[vchConfiguration] [varchar](255) NULL,
	[vchSetting] [varchar](255) NULL,
	[vchKey] [varchar](255) NULL,
	[vchValue] [varchar](max) NULL,
	[vchInstructions] [varchar](255) NULL,
	[dtCreationDate] [datetime] NULL,
	[vchEventType] [varchar] (50) NULL,
 CONSTRAINT [PK_tConfigurationSettingAudit] PRIMARY KEY CLUSTERED 
(
	[iConfigurationSettingAuditId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 95) ON [PRIMARY]
) ON [PRIMARY]

go

create trigger tConfigurationSettingAuditRecordInsert on tConfigurationSetting
after insert
as
begin
SET NOCOUNT ON;
  insert into tConfigurationSettingAudit 
  (iConfigurationSettingId, vchConfiguration, vchSetting, vchKey, vchValue, vchInstructions, dtCreationDate, vchEventType)
  select i.iConfigurationSettingId, i.vchConfiguration, i.vchSetting, i.vchKey, i.vchValue, i.vchInstructions, getdate(), 'insert'
  from inserted i
SET NOCOUNT OFF;  
end
go

create trigger tConfigurationSettingAuditRecordUpdate on tConfigurationSetting
after update
as
begin
SET NOCOUNT ON;
  insert into tConfigurationSettingAudit 
  (iConfigurationSettingId, vchConfiguration, vchSetting, vchKey, vchValue, vchInstructions, dtCreationDate, vchEventType)
  select i.iConfigurationSettingId, i.vchConfiguration, i.vchSetting, i.vchKey, i.vchValue, i.vchInstructions, getdate(), 'update - before'
  from  deleted i;
    insert into tConfigurationSettingAudit 
  (iConfigurationSettingId, vchConfiguration, vchSetting, vchKey, vchValue, vchInstructions, dtCreationDate, vchEventType)
  select i.iConfigurationSettingId, i.vchConfiguration, i.vchSetting, i.vchKey, i.vchValue, i.vchInstructions, getdate(), 'update - after'
  from  inserted i
SET NOCOUNT OFF;  
end
go

create trigger tConfigurationSettingAuditRecordDelete on tConfigurationSetting
after delete
as
begin
SET NOCOUNT ON;
  insert into tConfigurationSettingAudit 
  (iConfigurationSettingId, vchConfiguration, vchSetting, vchKey, vchValue, vchInstructions, dtCreationDate, vchEventType)
  select i.iConfigurationSettingId, i.vchConfiguration, i.vchSetting, i.vchKey, i.vchValue, i.vchInstructions, getdate(), 'delete'
  from deleted i
SET NOCOUNT OFF;  
end
go

INSERT INTO [dbo].[tConfigurationSetting]
           ([vchConfiguration]
           ,[vchSetting]
           ,[vchKey]
           ,[vchValue]
           ,[vchInstructions])
     VALUES
           ('dev'
           , 'Web'
           , 'AdminControllAccess'
           , 'Kenhaberle@technotects.com;scottscheider@technotects.com;davejones@technotects.com'
           , 'string value - email address seperated by a ; that are given access to admin controller. Should only be Technotects employees.')
		   
		   
INSERT INTO [dbo].[tConfigurationSetting]
           ([vchConfiguration]
           ,[vchSetting]
           ,[vchKey]
           ,[vchValue]
           ,[vchInstructions])
     VALUES
           ('dev'
           , 'Admin'
           , 'WindowsServiceConfigurationFile'
           , 'D:\Clients\Rittenhouse\R2Library\WindowsService\Service\R2V2.WindowsService.exe.config'
           , 'string value - Location on disk of the WindowsSerivce configuration file seperated by ; This is used show all settings that will be loaded for the service.')

INSERT INTO [dbo].[tConfigurationSetting]
           ([vchConfiguration]
           ,[vchSetting]
           ,[vchKey]
           ,[vchValue]
           ,[vchInstructions])
     VALUES
           ('dev'
           , 'Admin'
           , 'UtilitiesConfigurationFile'
           , 'D:\Clients\Rittenhouse\R2Library\Utilities\App\R2Utilities.exe.config'
           , 'string value - Location on disk of the Utilties configuration file seperated by ; This is used show all settings that will be loaded for the application.')		   


