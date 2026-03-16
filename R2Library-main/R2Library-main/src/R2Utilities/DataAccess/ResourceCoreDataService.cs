#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.Core.SqlCommandParameters;
using R2Library.Data.ADO.R2.DataServices;
using R2Utilities.Tasks.ContentTasks.BookInfo;
using R2V2.Core.Audit;
using R2V2.Core.Resource;

#endregion

namespace R2Utilities.DataAccess
{
    public class ResourceCoreDataService : DataServiceBase
    {
        private const string UpdateNewEdtitions = @"
            update r
            set    r.iNewEditResourceId = new.iResourceId, r.vchUpdaterId = 'UpdateNewEdtitions', r.dtLastUpdate = getdate()
            from  tResource r
             join [{0}].[PreludeData].[dbo].[Product] pr on r.vchResourceISBN = pr.sku
             join tResource new on pr.newAvailEd = new.vchResourceISBN
            where (r.iNewEditResourceId is null or r.iNewEditResourceId <> new.iResourceId) and
                  (r.tiRecordStatus = 1 and new.tiRecordStatus = 1 and r.iResourceStatusId <> 72 and new.iResourceStatusId <> 72) ";

        private const string UpdatePreviousEditions = @"
            update r
            set    r.iPrevEditResourceID = prev.iResourceId, r.vchUpdaterId = 'UpdatePreviousEditions', r.dtLastUpdate = getdate()
            from   tResource r
             join [{0}].[PreludeData].[dbo].[Product] pr on r.vchResourceISBN = pr.sku
             join tResource prev on pr.previousEd = prev.vchResourceISBN
            where (r.iPrevEditResourceID is null or r.iPrevEditResourceID <> prev.iResourceId) and
                  (r.tiRecordStatus = 1 and prev.tiRecordStatus = 1 and r.iResourceStatusId <> 72 and prev.iResourceStatusId <> 72) ";

        private const string ClearInvalidLatestEditions = @"
            update r
            set    r.iLatestEditResourceId = null, r.vchUpdaterId = 'LastestEditionCleanUp', r.dtLastUpdate = getdate()
            from   tresource r
            where  (iNewEditResourceId = 0 or iNewEditResourceId is null) and r.iLatestEditResourceId is not null ";

        private const string ClearInvalidPreviousEditions = @"
            update r
            set    r.iPrevEditResourceID = null, r.vchUpdaterId = 'LastestEditionCleanUp', r.dtLastUpdate = getdate()
            from   tresource r
            where  iPrevEditResourceID = 0 ";

        private const string GetNewEditions = @"
            select iResourceId, vchResourceISBN, vchResourceTitle, vchPublisherName, decResourcePrice, dtRISReleaseDate, sum(LicenseCount) as LicenseCount, PreviousIsbn
            from (
                select new.iResourceId, new.vchResourceISBN, new.vchResourceTitle, p.vchPublisherName, new.decResourcePrice,
                       new.dtRISReleaseDate, ISNULL(sum(irl.iLicenseCount), '0') as LicenseCount, r.vchResourceISBN as PreviousIsbn
                from   tResource r
                 join  [{0}].[PreludeData].[dbo].[Product] pr on r.vchResourceISBN = pr.sku
                 join  tResource new on pr.newAvailEd = new.vchResourceISBN
                 join  tPublisher p on r.iPublisherId = p.iPublisherId
                 join  tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId
                where  r.iNewEditResourceId <> new.iResourceId
                  and (r.tiRecordStatus = 1 and new.tiRecordStatus = 1 and r.iResourceStatusId <> 72 and new.iResourceStatusId <> 72)
                group by new.iResourceId, new.vchResourceISBN, new.vchResourceTitle, p.vchPublisherName, new.decResourcePrice, new.dtRISReleaseDate, r.vchResourceISBN
                union
                select new.iResourceId, new.vchResourceISBN, new.vchResourceTitle, p.vchPublisherName, new.decResourcePrice,
                       new.dtRISReleaseDate, 0 as LicenseCount, r.vchResourceISBN as PreviousIsbn
                from   tResource r
                 join  [{0}].[PreludeData].[dbo].[Product] pr on r.vchResourceISBN = pr.sku
                 join  tResource new on pr.newAvailEd = new.vchResourceISBN
                 join  tPublisher p on r.iPublisherId = p.iPublisherId
                 join  tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId
                where  r.iNewEditResourceId <> new.iResourceId
                  and  (r.tiRecordStatus = 1 and new.tiRecordStatus = 1 and r.iResourceStatusId <> 72 and new.iResourceStatusId <> 72)
                group by new.iResourceId, new.vchResourceISBN, new.vchResourceTitle, p.vchPublisherName, new.decResourcePrice, new.dtRISReleaseDate, r.vchResourceISBN
            ) as test
            group by iResourceId, vchResourceISBN, vchResourceTitle, vchPublisherName, decResourcePrice, dtRISReleaseDate, PreviousIsbn ";


        private const string ResourceSelect =
            @"select r.iResourceId, r.vchResourceTitle, r.vchResourceSubTitle, r.vchResourceAuthors, r.dtRISReleaseDate, r.dtResourcePublicationDate, r.tiBrandonHillStatus
                   , r.vchResourceISBN, r.vchResourceEdition, r.vchCopyRight, r.iPublisherId, r.iResourceStatusId, r.tiRecordStatus, r.tiDrugMonograph, r.vchResourceSortTitle
                   , r.chrAlphaKey, r.vchIsbn10, r.vchIsbn13, r.vchEIsbn, p.vchPublisherName
              from   dbo.tResource r
               join  dbo.tPublisher p on p.iPublisherId = r.iPublisherId
             ";

        private static readonly string GetAuthorInsert = new StringBuilder()
            .Append(
                "insert into {0} (iResourceId, vchFirstName, vchLastName, vchMiddleName, vchLineage, vchDegree, tiAuthorOrder) ")
            .Append("values(@ResourceId, @FirstName, @LastName, @MiddleName, @Lineage, @Degree, @AuthorOrder); ")
            .ToString();

