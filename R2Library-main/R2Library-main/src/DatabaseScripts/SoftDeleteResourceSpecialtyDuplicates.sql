
declare @resourceId as int
declare @specialtyId as int
declare @count as int 
declare @minResourceSpecialtyId as int

declare rsCursor cursor for 
    select rs.iResourceId, rs.ispecialtyId, count(*)
    from   tResourceSpecialty rs
    where  tiRecordStatus = 1
    group by rs.iResourceId, rs.ispecialtyId
    having count(*) > 2
    order by count(*) desc

open rsCursor   
fetch next from rsCursor into @resourceId, @specialtyId, @count

while @@FETCH_STATUS = 0  
begin
	print 'resourceId: ' + cast(@resourceId as varchar) + ', specialtyId: ' + cast(@specialtyId as varchar) + ', count: ' + cast(@count as varchar)

    select @minResourceSpecialtyId = min(iResourceSpecialtyId)
    from   tResourceSpecialty
    where  iResourceId = @resourceId and ispecialtyId = @specialtyId

    print ' - minResourceSpecialtyId: ' + cast(@minResourceSpecialtyId as varchar)

    update tResourceSpecialty
    set    tiRecordStatus = 0, vchUpdaterId = 'cleanupScript', dtLastUpdate = getdate()
    where  iResourceId = @resourceId and ispecialtyId = @specialtyId and iResourceSpecialtyId <> @minResourceSpecialtyId

    print ' - rows updated: ' + cast(@@ROWCOUNT as varchar)

	fetch next from rsCursor into @resourceId, @specialtyId, @count
end

close rsCursor  
deallocate rsCursor

