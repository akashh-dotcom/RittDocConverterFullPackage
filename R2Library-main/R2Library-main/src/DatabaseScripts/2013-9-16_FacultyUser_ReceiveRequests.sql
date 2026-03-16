

alter table tUser
add tiReceiveFacultyUserRequests tinyint not null default((0));



UPDATE     tUser
SET tiReceiveFacultyUserRequests = 1
FROM tUser u
JOIN tInstitution i on u.iInstitutionId = i.iInstitutionId and u.vchUserName = i.vchInstitutionAcctNum

