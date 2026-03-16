USE [DEV_RIT001]
GO



/****** Object:  StoredProcedure [dbo].[GetTermsToHighlight]    Script Date: 2/4/2014 1:11:55 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTermsToHighlight]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetTermsToHighlight]
GO



/****** Object:  UserDefinedTableType [dbo].[SearchTermType]    Script Date: 2/4/2014 1:22:04 PM ******/
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'SearchTermType' AND ss.name = N'dbo')
DROP TYPE [dbo].[SearchTermType]
GO

/****** Object:  UserDefinedTableType [dbo].[SearchTermType]    Script Date: 2/4/2014 1:22:04 PM ******/
IF NOT EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'SearchTermType' AND ss.name = N'dbo')
CREATE TYPE [dbo].[SearchTermType] AS TABLE(
	[searchTerm] [varchar](500) NULL
)
GO






/****** Object:  StoredProcedure [dbo].[GetTermsToHighlight]    Script Date: 2/4/2014 1:11:55 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTermsToHighlight]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[GetTermsToHighlight]
@termList [dbo].[SearchTermType] READONLY
WITH EXEC AS CALLER
AS
BEGIN
  SET NOCOUNT ON;

    declare @output table (word varchar(500) not null, id int not null, term varchar(1000) not null, termType varchar(15)) 

    declare term_cursor cursor for 
    	select searchTerm from @termList

    declare @searchWord nvarchar(500)
    declare @formOf nvarchar(500)

    open term_cursor   
    fetch next from term_cursor into @searchWord

    while @@FETCH_STATUS = 0  
    begin
        set @formOf = N''FORMSOF(INFLECTIONAL, '' + @searchWord + '')''
        
        insert into @output
            select @searchWord, min(iDiseaseNameId), vchDiseaseName, ''Disease'' termType
            from   tdiseasename
		    where  contains (vchDiseaseName, @formOf) and tiRecordStatus = 1
			group by vchDiseaseName
		
		insert into @output
            select @searchWord, min(iDiseaseNameId), vchDiseaseSynonym, ''DiseaseSynonym'' termType
            from   tdiseasesynonym
            where  contains (vchDiseaseSynonym, @formOf) and tiRecordStatus = 1
				--and vchDiseaseSynonym not in (
				--	select term from @output
				--)
			group by vchDiseaseSynonym

		insert into @output
            select @searchWord, min(iDrugListId), vchDrugName, ''Drug'' termType
            from   tDrugsList dl
            where  contains (vchDrugName, @formOf) and tiRecordStatus = 1 
				--and vchDrugName not in (
				--	select term from @output
				--)
			group by vchDrugName

		insert into @output
            select @searchWord, min(iDrugListId), vchDrugSynonymName, ''DrugSynonym'' termType
            from   tDrugSynonym
            where  contains (vchDrugSynonymName, @formOf) and tiRecordStatus = 1
				--and vchDrugSynonymName not in (
				--	select term from @output
				--)
			group by vchDrugSynonymName			

		insert into @output
            select @searchWord, min(iKeywordId), vchKeywordDesc, ''Keyword'' termType
            from   tKeyword
            where  contains (vchKeywordDesc, @formOf) and tiRecordStatus = 1
				--and vchKeywordDesc not in (
				--	select term from @output
				--)
			group by vchKeywordDesc					

        fetch next from term_cursor into @searchWord
    end

    close term_cursor  
    deallocate term_cursor

    select word, min(id) id, lower(ltrim(rtrim(term))) term, termType
	from @output
	group by word, term, termType
	having termType = min(termType)
END' 
END
GO


GRANT EXEC ON TYPE::dbo.SearchTermType TO R2UtilitiesUser
GRANT EXEC ON dbo.GetTermsToHighlight TO R2UtilitiesUser