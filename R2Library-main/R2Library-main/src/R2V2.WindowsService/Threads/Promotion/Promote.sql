----------------------------------------------------------------------
-- START
----------------------------------------------------------------------

begin try

	begin transaction

		declare @resourceId as int
		declare @publisherId as int
		declare @isbn as varchar(20)
		declare @UserId as varchar(20)
		declare @sourceResourceId as int
		declare @chapterId as varchar(25)
		declare @sectionId as varchar(25)
		declare @keyword as varchar(250)

		DECLARE @statusTable TABLE (isbn varchar(20), resourceId int, publisherId int, sourceResourceId int
			, tableName varchar(50), scriptAction varchar(2048), rowsAffected int)

		set @UserId = 'R2Promote'
		set @isbn = '1608316300'

		select @sourceResourceId = iResourceId from tResource where vchResourceISBN = @isbn;
		--select @sourceResourceId as 'sourceResourceId';

		----------------------------------------------------------------------
		-- vchPublisherName
		----------------------------------------------------------------------
		select @publisherId = dp.iPublisherId
		from   [RIT001_2012-08-22].dbo.tPublisher dp
		where  dp.vchPublisherName = (
			select vchPublisherName
			from   dbo.tPublisher sp
			 join  dbo.tResource sr on sr.iPublisherId = sp.iPublisherId and sr.vchResourceISBN = @isbn    
			)
		--select @publisherId as 'publisherId'

		if (@publisherId is null)
		begin
			insert into [RIT001_2012-08-22].dbo.tPublisher (vchPublisherName, vchPublisherAddr1, vchPublisherAddr2, vchPublisherCity, vchPublisherState
					, vchPublisherZip, decPayPerView, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus, iConsolidatedPublisherId
					, vchMarcCountyCode, tiFeaturedPublisher, vchFeaturedImageName, vchFeaturedDisplayName, vchFeaturedDescription)
				select vchPublisherName, vchPublisherAddr1, vchPublisherAddr2, vchPublisherCity, vchPublisherState
					, vchPublisherZip, sp.decPayPerView, @UserId, getdate(), null, null, 1, null
					, vchMarcCountyCode, 0, null, null, null
				from   dbo.tPublisher sp
				 join  dbo.tResource sr on sr.iPublisherId = sp.iPublisherId and sr.vchResourceISBN = @isbn;
        
				select @publisherId = SCOPE_IDENTITY();
				insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tPublisher', 'insert', @@ROWCOUNT);
		end
		--select @publisherId as 'publisherId'


		----------------------------------------------------------------------
		-- tResource
		----------------------------------------------------------------------
		--select @resourceId = iResourceId from [RIT001_2012-08-22].dbo.tResource where vchResourceISBN = @isbn
		select @resourceId = iResourceId from [RIT001_2012-08-22].dbo.tResource where vchResourceISBN = @isbn and tiRecordStatus = 1
		if (@resourceId is null)
		begin
			select @resourceId = iResourceId from [RIT001_2012-08-22].dbo.tResource where vchResourceISBN = @isbn
		end
		--select @resourceId as 'resourceId'

		if (@resourceId is null)
		begin
			insert into [RIT001_2012-08-22].dbo.tResource (vchResourceDesc, vchResourceTitle, vchResourceSubTitle, vchResourceAuthors
					, vchResourceAdditionalContributors, vchResourcePublisher, dtRISReleaseDate, dtResourcePublicationDate, tiBrandonHillStatus
					, vchResourceISBN, vchResourceEdition, decResourcePrice, dec3BundlePrice, decPayPerView, decSubScriptionPrice, vchResourceImageName
					, vchCopyRight, tiResourceReady, tiAllowSubscriptions, iPublisherId, iResourceStatusId, tiGloballyAccessible
					, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus, vchMARCRecord, vchResourceNLMCall
					, tiDrugMonograph, iDCTStatusId, tiDoodyReview, vchDoodyReviewURL, iPrevEditResourceID, vchAuthorXML, vchForthcomingDate
					, NotSaleable, vchResourceSortTitle, chrAlphaKey, vchIsbn10, vchIsbn13, vchEIsbn, vchResourceSortAuthor, dtQaApprovalDate, dtLastPromotionDate, tiContainsVideo
					, tiFreeResource, vchAffiliation)

				select vchResourceDesc, vchResourceTitle, vchResourceSubTitle, vchResourceAuthors
					, vchResourceAdditionalContributors, vchResourcePublisher, dtRISReleaseDate, dtResourcePublicationDate, tiBrandonHillStatus
					, vchResourceISBN, vchResourceEdition, decResourcePrice, dec3BundlePrice, decPayPerView, decSubScriptionPrice, vchResourceImageName
					, vchCopyRight, tiResourceReady, tiAllowSubscriptions, @publisherId, iResourceStatusId, tiGloballyAccessible
					, @UserId, getdate(), null, null, 1, vchMARCRecord, vchResourceNLMCall
					, tiDrugMonograph, iDCTStatusId, tiDoodyReview, vchDoodyReviewURL, iPrevEditResourceID, vchAuthorXML, vchForthcomingDate
					, NotSaleable, vchResourceSortTitle, chrAlphaKey, vchIsbn10, vchIsbn13, vchEIsbn, vchResourceSortAuthor,dtQaApprovalDate, getdate(), tiContainsVideo
					, tiFreeResource, vchAffiliation
				from   tResource
				where  vchResourceISBN = @isbn
			select @resourceId = SCOPE_IDENTITY();
			insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tResource', 'insert', @@ROWCOUNT);
		end
		else
		begin
			update dr
			set    dr.vchResourceTitle = sr.vchResourceTitle
				 , dr.vchResourceAuthors = sr.vchResourceAuthors
				 , dr.dtRISReleaseDate = CONVERT(date, GETDATE())
				 , dr.dtResourcePublicationDate = sr.dtResourcePublicationDate
				 , dr.tiBrandonHillStatus = sr.tiBrandonHillStatus
				 , dr.vchResourceEdition = sr.vchResourceEdition
				 , dr.decResourcePrice = sr.decResourcePrice
				 , dr.dec3BundlePrice = sr.dec3BundlePrice
				 , dr.decPayPerView = sr.decPayPerView
				 , dr.vchResourceImageName = sr.vchResourceImageName
				 , dr.tiResourceReady = sr.tiResourceReady
				 , dr.iPublisherId = @publisherId
				 , dr.iResourceStatusId = sr.iResourceStatusId
				 , dr.tiGloballyAccessible = sr.tiGloballyAccessible
				 , dr.vchUpdaterId = @userId
				 , dr.dtLastUpdate = getdate()
				 , dr.tiRecordStatus = 1
				 , dr.iPrevEditResourceID = sr.iPrevEditResourceID
				 , dr.vchAuthorXML = sr.vchAuthorXML
				 , dr.vchResourceSortTitle = sr.vchResourceSortTitle
				 , dr.chrAlphaKey = sr.chrAlphaKey
		--         , dr.vchIsbn10 = sr.
		--         , dr.vchIsbn13 = sr.
		--         , dr.vchEIsbn = sr.
				 , dr.vchResourceSortAuthor = sr.vchResourceSortAuthor
				 , dr.dtQaApprovalDate = sr.dtQaApprovalDate
				 , dr.dtLastPromotionDate = getdate()
				 , dr.tiContainsVideo = sr.tiContainsVideo
				 , dr.tiFreeResource = sr.tiFreeResource
				 , dr.vchAffiliation = isnull(sr.vchAffiliation, dr.vchAffiliation)
			from   [RIT001_2012-08-22].dbo.tResource dr
			 join  tResource sr on sr.vchResourceISBN = dr.vchResourceISBN
			where  dr.iResourceId = @resourceId;
			insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tResource', 'update', @@ROWCOUNT);
		end

		----------------------------------------------------------------------
		-- tResourceDiscipline
		----------------------------------------------------------------------
		update [RIT001_2012-08-22].dbo.tResourceDiscipline
		set    vchUpdaterId = @UserId
			 , dtLastUpdate = getdate()
			 , tiRecordStatus = 0
		where iResourceId = @resourceId and tiRecordStatus = 1;
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tResourceDiscipline', 'update', @@ROWCOUNT);

		insert into [RIT001_2012-08-22].dbo.tResourceDiscipline (iLibraryDisciplineId, iResourceId, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) 
			select iLibraryDisciplineId, @resourceId, @UserId, getdate(), null, null, tiRecordStatus
			from   tResourceDiscipline srd
			where  srd.iResourceId = @sourceResourceId
			  and  tiRecordStatus = 1;
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tResourceDiscipline', 'insert', @@ROWCOUNT);


		----------------------------------------------------------------------
		-- tResourcePracticeArea
		----------------------------------------------------------------------
		--select * from tResourcePracticeArea where iResourceId = 4528
		update [RIT001_2012-08-22].dbo.tResourcePracticeArea
		set    vchUpdaterId = @UserId
			 , dtLastUpdate = getdate()
			 , tiRecordStatus = 0
		where iResourceId = @resourceId and tiRecordStatus = 1;
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tResourcePracticeArea', 'update', @@ROWCOUNT);

		insert into [RIT001_2012-08-22].dbo.tResourcePracticeArea (iResourceId, iPracticeAreaId, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus)
			select @resourceId, dpa.iPracticeAreaId, @UserId, getdate(), null, null, 1
			from   tResourcePracticeArea srpa
			 join  dbo.tPracticeArea spa on spa.iPracticeAreaId = srpa.iPracticeAreaId
			 join  [RIT001_2012-08-22].dbo.tPracticeArea dpa on dpa.vchPracticeAreaCode = spa.vchPracticeAreaCode
			where  srpa.iResourceId = @sourceResourceId 
			   and srpa.tiRecordStatus = 1;
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tResourcePracticeArea', 'insert', @@ROWCOUNT);


		----------------------------------------------------------------------
		-- tResourceSpecialty
		----------------------------------------------------------------------
		--select * from tResourceSpecialty where iResourceId = 4528
		update [RIT001_2012-08-22].dbo.tResourceSpecialty
		set    vchUpdaterId = @UserId
			 , dtLastUpdate = getdate()
			 , tiRecordStatus = 0
		where iResourceId = @resourceId and tiRecordStatus = 1;
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tResourceSpecialty', 'update', @@ROWCOUNT);

		insert into [RIT001_2012-08-22].dbo.tResourceSpecialty (iResourceId, iSpecialtyId, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus)
			select @resourceId, ds.iSpecialtyId, @UserId, getdate(), null, null, 1
			from   tResourceSpecialty srs
			 join  dbo.tSpecialty ss on ss.iSpecialtyId = srs.iSpecialtyId
			 join  [RIT001_2012-08-22].dbo.tSpecialty ds on ds.vchSpecialtyCode = ss.vchSpecialtyCode
			where  srs.iResourceId = @sourceResourceId 
			   and srs.tiRecordStatus = 1;
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tResourceSpecialty', 'insert', @@ROWCOUNT);

		----------------------------------------------------------------------
		-- tAuthor
		----------------------------------------------------------------------
		--select * from tResourceSpecialty where iResourceId = 4528
		delete [RIT001_2012-08-22].dbo.tAuthor
		where iResourceId = @resourceId;
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tAuthor', 'delete', @@ROWCOUNT);

		insert into [RIT001_2012-08-22].dbo.tAuthor (iResourceId, vchFirstName, vchLastName, vchMiddleName, vchLineage, vchDegree, tiAuthorOrder)
			select @resourceId, vchFirstName, vchLastName, vchMiddleName, vchLineage, vchDegree, tiAuthorOrder
			from   tAuthor sa
			where  sa.iResourceId = @sourceResourceId;
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tAuthor', 'insert', @@ROWCOUNT);


		----------------------------------------------------------------------
		-- tDiseaseResource
		----------------------------------------------------------------------
		--select * from tDiseaseResource where vchResourceISBN = @isbn
		delete [RIT001_2012-08-22].dbo.tDiseaseResource where vchResourceISBN = @isbn
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tDiseaseResource', 'delete', @@ROWCOUNT);
		insert into [RIT001_2012-08-22].dbo.tDiseaseResource (iDiseaseNameId, vchResourceISBN, vchChapterId, vchSectionId, vchCreatorId, dtCreationDate
				, vchUpdaterId, dtLastUpdate, tiRecordStatus) 
			select sdr.iDiseaseNameId, sdr.vchResourceISBN, sdr.vchChapterId, sdr.vchSectionId, @UserId, getdate(), null, null, sdr.tiRecordStatus
			from   tDiseaseResource sdr     
			where  sdr.vchResourceISBN = @isbn and sdr.tiRecordStatus = 1;
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tDiseaseResource', 'insert', @@ROWCOUNT);


		----------------------------------------------------------------------
		-- tDiseaseSynonymResource
		----------------------------------------------------------------------
		--select * from tDiseaseSynonymResource where vchResourceISBN = @isbn
		delete [RIT001_2012-08-22].dbo.tDiseaseSynonymResource where vchResourceISBN = @isbn
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tDiseaseSynonymResource', 'delete', @@ROWCOUNT);
		insert into [RIT001_2012-08-22].dbo.tDiseaseSynonymResource (iDiseaseSynonymId, vchResourceISBN, vchChapterId, vchSectionId
				, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus)
			select sdsr.iDiseaseSynonymId, sdsr.vchResourceISBN, sdsr.vchChapterId, sdsr.vchSectionId, @UserId, getdate(), null, null, sdsr.tiRecordStatus
			from   tDiseaseSynonymResource sdsr     
			where  sdsr.vchResourceISBN = @isbn and sdsr.tiRecordStatus = 1;
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tDiseaseSynonymResource', 'insert', @@ROWCOUNT);


		------------------------------------------------------------------------
		------------------------------------------------------------------------
		--declare @sourceResourceId as int
		--declare @resourceId as int
		--declare @publisherId as int
		--declare @isbn as varchar(20)
		--declare @UserId as varchar(20)
		--
		--set @UserId = 'R2Promote'
		--set @isbn = '1585281980'
		--set @resourceId = 3990
		--
		--select @sourceResourceId = iResourceId from tResource where vchResourceISBN = @isbn;
		--select @sourceResourceId as 'sourceResourceId';
		------------------------------------------------------------------------
		------------------------------------------------------------------------

		----------------------------------------------------------------------
		-- tKeywordResource
		----------------------------------------------------------------------
		declare keywordResource_cursor cursor for 
			select kr.vchChapterId, kr.vchSectionId, k.vchKeywordDesc
			from   dbo.tKeywordResource kr
			 join  dbo.tKeyword k on k.iKeywordId = kr.iKeywordId
			where  kr.vchResourceISBN = @isbn

		open keywordResource_cursor   
		fetch next from keywordResource_cursor into @chapterId, @sectionId, @keyword

		while @@FETCH_STATUS = 0  
		begin
			print @chapterId + ' -- ' + @sectionId

			exec [RIT001_2012-08-22].dbo.sp_insertKeywordResource @keyword, @isbn, @chapterId, @sectionId, @UserId
	
			fetch next from keywordResource_cursor into @chapterId, @sectionId, @keyword
		end

		close keywordResource_cursor  
		deallocate keywordResource_cursor

		declare @tableCount as int
		select @tableCount = count(*) from [RIT001_2012-08-22].dbo.tKeywordResource where vchResourceISBN = @isbn;
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tKeywordResource', 'update', @tableCount);


		----------------------------------------------------------------------
		-- tDrugResource
		----------------------------------------------------------------------
		delete [RIT001_2012-08-22].dbo.tDrugResource where vchResourceISBN = @isbn
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tDrugResource', 'delete', @@ROWCOUNT);
		insert into [RIT001_2012-08-22].dbo.tDrugResource (iDrugListId, vchResourceISBN, vchChapterId, vchSectionId
				, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus, tiTopicIndex, vchTitle) 
			select sdr.iDrugListId, sdr.vchResourceISBN, sdr.vchChapterId, sdr.vchSectionId, @UserId, getdate(), null, null, sdr.tiRecordStatus, sdr.tiTopicIndex, sdr.vchTitle
			from   tDrugResource sdr     
			where  sdr.vchResourceISBN = @isbn and sdr.tiRecordStatus = 1;
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tDrugResource', 'insert', @@ROWCOUNT);


		----------------------------------------------------------------------
		-- tDrugSynonymResource
		----------------------------------------------------------------------
		delete [RIT001_2012-08-22].dbo.tDrugSynonymResource where vchResourceISBN = @isbn
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tDrugSynonymResource', 'delete', @@ROWCOUNT);
		insert into [RIT001_2012-08-22].dbo.tDrugSynonymResource (iDrugSynonymId, vchResourceISBN, vchChapterId, vchSectionId
				, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus, tiTopicIndex, vchTitle)
			select sdsr.iDrugSynonymId, sdsr.vchResourceISBN, sdsr.vchChapterId, sdsr.vchSectionId, @UserId, getdate(), null, null, sdsr.tiRecordStatus
				, sdsr.tiTopicIndex, sdsr.vchTitle
			from   tDrugSynonymResource sdsr     
			where  sdsr.vchResourceISBN = @isbn and sdsr.tiRecordStatus = 1;
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tDrugSynonymResource', 'insert', @@ROWCOUNT);


		----------------------------------------------------------------------
		-- tAtoZIndex - 6/19/2013
		----------------------------------------------------------------------
		delete [RIT001_2012-08-22].dbo.tAtoZIndex where vchResourceISBN = @isbn
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tAtoZIndex', 'delete', @@ROWCOUNT);
		-- tAtoZIndex -- tdiseasename/tDiseaseResource
		insert into [RIT001_2012-08-22].dbo.tAtoZIndex (iParentTableId, vchName, chrAlphaKey, iResourceId, vchResourceISBN, vchChapterId, vchSectionId, iAtoZIndexTypeId)
			select dn.iDiseaseNameId as Id, dn.vchDiseaseName as Name, upper(substring(ltrim(rtrim(dn.vchDiseaseName)), 0, 2)) as AlphaKey
				 , r.iResourceId, dr.vchResourceISBN, dr.vchChapterId, dr.vchSectionId, 1
			from   [RIT001_2012-08-22].dbo.tdiseasename dn
			 join  [RIT001_2012-08-22].dbo.tDiseaseResource dr on dr.iDiseaseNameId = dn.iDiseaseNameId and dr.vchResourceISBN = @isbn
			 join  [RIT001_2012-08-22].dbo.tResource r on r.vchResourceISBN = dr.vchResourceISBN
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tAtoZIndex', 'insert type 1, disease names', @@ROWCOUNT);
		-- tAtoZIndex -- tdiseasesynonym/tDiseaseSynonymResource
		insert into [RIT001_2012-08-22].dbo.tAtoZIndex (iParentTableId, vchName, chrAlphaKey, iResourceId, vchResourceISBN, vchChapterId, vchSectionId, iAtoZIndexTypeId)
			select ds.iDiseaseSynonymId as Id, ds.vchDiseaseSynonym as Name, upper(substring(ltrim(rtrim(ds.vchDiseaseSynonym)), 0, 2)) as AlphaKey
				 , r.iResourceId, dsr.vchResourceISBN, dsr.vchChapterId, dsr.vchSectionId, 2
			from   [RIT001_2012-08-22].dbo.tdiseasesynonym ds
			 join  [RIT001_2012-08-22].dbo.tDiseaseSynonymResource dsr on dsr.iDiseaseSynonymId = ds.iDiseaseSynonymId and dsr.vchResourceISBN = @isbn
			 join  [RIT001_2012-08-22].dbo.tResource r on r.vchResourceISBN = dsr.vchResourceISBN
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tAtoZIndex', 'insert type 2, disease synonyms', @@ROWCOUNT);
		-- tAtoZIndex -- tDrugsList/tDrugResource
		insert into [RIT001_2012-08-22].dbo.tAtoZIndex (iParentTableId, vchName, chrAlphaKey, iResourceId, vchResourceISBN, vchChapterId, vchSectionId, iAtoZIndexTypeId)
			select dl.iDrugListId as Id, dl.vchDrugName as Name, upper(substring(ltrim(rtrim(dl.vchDrugName)), 0, 2)) as AlphaKey
				 , r.iResourceId, dr.vchResourceISBN, dr.vchChapterId, dr.vchSectionId, 3
			from   [RIT001_2012-08-22].dbo.tDrugsList dl
			 join  [RIT001_2012-08-22].dbo.tDrugResource dr on dr.iDrugListId = dl.iDrugListId and dr.vchResourceISBN = @isbn
			 join  [RIT001_2012-08-22].dbo.tResource r on r.vchResourceISBN = dr.vchResourceISBN
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tAtoZIndex', 'insert type 3, drug names', @@ROWCOUNT);
		-- tAtoZIndex -- tDrugSynonym/tDrugSynonymResource
		insert into [RIT001_2012-08-22].dbo.tAtoZIndex (iParentTableId, vchName, chrAlphaKey, iResourceId, vchResourceISBN, vchChapterId, vchSectionId, iAtoZIndexTypeId)
			select ds.iDrugSynonymId as Id, ds.vchDrugSynonymName as Name, upper(substring(ltrim(rtrim(ds.vchDrugSynonymName)), 0, 2)) as AlphaKey
				 , r.iResourceId, dsr.vchResourceISBN, dsr.vchChapterId, dsr.vchSectionId, 4
			from   [RIT001_2012-08-22].dbo.tDrugSynonym ds
			 join  [RIT001_2012-08-22].dbo.tDrugSynonymResource dsr on dsr.iDrugSynonymId = ds.iDrugSynonymId and dsr.vchResourceISBN = @isbn
			 join  [RIT001_2012-08-22].dbo.tResource r on r.vchResourceISBN = dsr.vchResourceISBN
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tAtoZIndex', 'insert type 4, drug synonyms', @@ROWCOUNT);
		-- tAtoZIndex -- tKeyword/tKeywordResource
		insert into [RIT001_2012-08-22].dbo.tAtoZIndex (iParentTableId, vchName, chrAlphaKey, iResourceId, vchResourceISBN, vchChapterId, vchSectionId, iAtoZIndexTypeId)
			select k.iKeywordId as Id, k.vchKeywordDesc as Name, upper(substring(ltrim(rtrim(k.vchKeywordDesc)), 0, 2)) as AlphaKey
				 , r.iResourceId, kr.vchResourceISBN, kr.vchChapterId, kr.vchSectionId, 5
			from   [RIT001_2012-08-22].dbo.tKeyword k
			 join  [RIT001_2012-08-22].dbo.tKeywordResource kr on kr.iKeywordId = k.iKeywordId and kr.vchResourceISBN = @isbn
			 join  [RIT001_2012-08-22].dbo.tResource r on r.vchResourceISBN = kr.vchResourceISBN
		insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'tAtoZIndex', 'insert type 5, keywords', @@ROWCOUNT);

		select * from @statusTable;
		----------------------------------------------------------------------
		-- END
		----------------------------------------------------------------------
	commit 
end try
begin catch
    if @@TRANCOUNT > 0 
        rollback 
    insert into @statusTable values (@isbn, @resourceId, @publisherId, @sourceResourceId, 'EXCEPTION', ERROR_MESSAGE(), ERROR_NUMBER());
    select * from @statusTable;
end catch
