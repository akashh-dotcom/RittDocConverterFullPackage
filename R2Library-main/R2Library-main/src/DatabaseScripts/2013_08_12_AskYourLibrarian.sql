ALTER TABLE [dbo].[tUser] add [tiReceiveLibrarianAlert] [tinyint] NOT NULL DEFAULT ((0));

update tUser
set tUser.tiReceiveLibrarianAlert = 1
from tUser u
join tInstitution i on u.iInstitutionId = i.iInstitutionId
where i.vchInstitutionAcctNum = u.vchUserName;