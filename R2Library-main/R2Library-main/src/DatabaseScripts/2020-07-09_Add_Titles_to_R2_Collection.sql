



SELECT *
FROM tInstitutionResourceLicense
ORDER BY dtCreationDate DESC



--INSERT INTO tInstitutionResource ir
SELECT iInstitutionId, r.iResourceId, 1, lt.tiLicenseType, los.tiLicenseOriginalSourceId, GETDATE(), NULL, NULL, NULL, 0, 0, GETDATE(), 'SquishList #1186', NULL, NULL, 1, 
	NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
FROM tInstitution i
CROSS JOIN (
	SELECT iResourceId
	FROM tResource
	WHERE tiRecordStatus = 1
		AND iResourceId NOT IN (
			SELECT iResourceId
			FROM tInstitutionResourceLicense ir
			INNER JOIN tInstitution i
				ON i.iInstitutionId = ir.iInstitutionId
			WHERE vchInstitutionAcctNum = '023973'
		)
		AND vchIsbn13 IN (
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
	'9781455750856',
	'9781451186277',
	'9781455706952',
	'9781416040019',
	'9781284127270',
	'9781284154214',
	'9781635851410'
	)
) r
CROSS JOIN tLicenseType lt	
CROSS JOIN tLicenseOriginalSource los	
WHERE vchInstitutionAcctNum = '023973' AND lt.LicenseTypeDescription = 'Purchased' AND los.LicenseOriginalSourceDescription = 'Firm Order'



