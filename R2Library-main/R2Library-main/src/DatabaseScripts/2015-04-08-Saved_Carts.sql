
CREATE TABLE tCartType(
	iCartTypeId [int] NOT NULL,
	vchCartTypeDescription [varchar](50) NOT NULL,
 CONSTRAINT [PK__CartType__iCartTypeId] PRIMARY KEY NONCLUSTERED 
(
	[iCartTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 85) ON [PRIMARY]
) ON [PRIMARY]

go

Insert into tCartType
VALUES (1, 'Active');

Insert into tCartType
VALUES (2, 'Saved');

go
alter table tCart
add iCartTypeId [int] NOT NULL default(1);


go
alter table tCart
add iCartTypeId [int] NOT NULL default(1);

alter table tCart
add vchCartName varchar(100) NULL ;


