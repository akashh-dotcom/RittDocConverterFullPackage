USE [RIT001_2012-03-21]
GO


ALTER TABLE tuser
  ADD dtLastSession  smalldatetime;

update tuser
set dtLastSession = (select top(1) a.dtSessionStartTime from tApplicationSession a
where u.iuserId = a.iUserId and a.iUserId is not null
order by a.dtSessionStartTime desc)
from tuser u

