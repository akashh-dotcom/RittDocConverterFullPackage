alter table [dbo].[tResource]
add dec3BundlePrice decimal(12,2) null;

alter table tCartItem
add tiBundle [tinyint] NULL;

alter table tCartItem
add decBundlePrice decimal(12,2) null;


alter table tOrderHistoryItem
add tiBundle [tinyint] NULL;

alter table tOrderHistoryItem
add decBundlePrice decimal(12,2) null;

--select * from [tResource] where dec3BundlePrice is not null

--select top 10 * from tCartItem ci order by ci.iCartItemId desc