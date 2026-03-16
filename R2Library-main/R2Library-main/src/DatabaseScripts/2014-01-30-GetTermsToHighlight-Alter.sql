USE [TabersDictionary]
GO

/****** Object:  StoredProcedure [dbo].[GetTermsToHighlight]    Script Date: 1/30/2014 4:04:49 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetTermsToHighlight]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'ALTER PROCEDURE [dbo].[GetTermsToHighlight]
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
        set @formOf = N''FORMSOF(INFLECTIONAL, '' + @searchWord + '')''
        
        insert into @output
            select @searchWord, TermContentKey, lower(Term)
            from   [TabersDictionary].[dbo].[TermContent]
            where  contains (Term, @formOf) 

        fetch next from term_cursor into @searchWord
    end

    close term_cursor  
    deallocate term_cursor

    select *, ''Tabers'' termType from @output
END' 
END
GO


