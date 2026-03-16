


Update tResource
set decResourcePrice = 0
where tiFreeResource = 1;

update tCartItem
set decDiscountPrice = 0,
decListPrice = 0
from tCartItem ci
join tResource r on ci.iResourceId = r.iResourceId
where r.tiFreeResource = 1;