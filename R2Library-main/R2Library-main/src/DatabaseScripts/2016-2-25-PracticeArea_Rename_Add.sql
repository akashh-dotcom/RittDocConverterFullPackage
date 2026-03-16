
Insert Into tPracticeArea (iPracticeAreaId, vchPracticeAreaCode, vchPracticeAreaName, iSequenceNumber, vchCreatorId, dtCreationDate, tiRecordStatus)
Values (13, 'HADMIN', 'Health Administration', 130, 'SquishList #809', GETDATE(), 1)

go

Insert Into tResourcePracticeArea(iResourceId, iPracticeAreaId, vchCreatorId, dtCreationDate, tiRecordStatus)
select rs.iResourceId, 13, 'SquishList #809', GETDATE(), 1
from tSpecialty s 
join tResourceSpecialty rs on s.iSpecialtyId = rs.iSpecialtyId
where vchSpecialtyName = 'Hospitals & Administration'

go

update tSpecialty
set vchSpecialtyName = 'Health Administration'
where vchSpecialtyName = 'Hospitals & Administration'


Insert into DEV_R2Utilities..TransformQueue(resourceId, isbn, [status], dateadded)
select r.iResourceId, r.vchresourceisbn, 'A', getdate()
from tResourcePracticeArea rpa
join tResource r on rpa.iResourceId = r.iresourceId
where iPracticeAreaId = 13
group by r.iResourceId, r.vchresourceisbn
