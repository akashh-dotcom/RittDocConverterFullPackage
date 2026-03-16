

--DEV Config Settings for OpenAthens Keystone (OpenID Connect)
INSERT INTO tConfigurationSetting
SELECT 'dev', 'Oidc', 'Authority', 'https://connect.openathens.net', 'string value - URL of OpenId Connect issuing authority'

INSERT INTO tConfigurationSetting
SELECT 'dev', 'Oidc', 'RedirectUrl', 'https://dev.r2library.com/Authentication/AthensLogin', 'string value - RedirectUrl set in the OpenAthens publisher dashboard'

INSERT INTO tConfigurationSetting
SELECT 'dev', 'Oidc', 'ClientId', 'rittenhouse.com.oidc-app-v1.4313c6e1-1610-46a7-96db-290a37160dfc', 'string value - ClientId from the OIDC application record created in the OpenAthens publisher dashboard'

INSERT INTO tConfigurationSetting
SELECT 'dev', 'Oidc', 'ClientSecret', 'rg5AwmVprQrHVNJbO9nRjbxeT', 'string value - ClientSecret from the OIDC application created in the OpenAthens publisher dashboard'





--STAGE Config Settings for OpenAthens Keystone (OpenID Connect)
INSERT INTO tConfigurationSetting
SELECT 'stage', 'Oidc', 'Authority', 'https://connect.openathens.net', 'string value - URL of OpenId Connect issuing authority'

INSERT INTO tConfigurationSetting
SELECT 'stage', 'Oidc', 'RedirectUrl', 'https://stage.r2library.com/Authentication/AthensLogin', 'string value - RedirectUrl set in the OpenAthens publisher dashboard'

INSERT INTO tConfigurationSetting
SELECT 'stage', 'Oidc', 'ClientId', 'rittenhouse.com.oidc-app-v1.7a35c92c-b5b2-40d8-8630-4ce5c039afb8', 'string value - ClientId from the OIDC application record created in the OpenAthens publisher dashboard'

INSERT INTO tConfigurationSetting
SELECT 'stage', 'Oidc', 'ClientSecret', '2bgmsWAXP3zhNmWHYTKHy5R7b', 'string value - ClientSecret from the OIDC application created in the OpenAthens publisher dashboard'






--PROD Config Settings for OpenAthens Keystone (OpenID Connect)
INSERT INTO tConfigurationSetting
SELECT 'prod', 'Oidc', 'Authority', 'https://connect.openathens.net', 'string value - URL of OpenId Connect issuing authority'

INSERT INTO tConfigurationSetting
SELECT 'prod', 'Oidc', 'RedirectUrl', 'https://r2library.com/Authentication/AthensLogin', 'string value - RedirectUrl set in the OpenAthens publisher dashboard'

INSERT INTO tConfigurationSetting
SELECT 'prod', 'Oidc', 'ClientId', 'rittenhouse.com.oidc-app-v1.7a35c92c-b5b2-40d8-8630-4ce5c039afb8', 'string value - ClientId from the OIDC application record created in the OpenAthens publisher dashboard'

INSERT INTO tConfigurationSetting
SELECT 'prod', 'Oidc', 'ClientSecret', '2bgmsWAXP3zhNmWHYTKHy5R7b', 'string value - ClientSecret from the OIDC application created in the OpenAthens publisher dashboard'