        private static readonly string UpdateAutoArchive = new StringBuilder()
            .Append("update tResource ")
            .Append("set    iResourceStatusId = 7 ")
            .Append("     , vchUpdaterId = 'AutoArchive' ")
            .Append("     , dtLastUpdate = '{1}' ")
            .Append("     , dtArchiveDate = '{1}' ")
            .Append("where iResourceId in ( ")
            .Append("   select r.iResourceId ")
            .Append("   from   tResource r ")
            .Append(
                "   left join  [{0}].RittenhouseWeb.dbo.Product p on p.isbn10 = r.vchIsbn10 and p.productStatusId = 3 ")
            .Append(
                "   left join  [{0}].RittenhouseWeb.dbo.Product p2 on p2.isbn13 = r.vchIsbn13 and p2.productStatusId = 3 ")
            .Append(
                "   where  r.iResourceStatusId = 6 and (p.sku is not null or p2.sku is not null) and r.tiExcludeFromAutoArchive = 0 ")
            .Append("    ) ")
            .ToString();

        private static readonly string GetAutoArchives = new StringBuilder()
            .Append(
                "select iResourceId, vchResourceISBN, vchResourceTitle, vchPublisherName, decResourcePrice, dtRISReleaseDate, sum(LicenseCount) as LicenseCount ")
            .Append(
                "from (Select r.iResourceId, r.vchResourceISBN, r.vchResourceTitle, p.vchPublisherName, r.decResourcePrice, ")
            .Append("       r.dtRISReleaseDate, ISNULL(sum(irl.iLicenseCount), '0') as LicenseCount ")
            .Append("   from tResource r ")
            .Append("   join tPublisher p on r.iPublisherId = p.iPublisherId ")
            .Append("   join tInstitutionResourceLicense irl on r.iResourceId = irl.iResourceId ")
            .Append("   join tInstitution i on irl.iInstitutionId = i.iInstitutionId and i.tiHouseAcct = 0 ")
            .Append("   where r.iResourceId in ( ")
            .Append("       select r.iResourceId ")
            .Append("          from   tResource r ")
            .Append(
                "          left join  [{0}].RittenhouseWeb.dbo.Product p on p.isbn10 = r.vchIsbn10 and p.productStatusId = 3 ")
            .Append(
                "          left join  [{0}].RittenhouseWeb.dbo.Product p2 on p2.isbn13 = r.vchIsbn13 and p2.productStatusId = 3 ")
            .Append(
                "          where  r.iResourceStatusId = 6 and (p.sku is not null or p2.sku is not null) and r.tiExcludeFromAutoArchive = 0) ")
            .Append(
                "group by r.iResourceId, r.vchResourceISBN, r.vchResourceTitle, p.vchPublisherName, r.decResourcePrice, r.dtRISReleaseDate ")
            .Append("union ")
            .Append(
                "Select r.iResourceId, r.vchResourceISBN, r.vchResourceTitle, p.vchPublisherName, r.decResourcePrice, ")
            .Append("       r.dtRISReleaseDate, '0' as LicenseCount ")
            .Append("   from tResource r ")
            .Append("   join tPublisher p on r.iPublisherId = p.iPublisherId ")
            .Append("   where r.iResourceId in ( ")
            .Append("       select r.iResourceId ")
            .Append("          from   tResource r ")
            .Append(
                "          left join  [{0}].RittenhouseWeb.dbo.Product p on p.isbn10 = r.vchIsbn10 and p.productStatusId = 3 ")
            .Append(
                "          left join  [{0}].RittenhouseWeb.dbo.Product p2 on p2.isbn13 = r.vchIsbn13 and p2.productStatusId = 3 ")
            .Append(
                "          where  r.iResourceStatusId = 6 and (p.sku is not null or p2.sku is not null) and r.tiExcludeFromAutoArchive = 0) ")
            .Append(
                "group by r.iResourceId, r.vchResourceISBN, r.vchResourceTitle, p.vchPublisherName, r.decResourcePrice, r.dtRISReleaseDate ")
            .Append(") as test ")
            .Append(
                "group by iResourceId, vchResourceISBN, vchResourceTitle, vchPublisherName, decResourcePrice, dtRISReleaseDate ")
            .Append("order by LicenseCount ")
            .ToString();

        private static readonly string UpdateResourceAffiliationSql = new StringBuilder()
            .Append(" UPDATE tResource ")
            .Append(
                " SET    vchAffiliation = p.affiliation, tiAffiliationUpdatedByPrelude = 1, vchUpdaterId = 'AffiliationUpdate', dtLastUpdate = getdate()  ")
            .Append(" FROM   tResource r ")
            .Append(" INNER JOIN [{0}].RittenhouseWeb.dbo.Product p on r.vchIsbn10 = p.isbn10 ")
            .Append(" WHERE  r.vchAffiliation <> p.affiliation")
            .Append(" or (r.vchAffiliation is null and p.affiliation is not null)")
            .ToString();

        public ResourceCore GetResourceByIsbn(string isbn, bool excludeForthcoming)
        {
            var sql = new StringBuilder(ResourceSelect)
                .Append("where  r.vchResourceISBN = @Isbn ")
                .AppendFormat("  and   r.tiRecordStatus = 1 and r.iResourceStatusId in (6,7{0}) ",
                    excludeForthcoming ? "" : ",8")
                .Append("order by r.iResourceId desc ");

            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("Isbn", isbn)
            };

            var resource = GetFirstEntity<ResourceCore>(sql.ToString(), parameters, true);

            var resourcePracticeAreaDataService = new ResourcePracticeAreaDataService();
            resource.PracticeAreas = resourcePracticeAreaDataService.GetResourcePracticeArea(resource.Id);

            var resourceSpecialtyDataService = new ResourceSpecialtyDataService();
            resource.Specialties = resourceSpecialtyDataService.GetResourceSpecialty(resource.Id);

            var resourcePublisherDataService = new ResourcePublisherDataService();
            var associatedPublishers =
                resourcePublisherDataService.GetResourcePubslihers(resource.Id);

            if (associatedPublishers != null)
            {
                var parentPublisher = associatedPublishers.FirstOrDefault(x => x.ParentPublisherId == null);
                resource.PublisherName = parentPublisher != null ? parentPublisher.PublisherName : null;

                var associatedPublishersFound = associatedPublishers.Where(x => x.ParentPublisherId != null);
                var resourcePublishers = associatedPublishersFound as ResourcePublisher[] ??
                                         associatedPublishersFound.ToArray();
                resource.AssociatedPublishers = resourcePublishers.Any() ? resourcePublishers.ToList() : null;
            }

            return resource;
        }

