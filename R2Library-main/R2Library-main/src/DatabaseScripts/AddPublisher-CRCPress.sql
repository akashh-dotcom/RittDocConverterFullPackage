
--select * from tPublisherUser
--where tiRecordStatus = 1
--
--select * from tPublisher


insert into tPublisher  (vchPublisherName, vchPublisherAddr1, vchPublisherAddr2, vchPublisherCity, vchPublisherState, vchPublisherZip
    , decPayPerView, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus, iConsolidatedPublisherId
    , vchMarcCountyCode, tiFeaturedPublisher, vchFeaturedImageName, vchFeaturedDisplayName, vchFeaturedDescription)
values ('CRC Press', '', '', '', '', null
      , null, 'sjscheider', getdate(), null, null, 1, null
      , null, 0, null, null, null)

--select * from tPublisher where vchPublisherName like 'CRC%'
--
--delete from tPublisher where iPublisherId = 131
--
--select * from tPublisher where iConsolidatedPublisherId = 92
--
--select * from tPublisher where vchPublisherName like 'Hodder%'
--
--update tPublisher set vchPublisherName = 'Hodder Arnold' where iPublisherId = 92
--