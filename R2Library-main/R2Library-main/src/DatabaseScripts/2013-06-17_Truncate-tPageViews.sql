select min(dtCreationDate) from tPageViews where iPageViewId < 1000

select count(*) from [DEV_RIT001].dbo.tPageViews where dtCreationDate between '01/1/2005 0:00:00' and '12/31/2005 23:59:59.999' -- 611,800
select count(*) from [DEV_RIT001].dbo.tPageViews where dtCreationDate between '01/1/2006 0:00:00' and '12/31/2006 23:59:59.999' -- 884,533
select count(*) from [DEV_RIT001].dbo.tPageViews where dtCreationDate between '01/1/2007 0:00:00' and '12/31/2007 23:59:59.999' -- 2,038,314
select count(*) from [DEV_RIT001].dbo.tPageViews where dtCreationDate between '01/1/2008 0:00:00' and '12/31/2008 23:59:59.999' -- 3,216,220
select count(*) from [DEV_RIT001].dbo.tPageViews where dtCreationDate between '01/1/2009 0:00:00' and '12/31/2009 23:59:59.999' -- 9,074,418
select count(*) from [DEV_RIT001].dbo.tPageViews where dtCreationDate between '01/1/2010 0:00:00' and '12/31/2010 23:59:59.999' -- 6,372,181
select count(*) from [DEV_RIT001].dbo.tPageViews where dtCreationDate between '01/1/2011 0:00:00' and '12/31/2011 23:59:59.999' -- 6,983,469
select count(*) from [DEV_RIT001].dbo.tPageViews where dtCreationDate between '01/1/2012 0:00:00' and '12/31/2012 23:59:59.999' -- 6,845,884
select count(*) from [DEV_RIT001].dbo.tPageViews where dtCreationDate between '01/1/2013 0:00:00' and '12/31/2013 23:59:59.999' -- 0

truncate table tPageViews

