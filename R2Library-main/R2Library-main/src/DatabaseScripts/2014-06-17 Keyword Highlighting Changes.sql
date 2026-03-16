USE [DEV_RIT001]
GO

/****** Object:  StoredProcedure [dbo].[GetTermsToHighlight]    Script Date: 5/16/2014 3:43:56 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTermsToHighlight]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetTermsToHighlight]
GO


/****** Object:  UserDefinedTableType [dbo].[SearchTermType]    Script Date: 5/16/2014 3:44:09 PM ******/
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'SearchTermType' AND ss.name = N'dbo')
DROP TYPE [dbo].[SearchTermType]
GO

/****** Object:  UserDefinedTableType [dbo].[SearchTermType]    Script Date: 5/16/2014 3:44:09 PM ******/
IF NOT EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'SearchTermType' AND ss.name = N'dbo')
CREATE TYPE [dbo].[SearchTermType] AS TABLE(
	[searchTerm] [varchar](500) NULL,
	[isKeyword] [bit]
)
GO

GRANT EXECUTE ON TYPE::[dbo].[SearchTermType] TO [R2UtilitiesUser] AS [dbo]
GO



/****** Object:  StoredProcedure [dbo].[GetTermsToHighlight]    Script Date: 5/16/2014 3:53:54 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[GetTermsToHighlight]
@termList [dbo].[SearchTermType] READONLY
WITH EXEC AS CALLER
AS
BEGIN
  SET NOCOUNT ON;

    declare @output table (word varchar(500) not null, id int not null, term varchar(1000) not null, termType varchar(15)) 

    declare term_cursor cursor for 
    	select searchTerm, isKeyword from @termList

    declare @searchWord nvarchar(500)
	declare @isKeyword bit
    declare @formOf nvarchar(500)

    open term_cursor   
    fetch next from term_cursor into @searchWord, @isKeyword

    while @@FETCH_STATUS = 0  
    begin
        set @formOf = N'FORMSOF(INFLECTIONAL, ' + @searchWord + ')'
        
        insert into @output
            select @searchWord, min(iDiseaseNameId), vchDiseaseName, 'Disease' termType
            from   tdiseasename
		    where  contains (vchDiseaseName, @formOf) and tiRecordStatus = 1
			group by vchDiseaseName
		
		insert into @output
            select @searchWord, min(iDiseaseNameId), vchDiseaseSynonym, 'DiseaseSynonym' termType
            from   tdiseasesynonym
            where  contains (vchDiseaseSynonym, @formOf) and tiRecordStatus = 1
				--and vchDiseaseSynonym not in (
				--	select term from @output
				--)
			group by vchDiseaseSynonym

		insert into @output
            select @searchWord, min(iDrugListId), vchDrugName, 'Drug' termType
            from   tDrugsList dl
            where  contains (vchDrugName, @formOf) and tiRecordStatus = 1 
				--and vchDrugName not in (
				--	select term from @output
				--)
			group by vchDrugName

		insert into @output
            select @searchWord, min(iDrugListId), vchDrugSynonymName, 'DrugSynonym' termType
            from   tDrugSynonym
            where  contains (vchDrugSynonymName, @formOf) and tiRecordStatus = 1
				--and vchDrugSynonymName not in (
				--	select term from @output
				--)
			group by vchDrugSynonymName			

		if @isKeyword = 1
		begin
			insert into @output
				select @searchWord, min(iKeywordId), vchKeywordDesc, 'Keyword' termType
				from   tKeyword
				where  contains (vchKeywordDesc, @formOf) and tiRecordStatus = 1
					--and vchKeywordDesc not in (
					--	select term from @output
					--)
				group by vchKeywordDesc	
		end				

        fetch next from term_cursor into @searchWord, @isKeyword
    end

    close term_cursor  
    deallocate term_cursor

    select word, min(id) id, lower(ltrim(rtrim(term))) term, termType
	from @output
	group by word, term, termType
	having termType = min(termType)
END

GO

GRANT EXECUTE ON [dbo].[GetTermsToHighlight] TO [R2UtilitiesUser] AS [dbo]
GO






USE [TabersDictionary]
GO

/****** Object:  StoredProcedure [dbo].[GetTermsToHighlight]    Script Date: 6/17/2014 5:12:56 PM ******/
DROP PROCEDURE [dbo].[GetTermsToHighlight]
GO

/****** Object:  StoredProcedure [dbo].[GetTermsToHighlight]    Script Date: 6/17/2014 5:12:56 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



USE [TabersDictionary]
GO

/****** Object:  UserDefinedTableType [dbo].[SearchTermType]    Script Date: 6/17/2014 5:11:54 PM ******/
DROP TYPE [dbo].[SearchTermType]
GO

/****** Object:  UserDefinedTableType [dbo].[SearchTermType]    Script Date: 6/17/2014 5:11:54 PM ******/
CREATE TYPE [dbo].[SearchTermType] AS TABLE(
	[searchTerm] [varchar](500) NULL,
	[isKeyword] [bit]
)
GO

GRANT EXECUTE ON TYPE::[dbo].[SearchTermType] TO [R2UtilitiesUser] AS [dbo]
GO

GRANT EXECUTE ON TYPE::[dbo].[SearchTermType] TO [R2WebUser] AS [dbo]
GO




CREATE PROCEDURE [dbo].[GetTermsToHighlight]
@termList [dbo].[SearchTermType] READONLY
WITH EXEC AS CALLER
AS
BEGIN
  SET NOCOUNT ON;

    declare @output table (word varchar(500) not null, id int not null, term varchar(1000) not null) 

    declare term_cursor cursor for 
    	select searchTerm from @termList

    declare @searchWord nvarchar(500)
    declare @formOf nvarchar(500)

    open term_cursor   
    fetch next from term_cursor into @searchWord

    while @@FETCH_STATUS = 0  
    begin
        set @formOf = N'FORMSOF(INFLECTIONAL, ' + @searchWord + ')'
        
        insert into @output
            select @searchWord, TermContentKey, lower(Term)
            from   [TabersDictionary].[dbo].[TermContent]
            where  contains (Term, @formOf) 

        fetch next from term_cursor into @searchWord
    end

    close term_cursor  
    deallocate term_cursor

    select *, 'Tabers' termType from @output
END
GO

GRANT EXECUTE ON [dbo].[GetTermsToHighlight] TO [R2UtilitiesUser] AS [dbo]
GO

GRANT EXECUTE ON [dbo].[GetTermsToHighlight] TO [R2WebUser] AS [dbo]
GO


