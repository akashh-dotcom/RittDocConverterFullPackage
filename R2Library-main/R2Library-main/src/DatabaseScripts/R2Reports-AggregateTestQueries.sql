

select datepart(yyyy, contentViewTimestamp), datepart(mm, contentViewTimestamp), count(*)
from   ContentView
group by datepart(yyyy, contentViewTimestamp), datepart(mm, contentViewTimestamp)
order by 1 desc, 2 desc

select datepart(yyyy, pageViewTimestamp), datepart(mm, pageViewTimestamp), count(*)
from   PageView
group by datepart(yyyy, pageViewTimestamp), datepart(mm, pageViewTimestamp)
order by 1 desc, 2 desc


select datepart(yyyy, searchTimestamp), datepart(mm, searchTimestamp), count(*)
from   Search
group by datepart(yyyy, searchTimestamp), datepart(mm, searchTimestamp)
order by 1 desc, 2 desc

--------------------------------------------------------------------------------

select datepart(yyyy, contentTurnawayDate), datepart(mm, contentTurnawayDate), count(*)
from   DailyContentTurnawayCount
group by datepart(yyyy, contentTurnawayDate), datepart(mm, contentTurnawayDate)
order by 1 desc, 2 desc

select datepart(yyyy, contentViewDate), datepart(mm, contentViewDate), count(*)
from   DailyContentViewCount
group by datepart(yyyy, contentViewDate), datepart(mm, contentViewDate)
order by 1 desc, 2 desc

select datepart(yyyy, pageViewDate), datepart(mm, pageViewDate), count(*)
from   DailyPageViewCount
group by datepart(yyyy, pageViewDate), datepart(mm, pageViewDate)
order by 1 desc, 2 desc

select datepart(yyyy, searchDate), datepart(mm, searchDate), count(*)
from   DailySearchCount
group by datepart(yyyy, searchDate), datepart(mm, searchDate)
order by 1 desc, 2 desc

select datepart(yyyy, sessionDate), datepart(mm, sessionDate), count(*)
from   DailySessionCount
group by datepart(yyyy, sessionDate), datepart(mm, sessionDate)
order by 1 desc, 2 desc
