alter table R2library
add [referrer] [varchar](255) NULL;

ALTER TABLE R2library
ADD newExceptionHash [nvarchar](32) NULL;
go
UPDATE R2library
SET newExceptionHash = exceptionHash;
go
-- Delete the persisted column
ALTER TABLE R2library
   DROP COLUMN exceptionHash;
go
-- Rename new column to old name
EXEC sp_rename 'R2library.newExceptionHash', 'exceptionHash', 'COLUMN';