        public IList<ResourceCore> GetResourcesAll(bool orderByDescending)
        {
            var sql = new StringBuilder(ResourceSelect)
                .AppendFormat("order by r.iResourceId {0};", orderByDescending ? "desc" : "");
            //.Append("select r.iResourceId, r.vchResourceTitle, r.vchResourceSubTitle, r.vchResourceAuthors ")
            //.Append("     , r.dtRISReleaseDate, r.dtResourcePublicationDate, r.tiBrandonHillStatus, r.vchResourceISBN, r.vchResourceEdition ")
            //.Append("     , r.vchCopyRight, r.iPublisherId, r.iResourceStatusId, r.tiRecordStatus, r.tiDrugMonograph ")
            //.Append("     , r.vchResourceSortTitle, r.chrAlphaKey, r.vchIsbn10, r.vchIsbn13, r.vchEIsbn ")
            //.Append("     , p.vchPublisherName ")
            //.Append("from   dbo.tResource r ")
            //.Append(" join  dbo.tPublisher p on p.iPublisherId = r.iPublisherId ")
            //.AppendFormat("order by r.iResourceId {0};", (orderByDescending) ? "desc" : "");

            var parameters = new List<ISqlCommandParameter>();

            var resources = GetEntityList<ResourceCore>(sql.ToString(), parameters, true);

            return resources;
        }

        public IList<ResourceCore> GetResources(int minResourceId, int maxResourceId, int maxResourceCount,
            bool orderByDescending)
        {
            var sql = new StringBuilder(ResourceSelect.Replace("select r.iResourceId",
                    $"select top {maxResourceCount} r.iResourceId"))
                .Append("where r.iResourceId >= @MinResourceId and r.iResourceId <= @MaxResourceId ")
                .AppendFormat("order by r.iResourceId {0};", orderByDescending ? "desc" : "");

            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("MinResourceId", minResourceId),
                new Int32Parameter("MaxResourceId", maxResourceId)
            };

            var resources = GetEntityList<ResourceCore>(sql.ToString(), parameters, true);

