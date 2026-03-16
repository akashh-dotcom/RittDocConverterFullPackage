insert into tProduct select 'Precision Search', 1.0D,'jharvey',getdate(), null,null, 1

alter table tproduct add tiOptional tinyint null
alter table tCartItem add tiInclude tinyint null

update tcartitem set tiinclude=1 where dtPurchaseDate is not null


update tproduct set tioptional = 0 where iProductId=1
update tproduct set tioptional = 1 where iProductId=2