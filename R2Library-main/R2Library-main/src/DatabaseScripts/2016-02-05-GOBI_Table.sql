

create table tCartReseller(
	iResellerId int not null primary key,
	vchResellerName varchar(100) not null,
	vchResellerDisplayName varchar (100) not null,
	decDiscount decimal(10,2) not null,
	vchAccountNumberOverride varchar(20) not null,
	tiRecordStatus tinyint not null
)


Insert into tCartReseller
values (1, 'GOBI', 'GOBI Order', 15, '007460', 1);

alter table tCart
add iResellerId int null constraint FK_tCart_tCartReseller REFERENCES tCartReseller(iResellerId);