            return resources;
        }

        public IList<ResourceCore> GetResourcesByIsbns(string[] isbns, bool orderByDescending)
        {
            var sql = new StringBuilder(ResourceSelect);
            //.Append("where r.iResourceId >= @MinResourceId and r.iResourceId <= @MaxResourceId ")
            //.AppendFormat("order by r.iResourceId {0};", (orderByDescending) ? "desc" : "");

            var parameters = new List<ISqlCommandParameter>();

            for (var i = 0; i < isbns.Length; i++)
            {
                var paramName = $"ISBN_{i}";
                sql.AppendFormat("{0} r.vchResourceISBN = @{1} ", i == 0 ? "where" : "or", paramName);
                parameters.Add(new StringParameter(paramName, isbns[i]));
            }

            var resources = GetEntityList<ResourceCore>(sql.ToString(), parameters, true);

            return resources;
        }

        public List<ResourceCore> GetActiveAndArchivedResources(bool orderByDescending, int minResourceId,
            int maxResourceId, int maxRecords, string[] isbns)
        {
            var sql = new StringBuilder()
                .Append(ResourceSelect.Replace("select r.iResourceId,", $"select top {maxRecords} r.iResourceId,"))
                .Append("where  r.iResourceStatusId in (6,7) and r.tiRecordStatus = 1 ")
                .Append("  and  r.iResourceId between @MinResourceId and @MaxResourceId ");

            if (isbns != null && isbns.Length > 0)
            {
                sql.Append("  and  r.vchResourceISBN in (");
                for (var i = 0; i < isbns.Length; i++)
                {
                    sql.AppendFormat("{0}'{1}'", i == 0 ? string.Empty : ",", isbns[i]);
                }

                sql.Append(") ");
            }

            sql.AppendFormat("order by r.iResourceId {0};", orderByDescending ? "desc" : "");

            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("MinResourceId", minResourceId),
                new Int32Parameter("MaxResourceId", maxResourceId)
            };

            var resources = GetEntityList<ResourceCore>(sql.ToString(), parameters, true);

            return resources;
        }

        /// <param name="tableName"> </param>
        public int InsertAuthor(int resourceId, int order, Author author, string tableName)
        {
            //@ResourceId, @FirstName, @LastName, @MiddleName, @Lineage, @Degree, @AuthorOrder
            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("ResourceId", resourceId),
                new Int32Parameter("AuthorOrder", order),
                new StringParameter("FirstName", author.FirstName),
                new StringParameter("LastName", author.LastName),
                new StringParameter("MiddleName", author.MiddleInitial),
                new StringParameter("Lineage", author.Lineage),
                new StringParameter("Degree", author.Degrees)
            };

            var insert = string.Format(GetAuthorInsert, tableName);

            var id = ExecuteInsertStatementReturnIdentity(insert, parameters.ToArray(), false);
            return id;
        }

        public int DeleteResourceAuthors(int resourceId, string tableName)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("ResourceId", resourceId)
            };

            var delete = $"delete from {tableName} where iResourceId = @ResourceId ";

            var rowCount = ExecuteUpdateStatement(delete, parameters.ToArray(), false);
            Log.DebugFormat("delete row count: {0}", rowCount);
            return rowCount;
        }

        /// <param name="sortTitle"> </param>
        /// <param name="alphaChar"> </param>
        /// <param name="isbn10"> </param>
        /// <param name="isbn13"> </param>
        /// <param name="eisbn"> </param>
        /// <param name="updateId"> </param>
        public int UpdateNewResourceFields(int resourceId, string sortTitle, string alphaChar, string isbn10, string isbn13, string eisbn, string updateId)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("SortTitle", sortTitle),
                new StringParameter("AlphaChar", alphaChar),
                new StringParameter("Isbn10", isbn10),
                new StringParameter("Isbn13", isbn13),
                new StringParameter("EIsbn", eisbn),
                new StringParameter("UpdateId", updateId),
                new Int32Parameter("ResourceId", resourceId)
            };
            var update = new StringBuilder()
                .Append("update tResource ")
                .Append("set    vchResourceSortTitle = @SortTitle ")
                .Append("     , chrAlphaKey = @AlphaChar ")
                .Append("     , vchIsbn10 = COALESCE(@Isbn10, (select i.vchIsbn from tResourceIsbn i where i.iResourceId = @ResourceId and i.iResourceIsbnTypeId = 1)) ")
                .Append("     , vchIsbn13 = COALESCE(@Isbn13, (select i.vchIsbn from tResourceIsbn i where i.iResourceId = @ResourceId and i.iResourceIsbnTypeId = 2)) ")
                .Append("     , vchEIsbn = COALESCE(@EIsbn, (select i.vchIsbn from tResourceIsbn i where i.iResourceId = @ResourceId and i.iResourceIsbnTypeId = 3)) ")
                .Append("     , vchUpdaterId = @UpdateId ")
                .Append("     , dtLastUpdate = getdate() ")
                .Append(
                    "     , vchResourceEdition = rtrim(ltrim(vchResourceEdition)) ") // SJS - 1/23/2014 and again on 12/4/2015 - trim edition text - https://www.squishlist.com/technotects/r2cl/60/
                .Append("where iResourceId = @ResourceId ");

            // SJS - 1/21/2014 - If you are not logging the SQL to the log4net logs, there better be a big log explanation for it and approval from the Pope or Dalai Lama.
            // I'm now spending my time correcting all of these calls where the logging was set to false.
            var rowCount = ExecuteUpdateStatement(update.ToString(), parameters.ToArray(), true);
            Log.DebugFormat("update row count: {0}", rowCount);
            return rowCount;
        }

        /// <param name="updateId"> </param>
        public int UpdateResourceSortAuthor(int resourceId, string sortAuthor, string updateId)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("SortAuthor", sortAuthor),
                new StringParameter("UpdateId", updateId),
                new Int32Parameter("ResourceId", resourceId)
            };
            var update = new StringBuilder()
                .Append("update tResource ")
                .Append("set    vchResourceSortAuthor = @SortAuthor ")
                .Append("     , vchUpdaterId = @UpdateId ")
                .Append("     , dtLastUpdate = getdate() ")
                .Append("where iResourceId = @ResourceId ");

            var rowCount = ExecuteUpdateStatement(update.ToString(), parameters.ToArray(), false);
            Log.DebugFormat("update row count: {0}", rowCount);
            return rowCount;
        }

        public int SetResourceStatus(int resourceId, ResourceStatus resourceStatus, string updateId)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("StatusId", (int)resourceStatus),
                new StringParameter("UpdateId", updateId),
                new Int32Parameter("ResourceId", resourceId)
            };
            var update = new StringBuilder()
                .Append("update tResource ")
                .Append("set    iResourceStatusId = @StatusId ")
                .Append("     , vchUpdaterId = @UpdateId ")
                .Append("     , dtLastUpdate = getdate() ")
                .Append("where iResourceId = @ResourceId ");

            var rowCount = ExecuteUpdateStatement(update.ToString(), parameters.ToArray(), false);
            Log.DebugFormat("update row count: {0}", rowCount);
            return rowCount;
        }

        public int SetResourceTabersStatus(int resourceId, bool resourceTabersStatus, string updateId)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("TabersStatus", resourceTabersStatus ? 1 : 0),
                new StringParameter("UpdateId", updateId),
                new Int32Parameter("ResourceId", resourceId)
            };
            var update = new StringBuilder()
                .Append("update tResource ")
                .Append("set    tiTabersStatus = @TabersStatus ")
                .Append("     , vchUpdaterId = @UpdateId ")
                .Append("     , dtLastUpdate = getdate() ")
                .Append("where iResourceId = @ResourceId ");

            var rowCount = ExecuteUpdateStatement(update.ToString(), parameters.ToArray(), false);
            Log.DebugFormat("update row count: {0}", rowCount);
            return rowCount;
        }

        public int AutoArchiveResources(string linkedServerName)
        {
            var date = DateTime.Now;

            var parameters = new List<ISqlCommandParameter>();
            var update =
                string.Format(
                    string.IsNullOrWhiteSpace(linkedServerName)
                        ? UpdateAutoArchive.Replace("[{0}].", "")
                        : UpdateAutoArchive, linkedServerName, date);
            Log.Debug(update);
            var rowCount = ExecuteUpdateStatement(update, parameters.ToArray(), false);
            Log.DebugFormat("update row count: {0}", rowCount);

            var sql = new StringBuilder()
                .Append(" INSERT INTO tResourceAudit ")
                .AppendFormat(" select iResourceId, {0}, 'AutoArchiveResources', getdate() ",
                    (int)ResourceAuditType.Unspecificed)
                .Append(" , ' [iResourceStatusId changed from Active(6) to Archived(7)]' ")
                .AppendFormat(" from tResource where  dtLastUpdate = '{0}' and vchUpdaterId = 'AutoArchive'", date)
                .ToString();

            var inserted = ExecuteInsertStatementReturnRowCount(sql, parameters.ToArray(), false);

            return rowCount;
        }

        public List<ArchiveResource> GetAutoArchiveResources(string linkedServerName)
        {
            var parameters = new List<ISqlCommandParameter>();
            var sql = string.IsNullOrWhiteSpace(linkedServerName)
                ? GetAutoArchives.Replace("[{0}].", "")
                : string.Format(GetAutoArchives, linkedServerName);
            Log.Debug(sql);
            var archiveResources = GetEntityList<ArchiveResource>(sql, parameters, true);
            Log.DebugFormat("update row count: {0}", archiveResources.Count);
            return archiveResources; //_getAutoPreviousEdition
        }

        public int UpdateResourceEditions(string linkedServerName)
        {
            var totalRowCount = 0;
            var parameters = new List<ISqlCommandParameter>();

            var update = string.IsNullOrWhiteSpace(linkedServerName)
                ? UpdateNewEdtitions.Replace("[{0}].", "")
                : string.Format(UpdateNewEdtitions, linkedServerName);
            Log.Debug(update);
            var rowCount = ExecuteUpdateStatement(update, parameters.ToArray(), false);

            Log.DebugFormat("update New Editions row count: {0}", rowCount);
            totalRowCount += rowCount;
            update = string.IsNullOrWhiteSpace(linkedServerName)
                ? UpdatePreviousEditions.Replace("[{0}].", "")
                : string.Format(UpdatePreviousEditions, linkedServerName);
            Log.Debug(update);
            rowCount = ExecuteUpdateStatement(update, parameters.ToArray(), false);

            Log.DebugFormat("update Previous Editions row count: {0}", rowCount);
            totalRowCount += rowCount;

            ExecuteUpdateStatement(ClearInvalidLatestEditions, parameters.ToArray(), false);
            ExecuteUpdateStatement(ClearInvalidPreviousEditions, parameters.ToArray(), false);

            var sql = new StringBuilder()
                .Append(" INSERT INTO tResourceAudit ")
                .AppendFormat(
                    " select r.iResourceId, {0}, 'UpdateNewEdtitions', getdate(), ' [iNewEditResourceId changed from ' ",
                    (int)ResourceAuditType.Unspecificed)
                .Append(
                    " + cast(isnull(r.iNewEditResourceId, 0) as varchar(10)) + ' to '+ cast(isnull(new.iResourceId, 0) as varchar(10)) + ']' ")
                .Append(" from  tResource r ")
                .AppendFormat(" join {0}[PreludeData].[dbo].[Product] pr on r.vchResourceISBN = pr.sku ",
                    string.IsNullOrWhiteSpace(linkedServerName) ? "" : $"[{linkedServerName}].")
                .Append(" join tResource new on pr.newAvailEd = new.vchResourceISBN ")
                .Append(" where (r.iNewEditResourceId is null or r.iNewEditResourceId <> new.iResourceId) and ")
                .Append(" (r.tiRecordStatus = 1 and new.tiRecordStatus = 1 and r.iResourceStatusId <> 72 ")
                .Append(" and new.iResourceStatusId <> 72) ")
                .ToString();

            var inserted = ExecuteInsertStatementReturnRowCount(sql, parameters.ToArray(), false);

            return totalRowCount;
        }

        public List<NewEditionResource> GetNewEditionResources(string linkedServerName)
        {
            var parameters = new List<ISqlCommandParameter>();
            var sql = string.IsNullOrWhiteSpace(linkedServerName)
                ? GetNewEditions.Replace("[{0}].", "")
                : string.Format(GetNewEditions, linkedServerName);
            Log.Debug(sql);
            var newEditionResource = GetEntityList<NewEditionResource>(sql, parameters, true);
            Log.DebugFormat("update row count: {0}", newEditionResource.Count);
            return newEditionResource;
        }

        public int UpdateResourceAffiliation(string linkedServerName)
        {
            var parameters = new List<ISqlCommandParameter>();

            var sql = new StringBuilder()
                .Append("INSERT INTO tResourceAudit ")
                .AppendFormat(
                    "select r.iResourceId, {0}, 'UpdateResourceAffiliation', getdate(), ' [vchAffiliation changed from ''' ",
                    (int)ResourceAuditType.Unspecificed)
                .Append(" + vchAffiliation + ''' to '''+  p.affiliation + ''']' ")
                .Append("from  tResource r ")
                .AppendFormat(" INNER JOIN {0}RittenhouseWeb.dbo.Product p on r.vchIsbn10 = p.isbn10 "
                    , string.IsNullOrWhiteSpace(linkedServerName) ? "" : $"[{linkedServerName}].")
                .Append("WHERE  vchAffiliation <> p.affiliation")
                .ToString();

            var inserted = ExecuteInsertStatementReturnRowCount(sql, parameters.ToArray(), false);
            //.Append("FROM   tResource r ")
            //.Append(" INNER JOIN [{0}].RittenhouseWeb.dbo.Product p on r.vchIsbn10 = p.isbn10 ")
            //.Append("WHERE  vchAffiliation <> p.affiliation")


            sql = string.IsNullOrWhiteSpace(linkedServerName)
                ? UpdateResourceAffiliationSql.Replace("[{0}].", "")
                : string.Format(UpdateResourceAffiliationSql, linkedServerName);
            Log.Debug(sql);
            var rowCount = ExecuteUpdateStatement(sql, parameters.ToArray(), false);
            Log.DebugFormat("update row count: {0}", rowCount);

            //TODO: Need tp add to Resource Audit

            return rowCount > 0 ? rowCount : 0;
        }

        public List<EmailResource> ProcessResourceLatestEditions()
        {
            var parameters = new List<ISqlCommandParameter>();

            var sql =
                "select iResourceId, iPrevEditResourceId, vchResourceIsbn from tResource where iNewEditResourceId is null and (tiRecordStatus = 1 and iResourceStatusId <> 72)";
            var latestEditions = GetEntityList<ResourceEdition>(sql, null, false);


            foreach (var resourceEdition in latestEditions)
            {
                sql =
                    "select iResourceId, vchResourceIsbn, iPrevEditResourceId, iLatestEditResourceId from tResource where iNewEditResourceId = @NewEdititonResourceId and (tiRecordStatus = 1 and iResourceStatusId <> 72)";
                parameters = new List<ISqlCommandParameter>
                {
                    new Int32Parameter("NewEdititonResourceId", resourceEdition.ResourceId)
                };

                var prevEditions = GetEntityList<ChildResourceEdition>(sql, parameters, false);
                if (prevEditions.Any())
                {
                    SetChildEditions(resourceEdition, prevEditions);
                }
            }

            var latestEditionsToUpdate = latestEditions
                .Where(x => x.ResourcesToSetLatestEdition != null && x.ResourcesToSetLatestEdition.Any()).ToList();

            var latestEditionResouresUpdated = new List<int>();
            var currentDateTime = DateTime.Now;
            var lastEdtitionSetCount = 0;
            foreach (var resourceEdition in latestEditionsToUpdate)
            {
                var resourceIdSql = new StringBuilder();
                foreach (var childEdition in resourceEdition.ResourcesToSetLatestEdition)
                {
                    resourceIdSql.AppendFormat("{0},", childEdition.ResourceId);
                    latestEditionResouresUpdated.Add(childEdition.ResourceId);
                }

                if (resourceIdSql.Length <= 0)
                {
                    continue;
                }

                sql = new StringBuilder()
                    .Append(" INSERT INTO tResourceAudit ")
                    .AppendFormat(
                        " select iResourceId, {0}, 'Set iLatestEditResourceId', getdate(), ' [iLatestEditResourceId changed from ' + cast(isnull(iLatestEditResourceId, 0) as varchar(10)) + ' to 1]' ",
                        (int)ResourceAuditType.Unspecificed)
                    .AppendFormat(" from tResource where iResourceId in ({0})",
                        resourceIdSql.ToString(0, resourceIdSql.Length - 1))
                    .ToString();

                var inserted = ExecuteInsertStatementReturnRowCount(sql, parameters.ToArray(), false);

                sql = string.Format(
                    "update tResource set iLatestEditResourceId = {0}, vchUpdaterId = 'Set iLatestEditResourceId', dtLastUpdate = '{2}' where iResourceId in ({1})",
                    resourceEdition.ResourceId, resourceIdSql.ToString(0, resourceIdSql.Length - 1), currentDateTime);

                lastEdtitionSetCount += ExecuteUpdateStatement(sql, parameters.ToArray(), false);
            }

            sql = @"
                select r.vchResourceISBN, r.vchResourceTitle, r.vchResourceEdition, p.vchPublisherName, r.decResourcePrice
                , sum(irl.iLicenseCount) as LicenseCount, r.dtRISReleaseDate, r.iResourceId
                , newr.vchResourceISBN as NewIsbn, newr.vchResourceTitle as NewTitle, newr.vchResourceEdition as NewEdition
                from tResource r
                join tresource newR on r.iLatestEditResourceId = newR.iResourceId
                join tPublisher p on r.iPublisherId = p.iPublisherId
                left join tInstitutionResourceLicense irl on irl.iResourceId = r.iResourceId
                where r.vchUpdaterId = 'Set iLatestEditResourceId' and r.dtLastUpdate = '{0}'
                group by r.vchResourceISBN, r.vchResourceTitle, r.vchResourceEdition, p.vchPublisherName, r.decResourcePrice
                , r.dtRISReleaseDate, r.iResourceId, newr.vchResourceISBN, newr.vchResourceTitle , newr.vchResourceEdition
                ";

            List<EmailResource> emailResources = null;
            if (lastEdtitionSetCount > 0)
            {
                var sqlQuery = string.Format(sql, currentDateTime);

                emailResources = GetEntityList<EmailResource>(sqlQuery, parameters, false);
                if (emailResources.Any())
                {
                    return emailResources.OrderBy(x => x.NewIsbn).ToList();
                }
            }

            return emailResources;
        }

        private void SetChildEditions(ResourceEdition resourceEdition, List<ChildResourceEdition> childResourceEditions)
        {
            resourceEdition.ResourcesToSetLatestEdition = new List<ChildResourceEdition>();
            if (childResourceEditions.Any(y => y.LatestEditResourceId != resourceEdition.ResourceId))
            {
                resourceEdition.ResourcesToSetLatestEdition.AddRange(
                    childResourceEditions.Where(y => y.LatestEditResourceId != resourceEdition.ResourceId));

                foreach (var childResourceEdition in childResourceEditions)
                {
                    Log.DebugFormat(
                        "ChildResourceEdition ResourceId: {0} CurrentLatestEditResourceId: {1} NewLatestEditResourceId: {2}",
                        childResourceEdition.ResourceId, childResourceEdition.LatestEditResourceId,
                        resourceEdition.ResourceId);
                }
            }

            var sql = new StringBuilder()
                .Append(
                    " select iResourceId, vchResourceIsbn, iPrevEditResourceId, iLatestEditResourceId from tResource ")
                .Append(
                    " where iNewEditResourceId = @NewEdititonResourceId and (tiRecordStatus = 1 and iResourceStatusId <> 72) ")
                .Append(" and (iLatestEditResourceId is null or iLatestEditResourceId <> @LatestEditResourceId) ")
                .ToString();
            //const string sql = "select iResourceId, vchResourceIsbn, iPrevEditResourceId, iLatestEditResourceId from tResource where iNewEditResourceId = @NewEdititonResourceId and (tiRecordStatus = 1 and iResourceStatusId <> 72)";
            while (childResourceEditions.Any())
            {
                var tempChildEditions = new List<ChildResourceEdition>();
                foreach (var childResourceEdition in childResourceEditions)
                {
                    var parameters = new List<ISqlCommandParameter>
                    {
                        new Int32Parameter("NewEdititonResourceId", childResourceEdition.ResourceId),
                        new Int32Parameter("LatestEditResourceId", resourceEdition.ResourceId)
                    };
                    var prevEditions = GetEntityList<ChildResourceEdition>(sql, parameters, false);
                    if (prevEditions.Any())
                    {
                        tempChildEditions.AddRange(prevEditions);
                        foreach (var prevEdition in prevEditions)
                        {
                            Log.DebugFormat(
                                "ChildResourceEdition ResourceId: {0} CurrentLatestEditResourceId: {1} NewLatestEditResourceId: {2}",
                                prevEdition.ResourceId, prevEdition.LatestEditResourceId, resourceEdition.ResourceId);
                        }
                    }
                }

                if (tempChildEditions.Any())
                {
                    resourceEdition.ResourcesToSetLatestEdition.AddRange(tempChildEditions);
                }

                break;
            }
        }

        public void UpdateInstitutionConsortia(string linkedServerName)
        {
            var linkedServer = string.IsNullOrWhiteSpace(linkedServerName)
                ? ""
                : $"[{linkedServerName}].";

            var sql = new StringBuilder()
                .Append("update tInstitution ")
                .Append("set vchConsortia = c.consort ")
                .Append("from tInstitution i ")
                .AppendFormat("join {0}[PreludeData].[dbo].[Customer] c on i.vchInstitutionAcctNum = c.accountNumber ",
                    linkedServer)
                .Append("where c.consort is not null ")
                .ToString();

            Log.Debug(sql);

            var rowCount = ExecuteUpdateStatement(sql, new List<ISqlCommandParameter>(), false);

            Log.DebugFormat("UpdateInstitutionConsortia -- update row count: {0}", rowCount);
        }

        public void UpdateInstitutionTerritory(string linkedServerName)
        {
            const string updateString = @"
update tInstitution
set iTerritoryId = ct.iTerritoryId
, vchUpdaterId = 'UpdateInstitutionTerritory'
, dtLastUpdate = GETDATE()
from tInstitution i
join tTerritory t on t.iTerritoryId = i.iTerritoryId
join {0}[PreludeData].[dbo].Customer c on c.accountNumber = i.vchInstitutionAcctNum
join tTerritory ct on ct.vchTerritoryCode = c.territory
where t.vchTerritoryCode <> ct.vchTerritoryCode
";
            var updateSql = string.Format(updateString,
                string.IsNullOrWhiteSpace(linkedServerName) ? "" : $"[{linkedServerName}].");

            Log.Debug(updateSql);
            var updateRowCount = ExecuteUpdateStatement(updateSql, new List<ISqlCommandParameter>(), false);

            Log.DebugFormat("UpdateInstitutionTerritory -- update row count: {0}", updateRowCount);
        }


        public int UpdateEisbns(List<OnixEisbn> onixEisbns)
        {
            var takeCount = 25;
            var i = 0;
            var totalUpdated = 0;

            while (true)
            {
                var items = onixEisbns.Skip(i * takeCount).Take(takeCount).ToArray();
                if (items.Length == 0)
                {
                    break;
                }

                var sql = new StringBuilder();

                foreach (var onixEisbn in items)
                {
                    var eIsbn = onixEisbn.EIsbn13 ?? onixEisbn.EIsbn10;
                    if (string.IsNullOrWhiteSpace(eIsbn))
                    {
                        continue;
                    }

                    sql.Append(
                        " Insert into tResourceAudit([iResourceId],[tiResourceAuditTypeId],[vchCreatorId],[dtCreationDate],[vchEventDescription]) ");
                    sql.Append(" select iResourceId, 1, 'UpdateWithOnixDataTask', GETDATE(), ");
                    sql.AppendFormat(
                        " case when vchEIsbn is null then 'Adding eIsbn from ONIX' else 'Change vchEIsbn from ' + vchEIsbn + ' to {0}' end ",
                        eIsbn);
                    sql.AppendFormat(
                        " from tResource where vchIsbn13 = '{1}' and (vchEIsbn is null or vchEIsbn <> '{0}'); ", eIsbn,
                        onixEisbn.Isbn13);
                    sql.AppendFormat(
                        " Update tResource set vchEIsbn = '{0}', vchUpdaterId = 'UpdateWithOnixDataTask', dtLastUpdate = getdate() where vchisbn13 = '{1}' and (vchEIsbn is null or vchEIsbn <> '{0}'); ",
                        eIsbn, onixEisbn.Isbn13);
                }

                totalUpdated += ExecuteUpdateStatement(sql.ToString(), new List<ISqlCommandParameter>(), false);

                i++;
            }

            return totalUpdated;
        }

        public List<ResourceTitleChange> GetRittenhouseTitles(string linkedServerName,
            Dictionary<string, string> isbnAndTitles)
        {
            var resourceIsbns = new StringBuilder();
            foreach (var isbnAndTitle in isbnAndTitles)
            {
                resourceIsbns.AppendFormat("{1}'{0}'", isbnAndTitle.Key, resourceIsbns.Length == 0 ? "" : ",");
            }

            var sql = @"
select p.title, p.subtitle, r.vchResourceTitle, r.vchResourceSubTitle, r.iResourceId, r.vchResourceIsbn, r.vchIsbn13
from tResource r
left join [PreludeData]..Product p on r.vchResourceIsbn = p.sku
where r.vchResourceIsbn in ({0})
order by r.iResourceId desc
";
            sql = string.Format(sql, resourceIsbns);
            var rittenhouseResourceTitles =
                GetEntityList<ResourceTitleChange>(sql, new List<ISqlCommandParameter>(), false);

            foreach (var rittenhouseResourceTitle in rittenhouseResourceTitles)
            {
                rittenhouseResourceTitle.AlternateTitle = isbnAndTitles[rittenhouseResourceTitle.Isbn];
            }

            return rittenhouseResourceTitles;
        }


        public List<ResourceTitleChange> GetRittenhouseTitles(string linkedServerName, int minResourceId,
            int maxResourceId)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("MinResourceId", minResourceId),
                new Int32Parameter("MaxResourceId", maxResourceId)
            };
            var sql = @"
