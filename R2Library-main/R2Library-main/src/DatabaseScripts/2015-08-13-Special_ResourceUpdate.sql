

update tCartItem 
set vchSpecialIconName = sd.vchIconName, 
vchSpecialText = s.vchName
from tCartItem ci
join tCart c on ci.iCartId = c.iCartId and c.tiProcessed = 1
join tSpecialResource sr on ci.iResourceId = sr.iResourceId
join tSpecialDiscount sd on sr.iSpecialDiscountId = sd.iSpecialDiscountId
join tSpecial s on sd.iSpecialId = s.iSpecialId
where c.dtPurchaseDate between s.dtStartDate and s.dtEndDate
and (ci.vchSpecialText is null or ci.vchSpecialIconName is null)
and s.tiRecordStatus = 1
and sd.tiRecordStatus = 1
and sr.tiRecordStatus = 1
and c.tiRecordStatus = 1
and ci.tiRecordStatus = 1
