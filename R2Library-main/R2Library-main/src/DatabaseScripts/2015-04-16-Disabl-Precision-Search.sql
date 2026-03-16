
update tCartItem
set tiRecordStatus = 0,
vchUpdaterId = 'SquishList #680',
dtLastUpdate = getdate()
where iProductId = 2 and tiRecordStatus = 1;


Update tProduct
set tiRecordStatus = 0,
vchUpdaterId = 'SquishList #680',
dtLastUpdate = getdate()
where iProductId = 2;