select ltrim(rtrim(p.title)) as title, ltrim(rtrim(p.subtitle)) as subtitle, ltrim(rtrim(r.vchResourceTitle)) as vchResourceTitle
, ltrim(rtrim(r.vchResourceSubTitle)) as vchResourceSubTitle, r.iResourceId, r.vchResourceIsbn, r.vchIsbn13
from tResource r
left join [RittenhouseWeb]..Product p on r.vchResourceISBN = p.isbn10
where r.iResourceStatusId in (6,7)
and r.iResourceId >= @MinResourceId
and r.iResourceId <= @MaxResourceId
and r.tiRecordStatus = 1
order by r.iResourceId desc
";
            //sql = string.Format(sql, string.IsNullOrWhiteSpace(linkedServerName) ? "" : string.Format("[{0}].", linkedServerName));
            var rittenhouseResourceTitles = GetEntityList<ResourceTitleChange>(sql, parameters, false);
            return rittenhouseResourceTitles;
        }

        public bool UpdateResourceTitle(ResourceTitleChange resourceTitleChange, string r2UtilitiesDatabaseName)
        {
            try
            {
                var parameters = new List<ISqlCommandParameter>
                {
                    new Int32Parameter("ResourceId", resourceTitleChange.ResourceId)
                };

                var transformQueueInsert = new StringBuilder()
                    .Append(
                        $"insert into {r2UtilitiesDatabaseName}..TransformQueue (resourceId, isbn, status, dateAdded) ")
                    .Append($" select r.iResourceId, '{resourceTitleChange.Isbn}', 'A', GETDATE() ")
                    .Append(" from tResource r ")
                    .Append(
                        $" left join {r2UtilitiesDatabaseName}..TransformQueue tq on r.iResourceId = tq.resourceId and tq.status = 'A' ")
                    .Append($" where r.iResourceId = {resourceTitleChange.ResourceId} and tq.transformQueueId is null ")
                    .ToString();


                var auditInsert = new StringBuilder()
                    .Append(
                        " Insert into tResourceAudit([iResourceId],[tiResourceAuditTypeId],[vchCreatorId],[dtCreationDate],[vchEventDescription]) ")
                    .Append(" select iResourceId, 1, 'UpdateTitleTask', GETDATE(), ")
                    .AppendFormat(" '{1} title from ' + ISNULL(vchResourceTitle, '') + ' to {0} | ",
                        resourceTitleChange.GetNewTitle().Replace("'", "''"),
                        resourceTitleChange.IsRevert ? "Reverted" : "UpUpdateddates")
                    .AppendFormat("RittenhouseTitle: {0}",
                        resourceTitleChange.RittenhouseTitle?.Replace("'", "''") ?? "");

                if (resourceTitleChange.IsRevert)
                {
                    if (resourceTitleChange.SubTitle != resourceTitleChange.GetNewSubTitle())
                    {
                        auditInsert.AppendFormat(
                            " | Updated Subtitle from ' + ISNULL(vchResourceSubTitle, '') + ' to {0}",
                            resourceTitleChange.GetNewSubTitle()?.Replace("'", "''"));
                    }
                }
                else
                {
                    if (resourceTitleChange.UpdateType == ResourceTitleUpdateType.RittenhouseEqualR2TitleAndSub)
                    {
                        auditInsert.Append(" | Removed Subtitle");
                    }
                }

                auditInsert.Append("' from tResource where iResourceId = @ResourceId ; ");

                var resourceUpdate = new StringBuilder()
                    .AppendFormat(
                        " Update tResource set vchResourceTitle = '{0}', vchUpdaterId = 'UpdateTitleTask', dtLastUpdate = getdate() ",
                        resourceTitleChange.GetNewTitle().Replace("'", "''").Replace("&amp;", "&"));

                if (resourceTitleChange.IsRevert)
                {
                    if (resourceTitleChange.SubTitle != resourceTitleChange.GetNewSubTitle())
                    {
                        resourceUpdate.AppendFormat(", vchResourceSubTitle = '{0}' ",
                            resourceTitleChange.GetNewSubTitle()?.Replace("'", "''").Replace("&amp;", "&"));
                    }
                }
                else
                {
                    if (resourceTitleChange.UpdateType == ResourceTitleUpdateType.RittenhouseEqualR2TitleAndSub)
                    {
                        resourceUpdate.Append(", vchResourceSubTitle = null ");
                    }
                }

                resourceUpdate.Append(" where iResourceId = @ResourceId; ");

                var sql = new StringBuilder()
                    .Append(transformQueueInsert)
                    .Append(auditInsert)
                    .Append(resourceUpdate)
                    .ToString();

                return ExecuteUpdateStatement(sql, parameters, false) > 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }

            return false;
        }

        public int InsertDataIntoConfigSettings(string[] configurationSettingInserts, bool deleteFirst)
        {
            if (deleteFirst)
            {
                ExecuteUpdateStatement("truncate table tConfigurationSetting;", new List<ISqlCommandParameter>(),
                    false);
            }

            var sql = new StringBuilder();

            var insertCounter = 0;
            var counter = 0;
            foreach (var item in configurationSettingInserts)
            {
                counter++;

                sql.Append(item);
                if (counter == 50)
                {
                    insertCounter += ExecuteInsertStatementReturnRowCount(sql.ToString(), null, true);
                    sql = new StringBuilder();
                    counter = 0;
                }
            }

            if (counter != 0)
            {
                insertCounter += ExecuteInsertStatementReturnRowCount(sql.ToString(), null, true);
            }

            return insertCounter;
        }

        public List<ResourcePriceUpdateItem> GetResourcePriceUpdates()
        {
            var sql = @"
 select rpu.*
 from tResourcePriceUpdate rpu
 join tResource r on r.tiRecordStatus = 1 and r.vchIsbn10 = rpu.vchResourceISBN or r.vchIsbn13 = rpu.vchResourceISBN
 where CONVERT(date, dtUpdateDate) <= CONVERT(date, getdate()) and rpu.tiRecordStatus = 1 and rpu.dtLastUpdate is null
";

            var priceUpdates = GetEntityList<ResourcePriceUpdateItem>(sql, new List<ISqlCommandParameter>(), false);
            return priceUpdates;
        }

        public int UpdateResourcePrice(ResourcePriceUpdateItem resourcePriceUpdateItem)
        {
            var sql = $@"
 Insert into tResourceAudit([iResourceId],[tiResourceAuditTypeId],[vchCreatorId],[dtCreationDate],[vchEventDescription])
 Select r.iResourceId, 1, 'PriceUpdateTask', GETDATE(), 'Updated ListPrice from ' + convert(varchar(100), r.decResourcePrice) + ' to ' + convert(varchar(100), rpu.decResourcePrice)
 from tResource r join tResourcePriceUpdate rpu on r.vchIsbn10 = rpu.vchResourceISBN or r.vchIsbn13 = rpu.vchResourceISBN
 where r.decResourcePrice <> rpu.decResourcePrice and rpu.iResourcePriceUpdateId = {resourcePriceUpdateItem.Id}

 update tResource
 set decResourcePrice = rpu.decResourcePrice,
 dtLastUpdate = GETDATE(),
 vchUpdaterId = 'PriceUpdateTask'
 from tResource r
 join tResourcePriceUpdate rpu on r.vchIsbn10 = rpu.vchResourceISBN or r.vchIsbn13 = rpu.vchResourceISBN
 where  r.decResourcePrice <> rpu.decResourcePrice and  rpu.iResourcePriceUpdateId = {resourcePriceUpdateItem.Id};

 update tResourcePriceUpdate
 set dtLastUpdate = GETDATE(),
 vchUpdaterId = 'PriceUpdateTask'
 where iResourcePriceUpdateId  = {resourcePriceUpdateItem.Id};
";
            return ExecuteUpdateStatement(sql, new List<ISqlCommandParameter>(), false);
        }


        public int InsertYbpResources()
        {
            //            var sql = $@"
            //Insert into rittenhouse..R2Library_Resources ([sku], [EAN_13], [eISBN], [list_price], [status])
            //select r.vchResourceISBN, r.vchIsbn13, r.vchEIsbn, round(CAST(r.decResourcePrice * 100 AS float), 0), r.iResourceStatusId
            //from tResource r
            //where r.tiRecordStatus = 1 and r.iResourceStatusId <> 72
            //and r.vchResourceISBN not in (
            //	select rr.sku from rittenhouse..R2Library_Resources rr where rr.sku is not null
            //)
            //";
            var sql = @"
Insert into rittenhouse..R2Library_Resources ([sku], [EAN_13], [eISBN], [list_price], [status])
select r.vchResourceISBN, r.vchIsbn13, r.vchEIsbn, round(CAST(r.decResourcePrice * 100 AS float), 0), r.iResourceStatusId
from tResource r
where r.tiRecordStatus = 1 and r.iResourceStatusId <> 72
";
            return ExecuteUpdateStatement(sql, new List<ISqlCommandParameter>(), false);
        }

        public int TruncateYbpResources()
        {
            var sql = "truncate table rittenhouse..R2Library_Resources";
//            var sql = $@"
//update rittenhouse..R2Library_Resources
//set EAN_13 = r.vchIsbn13
//, eISBN = r.vchEIsbn
//, list_price = round(CAST(r.decResourcePrice * 100 AS float), 0)
//, status = r.iResourceStatusId
//--select r.vchResourceISBN, r.vchIsbn13, r.vchEIsbn, round(CAST(r.decResourcePrice * 100 AS float), 0), r.iResourceStatusId
//from tResource r
//join rittenhouse..R2Library_Resources rr on r.vchResourceISBN = rr.sku
//where r.tiRecordStatus = 1 and r.iResourceStatusId <> 72
//and (
//	rr.EAN_13 <> r.vchIsbn13 or rr.eISBN <> r.vchEIsbn or round(rr.list_price, 0) <> round(CAST(r.decResourcePrice * 100 AS float), 0) or rr.status <> r.iResourceStatusId
//)

            //";
            return ExecuteUpdateStatement(sql, new List<ISqlCommandParameter>(), false);
        }

//        public int DeleteYbpResources()
//        {
//            var sql = $@"
//delete from rittenhouse..R2Library_Resources
//--select * from rittenhouse..R2Library_Resources
//where sku not in (
//	select r.vchResourceISBN from tResource r
//	where r.tiRecordStatus = 1 and r.iResourceStatusId <> 72
//)
//";
//            return ExecuteUpdateStatement(sql, new List<ISqlCommandParameter>(), false);
//        }
    }
}