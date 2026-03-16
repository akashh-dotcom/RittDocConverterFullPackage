


--*** Run this on DEV ***
INSERT INTO tConfigurationSetting
SELECT 'dev', 'Content', 'IgnoreExternalEntities', 'true', 'boolean value - set to true to ignore external entities in book.xml file. This speeds up the book.xml transform.'


--*** Run this on STAGE ***
INSERT INTO tConfigurationSetting
SELECT 'stage', 'Content', 'IgnoreExternalEntities', 'true', 'boolean value - set to true to ignore external entities in book.xml file. This speeds up the book.xml transform.'


--*** Run this on PROD - value set to false until we are ready to turn on this feature ***
INSERT INTO tConfigurationSetting
SELECT 'prod', 'Content', 'IgnoreExternalEntities', 'false', 'boolean value - set to true to ignore external entities in book.xml file. This speeds up the book.xml transform.'