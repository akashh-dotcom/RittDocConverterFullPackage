


ALTER TABLE tPublisher
ADD 
tiFeaturedPublisher  tinyInt not null default ((0)),
vchFeaturedImageName  varchar(100) null,
vchFeaturedDisplayName   varchar(100) null,
vchFeaturedDescription   varchar(2000) null
;

 update tPublisher
 Set tiFeaturedPublisher = 1, vchFeaturedImageName = 'Springer.jpg', vchFeaturedDisplayName = 'Springer Science+Business Media', vchFeaturedDescription = 'Springer Science+Business Media is a global publishing company publishing books, e-books and peer-reviewed journals in science, technical and medical (STM) publishing. Springer also hosts a number of scientific databases, including SpringerLink, SpringerProtocols, and SpringerImages. Book publications include major reference works, textbooks, monographs and book series; more than 37,000 titles are available as e-books in 13 subject collections. Springer has more than 60 publishing houses, more than 5,000 employees and around 2,000 journals and publishes 6,000 new books each year. Springer has major offices in Berlin, Heidelberg, Dordrecht, and New York City.'
 where iPublisherId = 25