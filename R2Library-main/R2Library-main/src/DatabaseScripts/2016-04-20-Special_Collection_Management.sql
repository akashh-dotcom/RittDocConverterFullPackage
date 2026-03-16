alter table tCollection
add tiIsSpecialCollection tinyint not null default((0));

go

alter table tCollection
add iSpecialCollectionSequence int not null default((0));

go

update tCollection
set tiIsSpecialCollection = 1,
iSpecialCollectionSequence = 6
where vchCollectionName = 'Clinical Cornerstone';

update tCollection
set tiIsSpecialCollection = 1,
iSpecialCollectionSequence = 7
where vchCollectionName = 'Noteworthy Nursing';

update tCollection
set tiIsSpecialCollection = 1,
iSpecialCollectionSequence = 1
where vchCollectionName = 'Best of 2015';

update tCollection
set tiIsSpecialCollection = 1,
iSpecialCollectionSequence = 3
where vchCollectionName = 'Nursing Essentials';

update tCollection
set tiIsSpecialCollection = 1,
iSpecialCollectionSequence = 4
where vchCollectionName = 'Hospital Essentials';

update tCollection
set tiIsSpecialCollection = 1,
iSpecialCollectionSequence = 5
where vchCollectionName = 'Medical Essentials';

update tCollection
set tiIsSpecialCollection = 1,
iSpecialCollectionSequence = 2
where vchCollectionName = '2015 AJN Books of the Year';

alter table tCollection
add vchDescription varchar(1000) null;

