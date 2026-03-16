INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])
VALUES ('dev','R2Utilities','Ip2LocationDownloadUrl','http://www.ip2location.com/download?login=mikemalone@technotects.com&password=Techno2016!&productcode=DB1','The URL that is used to download the Ip2Location zip file');

INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])
VALUES ('stage','R2Utilities','Ip2LocationDownloadUrl','http://www.ip2location.com/download?login=mikemalone@technotects.com&password=Techno2016!&productcode=DB1','The URL that is used to download the Ip2Location zip file');

INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])
VALUES ('prod','R2Utilities','Ip2LocationDownloadUrl','http://www.ip2location.com/download?login=mikemalone@technotects.com&password=Techno2016!&productcode=DB1','The URL that is used to download the Ip2Location zip file');


INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])
VALUES ('dev','R2Utilities','Ip2LocationFileDestinations','E:\Ip2Location\Test_1;E:\Ip2Location\Test_2','Locations for the IPCountry.csv file once extracted from the zip file. Each location is seperatd by a ;');

INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])
VALUES ('stage','R2Utilities','Ip2LocationFileDestinations','E:\Ip2Location\Test_1;E:\Ip2Location\Test_2','Locations for the IPCountry.csv file once extracted from the zip file. Each location is seperatd by a ;');

INSERT INTO [dbo].[tConfigurationSetting]([vchConfiguration],[vchSetting],[vchKey],[vchValue],[vchInstructions])
VALUES ('prod','R2Utilities','Ip2LocationFileDestinations','E:\Ip2Location\Test_1;E:\Ip2Location\Test_2','Locations for the IPCountry.csv file once extracted from the zip file. Each location is seperatd by a ;');


alter table tConfigurationSetting
alter column vchValue varchar(max)

