
update tOrderHistory 
set iDiscountTypeId = 3
from tCart c
join tOrderHistory oh on c.iCartId = oh.iCartId
where c.tiProcessed = 1 and c.iCartTypeId = 3