
alter table tProduct add tiAgreement tinyint 

update tproduct set tiAgreement = 1

update tproduct set decprice = 495 where iProductId=2


alter table tCartItem add tiAgree tinyint