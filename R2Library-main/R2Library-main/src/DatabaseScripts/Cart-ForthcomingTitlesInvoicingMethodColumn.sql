alter table tcart add tiForthcomingTitlesInvoicingMethod tinyint null

alter table tCartItem add dtPurchaseDate datetime null


update tcartitem 
set tcartitem.dtPurchaseDate = c.dtpurchasedate
from tcart c 
where c.icartid = tcartitem.icartid
and c.dtPurchaseDate is not null
