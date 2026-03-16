
--Create institution in DEV database first!


--select * from tInstitution where vchInstitutionAcctNum = '008862'
--select * from [tInstitutionResourceLicense] where iInstitutionId = 3864 and iresourceid = 4940
--select * from tResource where vchResourceISBN = '1451176791'


DECLARE @InstitutionAcctNum varchar(20);
DECLARE @LicenseCount int;
DECLARE @InstitutionId int;
Declare @CreatorId varchar(50);

--Inputs
set @InstitutionAcctNum = '023973';
set @LicenseCount = 1;
set @CreatorId = 'SquishList #1186';

select @InstitutionId = iInstitutionId from tInstitution where vchInstitutionAcctNum = @InstitutionAcctNum



INSERT INTO [dbo].[tInstitutionResourceLicense]([iInstitutionId], [iResourceId], [iLicenseCount], [tiLicenseTypeId], [tiLicenseOriginalSourceId] , [dtFirstPurchaseDate], [iPdaViewCount],
	[iPdaMaxViews], [dtCreationDate], [vchCreatorId], [tiRecordStatus])
select @InstitutionId, r.iResourceId, @LicenseCount, 1, 1, getdate(), 0, 0, getdate(), @CreatorId, 1
from tResource r
where r.vchIsbn13 in (

'9780521697163',
'9781437715279',
'9781933864617',
'9780781765800',
'9780323072427',
'9781620701072',
'9781620701256',
'9781604069044',
'9781451176056',
'9780323299565',
'9780702075599',
'9780702075599',
'9780781765800',
'9780702031717',
'9781496377234',
'9780781765800',
'9781585285365',
'9781585624928',
'9781585625079',
'9781585624966',
'9781607951889',
'9781451186277',
'9781416040019',
'9781284127270',
'9781284154214',
'9781635851410'


)
AND r.vchIsbn13 NOT IN (
'9780781765800',
'9781496377234',
'9780702075599'
	)



INSERT INTO [dbo].[tInstitutionResourceAudit] ([iInstitutionId], [iResourceId], [iInstitutionResourceAuditTypeId], [iLicenseCount], [decSingleLicensePrice], [vchCreatorId], [dtCreationDate],
	[vchEventDescription], [bLegacy])
select @InstitutionId , iResourceId, 5, @LicenseCount, 0.00, @CreatorId, getdate(), @CreatorId, 0
from tResource where iResourceId in (
	select irl.iResourceId from tInstitutionResourceLicense irl where vchCreatorId = @CreatorId
);






--Only run this if updating a trial institution to active
Update tInstitution
set iInstitutionAcctStatusId = 1,
vchUpdaterId = @CreatorId,
dtLastUpdate = getdate()
where iInstitutionId = @InstitutionId



--------------------------------------------------------------------------------------------------------------

--Pre-creation checks

DECLARE @InstitutionAcctNum varchar(20);

--Inputs
set @InstitutionAcctNum = '023973';

SELECT  *
FROM tInstitutionResourceLicense irl
INNER JOIN tInstitution i ON i.iInstitutionId = irl.iInstitutionId AND i.vchInstitutionAcctNum = @InstitutionAcctNum 
INNER JOIN tResource r ON r.iResourceId = irl.iResourceId
	AND r.vchIsbn13 IN (


	
'9780521697163',
'9781437715279',
'9781933864617',
'9780781765800',
'9780323072427',
'9781620701072',
'9781620701256',
'9781604069044',
'9781451176056',
'9780323299565',
'9780702075599',
'9780702075599',
'9780781765800',
'9780702031717',
'9781496377234',
'9780781765800',
'9781585285365',
'9781585624928',
'9781585625079',
'9781585624966',
'9781607951889',
'9781451186277',
'9781416040019',
'9781284127270',
'9781284154214',
'9781635851410'
)
AND r.vchIsbn13 NOT IN (
'9780781765800',
'9781496377234',
'9780702075599'
	)


--------------------------------------------------------------------------------------------------------------
--Post-creation checks


DECLARE @InstitutionAcctNum varchar(20);
DECLARE @LicenseCount int;
DECLARE @InstitutionId int;
Declare @CreatorId varchar(50);

--Inputs
set @InstitutionAcctNum = '023973';
set @LicenseCount = 1;
set @CreatorId = 'SquishList #1186';


SELECT *
FROM tInstitutionResourceLicense
WHERE vchCreatorId = @CreatorId



SELECT *
FROM tInstitutionResourceAudit
WHERE vchCreatorId = @CreatorId



SELECT *
FROM tInstitution 
WHERE vchInstitutionAcctNum = @InstitutionAcctNum

