alter table tCollection
add tiIsPublic [tinyint] NOT NULL DEFAULT ((0));

alter table tCollection
alter column [vchDescription] [varchar](max) NULL

INSERT INTO [dbo].[tConfigurationSetting]
           ([vchConfiguration]
           ,[vchSetting]
           ,[vchKey]
           ,[vchValue]
           ,[vchInstructions])
     VALUES
           ('dev'
           ,'Admin'
           ,'PublicCollectionTabName'
           ,'Collections'
           ,'String - The tab name of the public Special Collections'
		   );
INSERT INTO [dbo].[tConfigurationSetting]
           ([vchConfiguration]
           ,[vchSetting]
           ,[vchKey]
           ,[vchValue]
           ,[vchInstructions])
     VALUES
           ('stage'
           ,'Admin'
           ,'PublicCollectionTabName'
           ,'Collections'
           ,'String - The tab name of the public Special Collections'
		   );
INSERT INTO [dbo].[tConfigurationSetting]
           ([vchConfiguration]
           ,[vchSetting]
           ,[vchKey]
           ,[vchValue]
           ,[vchInstructions])
     VALUES
           ('prod'
           ,'Admin'
           ,'PublicCollectionTabName'
           ,'Collections'
           ,'String - The tab name of the public Special Collections'
		   );

