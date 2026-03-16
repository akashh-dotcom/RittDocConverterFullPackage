
insert into tConfigurationSetting (vchConfiguration, vchSetting, vchKey, vchValue, vchInstructions)
values ('dev', 'Content', 'TransformInfoThresholdInMilliseconds',  '500',  'This setting dictates the threshold for transform info messages via log4net');

insert into tConfigurationSetting (vchConfiguration, vchSetting, vchKey, vchValue, vchInstructions)
values ('dev', 'Content', 'TransformWarnThresholdInMilliseconds',  '1000', 'This setting dictates the threshold for transform warning messages via log4net');

insert into tConfigurationSetting (vchConfiguration, vchSetting, vchKey, vchValue, vchInstructions)
values ('dev', 'Content', 'TransformErrorThresholdInMilliseconds',  '2000', 'This setting dictates the threshold for transform error messages via log4net');


select * from tConfigurationSetting
where  vchKey like 'Transform%'
