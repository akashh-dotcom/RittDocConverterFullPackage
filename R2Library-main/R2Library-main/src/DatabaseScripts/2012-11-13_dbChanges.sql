/****** Object: Procedure [dbo].[sp_R2UtilitiesTransformQueueInsert]   Script Date: 11/13/2012 3:31:05 PM ******/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE [dbo].[sp_R2UtilitiesTransformQueueInsert]
@ResourceId int, @Isbn varchar(50), @Status char(1)
WITH EXEC AS CALLER
AS
declare @RecordCount as int;

select @RecordCount = count(*)
from   R2Utilities.dbo.TransformQueue
where  resourceId = @ResourceId
  and  isbn = @Isbn
  and  status = @Status

if (@RecordCount = 0)
begin
    insert into R2Utilities.dbo.TransformQueue (resourceId, isbn, status, dateAdded)
    values (@ResourceId, @Isbn, @Status, getdate());
end

select @@ROWCOUNT;
GO

