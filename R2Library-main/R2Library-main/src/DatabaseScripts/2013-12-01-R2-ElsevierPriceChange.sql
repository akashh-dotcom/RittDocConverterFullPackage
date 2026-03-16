BULK INSERT dbo.tResourcePriceUpdate FROM 'D:\Clients\Rittenhouse\R2Library\R2-ElsevierPriceChange.txt'
WITH (
   DATAFILETYPE = 'char',
   FIELDTERMINATOR = '\t',
   ROWTERMINATOR = '\n'
)

select rpu1.isbn, (select count(*) from dbo.tResourcePriceUpdate rpu2 where rpu2.isbn = rpu1.isbn and rpu2.price = rpu1.price)
from   dbo.tResourcePriceUpdate rpu1

select rpu1.isbn, max(price)
from   dbo.tResourcePriceUpdate rpu1
group by isbn
order by max(price) desc

select rpu1.isbn, max(price)
from   dbo.tResourcePriceUpdate rpu1
 join  dbo.tResource r on r.vchIsbn13 = rpu1.isbn and 
group by isbn


update r1
set    r1.decResourcePrice = mx.maxPrice
from   dbo.tResource r1
 join ( select rpu1.isbn, max(price) as maxPrice
        from   dbo.tResourcePriceUpdate rpu1
         join  dbo.tResource r2 on r2.vchIsbn13 = rpu1.isbn
        group by isbn
       ) as mx on mx.isbn = r1.vchIsbn13

select rpu1.isbn, r.decResourcePrice, max(price)
from   dbo.tResourcePriceUpdate rpu1
 join  dbo.tResource r on r.vchIsbn13 = rpu1.isbn 
group by isbn, r.decResourcePrice



--'9781455712212',
--'9781455707904',
--'9781455706051',
--'9781437717884',
--'9781437717884',
--'9781437709223',
--'9781437701555',
--'9781416056423',
--'9781416053163',
--'9781416040019',
--'9780729582018',
--'9780729581769',
--'9780729541008',
--'9780729539487',
--'9780702034268',
--'9780323186117',
--'9780323112376',

--select * from tResourcePriceUpdate where isbn in (
--'9780323084789',
--'9781437707311',
--'9780729538923',
--'9780729538749',
--'9781437700121')


