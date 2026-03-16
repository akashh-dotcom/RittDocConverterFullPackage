
INSERT INTO [dbo].[tConfigurationSetting]
           ([vchConfiguration]
           ,[vchSetting]
           ,[vchKey]
           ,[vchValue]
           ,[vchInstructions])
     VALUES
           ('dev'
           ,'R2Utilities'
           ,'EIsbnGetUrl'
           ,'http://dev-productsdb.rittenhouse.com/onixeisbn/getonixeisbnsbyisbn'
           ,'URL to get EISBNs from ONIX in UpdateWithOnixDataTask')
GO
