
declare @resourceId as int
declare @practiceAreaId as int
declare @count as int 
declare @minResourcePracticeAreaId as int

declare rpaCursor cursor for 
    select rpa.iResourceId, rpa.iPracticeAreaId, count(*)
    from   tResourcePracticeArea rpa
    where  tiRecordStatus = 1
    group by rpa.iResourceId, rpa.iPracticeAreaId
    having count(*) > 2
    order by count(*) desc

open rpaCursor   
fetch next from rpaCursor into @resourceId, @practiceAreaId, @count

while @@FETCH_STATUS = 0  
begin
	print 'resourceId: ' + cast(@resourceId as varchar) + ', practiceAreaId: ' + cast(@practiceAreaId as varchar) + ', count: ' + cast(@count as varchar)

    select @minResourcePracticeAreaId = min(iResourcePracticeAreaId)
    from   tResourcePracticeArea
    where  iResourceId = @resourceId and iPracticeAreaId = @practiceAreaId

    print ' - minResourcePracticeAreaId: ' + cast(@minResourcePracticeAreaId as varchar)

    update tResourcePracticeArea
    set    tiRecordStatus = 0, vchUpdaterId = 'cleanupScript', dtLastUpdate = getdate()
    where  iResourceId = @resourceId and iPracticeAreaId = @practiceAreaId and iResourcePracticeAreaId <> @minResourcePracticeAreaId

    print ' - rows updated: ' + cast(@@ROWCOUNT as varchar)

	fetch next from rpaCursor into @resourceId, @practiceAreaId, @count
end

close rpaCursor  
deallocate rpaCursor


select * from [TECHNOSERV04\SQL2005].PreludeData.dbo.Product

