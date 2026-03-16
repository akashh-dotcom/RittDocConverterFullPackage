

alter table CustomerType
add r2Type varchar(50) null;

go

update CustomerType
set r2Type = 'Business to Business' where customerTypeCode = 'IA'
update CustomerType
set r2Type = 'Business to Business' where customerTypeCode = 'IC'
update CustomerType
set r2Type = 'Hospital' where customerTypeCode = 'ID'
update CustomerType
set r2Type = 'Hospital' where customerTypeCode = 'IG'
update CustomerType
set r2Type = 'Hospital' where customerTypeCode = 'IH'
update CustomerType
set r2Type = 'Hospital' where customerTypeCode = 'IN'
update CustomerType
set r2Type = 'Hospital' where customerTypeCode = 'IP'
update CustomerType
set r2Type = 'Hospital' where customerTypeCode = 'IR'
update CustomerType
set r2Type = 'Business to Business' where customerTypeCode = 'IT'
update CustomerType
set r2Type = '2-Year Library' where customerTypeCode = 'IV'
update CustomerType
set r2Type = 'Business to Business' where customerTypeCode = 'IX'
update CustomerType
set r2Type = '2-Year Library' where customerTypeCode = 'L2G'
update CustomerType
set r2Type = '2-Year Library' where customerTypeCode = 'L2N'
update CustomerType
set r2Type = '4-Year Library' where customerTypeCode = 'L4G'
update CustomerType
set r2Type = '4-Year Library' where customerTypeCode = 'L4N'
update CustomerType
set r2Type = 'Hospital' where customerTypeCode = 'LHG'
update CustomerType
set r2Type = 'Hospital' where customerTypeCode = 'LHN'
update CustomerType
set r2Type = 'Medical Library' where customerTypeCode = 'LMG'
update CustomerType
set r2Type = 'Medical Library' where customerTypeCode = 'LMN'
update CustomerType
set r2Type = 'Special Library' where customerTypeCode = 'LPG'
update CustomerType
set r2Type = 'Special Library' where customerTypeCode = 'LPN'
update CustomerType
set r2Type = 'Special Library' where customerTypeCode = 'LRG'
update CustomerType
set r2Type = 'Special Library' where customerTypeCode = 'LRN'
update CustomerType
set r2Type = 'Special Library' where customerTypeCode = 'LTG'
update CustomerType
set r2Type = 'Special Library' where customerTypeCode = 'LTN'
update CustomerType
set r2Type = 'Special Library' where customerTypeCode = 'LUG'
update CustomerType
set r2Type = 'Special Library' where customerTypeCode = 'LUN'
update CustomerType
set r2Type = 'Business to Business' where customerTypeCode = 'SP'
update CustomerType
set r2Type = 'Business to Business' where customerTypeCode = 'SV'




