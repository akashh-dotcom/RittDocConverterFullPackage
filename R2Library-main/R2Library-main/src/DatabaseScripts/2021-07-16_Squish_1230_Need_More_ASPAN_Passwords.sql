



DECLARE @prefix char(2) = '2_'
DECLARE @creatorId varchar(50) = 'Squish #1230'

------------------------

DECLARE @PasswordAlphabet TABLE(alphanum varchar(62), symbols varchar(19))
INSERT INTO @PasswordAlphabet
SELECT 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890', '`~!@#$%^&*:,./\;''|-'

DECLARE @UserGen TABLE(username varchar(8))
INSERT INTO @UserGen
SELECT DISTINCT TOP 500
@prefix + FORMAT(CAST(LEFT(CAST(CAST(CRYPT_GEN_RANDOM(3) as int) as varchar), 6) as int), '000000')
FROM tUser

DECLARE @PasswordGen TABLE(username varchar(8), alphanum1 int, alphanum2 int, alphanum3 int, alphanum4 int, alphanum5 int, symbol1 int, symbol2 int, symbol3 int, symidx1 int, symidx2 int, symidx3 int, [password] varchar(8))
INSERT INTO @PasswordGen
SELECT TOP 500
username,
ABS(CHECKSUM(NEWID()) % 62) + 1 alphanum1,
ABS(CHECKSUM(NEWID()) % 62) + 1 alphanum2,
ABS(CHECKSUM(NEWID()) % 62) + 1 alphanum3,
ABS(CHECKSUM(NEWID()) % 62) + 1 alphanum4,
ABS(CHECKSUM(NEWID()) % 62) + 1 alphanum5,
ABS(CHECKSUM(NEWID()) % 19) + 1 symbol1, 
ABS(CHECKSUM(NEWID()) % 19) + 1 symbol2, 
ABS(CHECKSUM(NEWID()) % 19) + 1 symbol3, 
ABS(CHECKSUM(NEWID()) % 2) + 2 symidx1,
ABS(CHECKSUM(NEWID()) % 2) + 4 symidx2,
ABS(CHECKSUM(NEWID()) % 2) + 6 symidx3,
NULL
FROM @UserGen

UPDATE @PasswordGen
SET [password] = SUBSTRING(alphanum, alphanum1, 1) + SUBSTRING(alphanum, alphanum2, 1) + SUBSTRING(alphanum, alphanum3, 1) + SUBSTRING(alphanum, alphanum4, 1) + SUBSTRING(alphanum, alphanum5, 1)
FROM @PasswordGen
CROSS JOIN @PasswordAlphabet


UPDATE @PasswordGen
SET [password] = STUFF(STUFF(STUFF([password], symidx1, 0, SUBSTRING(symbols, symbol1, 1)), symidx2, 0, SUBSTRING(symbols, symbol2, 1)), symidx3, 0, SUBSTRING(symbols, symbol3, 1))
FROM @PasswordGen
CROSS JOIN @PasswordAlphabet


-----------------------------------------------

INSERT INTO tUser
SELECT 'ASPAN', 'USER', pg.userName, pg.[password], 'aspan@rittenhouse.com', 0, 0, 0, i.iInstitutionId, r.iRoleId, NULL, @creatorId, GETDATE(), NULL, NULL, 1, NULL, NULL, NULL, 0, 0, 0, 0, 0, 0, NULL, 0, 0, 0, 0, 0, 0, NULL, 0, 0, NULL, NULL, NULL, NULL, 0, 0, NULL, NULL
FROM @PasswordGen pg
LEFT JOIN tInstitution i
	ON i.vchInstitutionName = 'ASPAN'
LEFT JOIN tRole r
	ON r.vchRoleCode = 'USERS'


SELECT username
FROM @UserGen
GROUP BY username
HAVING COUNT(username) > 1










