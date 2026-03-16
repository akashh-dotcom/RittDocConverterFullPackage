#region

using System;
using System.Collections.Generic;
using System.Text;
using R2Library.Data.ADO.Core.SqlCommandParameters;
using R2Library.Data.ADO.R2.DataServices;
using R2Utilities.Infrastructure.Settings;
using R2V2.Core.Audit;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2Utilities.DataAccess
{
    public class DoodyUpdateService : DataServiceBase
    {
        private readonly ILog<DoodyUpdateService> _log;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;

        public DoodyUpdateService(ILog<DoodyUpdateService> log, IR2UtilitiesSettings r2UtilitiesSettings)
        {
            _log = log;
            _r2UtilitiesSettings = r2UtilitiesSettings;
        }

        public List<CoreResource> GetDctCoreResources()
        {
            var newDctResources = new List<CoreResource>();
            try
            {
                var sqlBuilder = new StringBuilder();

                //Insert
                sqlBuilder
                    .Append("select r.iResourceId, r.vchResourceIsbn ")
                    .Append("from tResource r ")
                    .Append(
                        "join RittenhouseWeb..DoodyCoreTitleScore dct on r.vchIsbn13 = dct.isbn13 and dct.isEssential = 0 ")
                    .Append(
                        "left join tResourceCollection rc on r.iResourceId = rc.iResourceId and rc.iCollectionId = 5 ")
                    .Append("where rc.iResourceCollectionId is null ")
                    .Append(" group by r.iResourceId, r.vchResourceIsbn");

                var insertedDct =
                    GetEntityList<CoreResource>(sqlBuilder.ToString(), new List<ISqlCommandParameter>(), true);
                newDctResources.AddRange(insertedDct);

                //Update
                sqlBuilder = new StringBuilder();
                sqlBuilder
                    .Append("select r.iResourceId, r.vchResourceIsbn ")
                    .Append("from tResource r ")
                    .Append(
                        "join tResourceCollection rc on r.iResourceId = rc.iResourceId and rc.iCollectionId = 5 and rc.tiRecordStatus = 0 ")
                    .Append(
                        "join RittenhouseWeb..DoodyCoreTitleScore dct on r.vchIsbn13 = dct.isbn13 and dct.isEssential = 0 ")
                    .Append(" group by r.iResourceId, r.vchResourceIsbn");

                var udpatedDct =
                    GetEntityList<CoreResource>(sqlBuilder.ToString(), new List<ISqlCommandParameter>(), true);
                newDctResources.AddRange(udpatedDct);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }

            return newDctResources;
        }

        public List<CoreResource> GetDctEssentialCoreResources()
        {
            var newDctResoruces = new List<CoreResource>();
            try
            {
                var sqlBuilder = new StringBuilder();

                //Insert
                sqlBuilder.Append("select r.iResourceId, r.vchResourceIsbn ")
                    .Append("from tResource r ")
                    .Append(
                        "join RittenhouseWeb..DoodyCoreTitleScore dct on r.vchIsbn13 = dct.isbn13 and dct.isEssential = 1")
                    .Append(
                        "left join tResourceCollection rc on r.iResourceId = rc.iResourceId and rc.iCollectionId = 6 ")
                    .Append("where rc.iResourceCollectionId is null ")
                    .Append(" group by r.iResourceId, r.vchResourceIsbn");

                var insertedDctEssential =
                    GetEntityList<CoreResource>(sqlBuilder.ToString(), new List<ISqlCommandParameter>(), true);
                newDctResoruces.AddRange(insertedDctEssential);

                //Update
                sqlBuilder = new StringBuilder();
                sqlBuilder.Append("select r.iResourceId, r.vchResourceIsbn ")
                    .Append("from tResource r ")
                    .Append(
                        "join tResourceCollection rc on r.iResourceId = rc.iResourceId and rc.iCollectionId = 6  and rc.tiRecordStatus = 0 ")
                    .Append(
                        "join RittenhouseWeb..DoodyCoreTitleScore dct on r.vchIsbn13 = dct.isbn13 and dct.isEssential = 1 ")
                    .Append(" group by r.iResourceId, r.vchResourceIsbn");

                var udpatedDctEssential =
                    GetEntityList<CoreResource>(sqlBuilder.ToString(), new List<ISqlCommandParameter>(), true);
                newDctResoruces.AddRange(udpatedDctEssential);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }

            return newDctResoruces;
        }

        public void UpdateDct(out int inserted, out int updated, out int deleted)
        {
            try
            {
                var date = DateTime.Now;

                //--DCT Insert
                var sql = new StringBuilder()
                    .Append(
                        "Insert into tResourceCollection (iCollectionId, iResourceId, vchCreatorId, dtCreationDate, tiRecordStatus) ")
                    .AppendFormat("select 5, r.iResourceId, 'UpdateDct', '{0}', 1 ", date)
                    .Append("from tResource r ")
                    .Append(
                        "join RittenhouseWeb..DoodyCoreTitleScore dct on r.vchIsbn13 = dct.isbn13 ") //Essential is also DCT
                    //.Append("join RittenhouseWeb..DoodyCoreTitleScore dct on r.vchIsbn13 = dct.isbn13 and dct.isEssential = 0 ")
                    .Append(
                        "left join tResourceCollection rc on r.iResourceId = rc.iResourceId and rc.iCollectionId = 5 ")
                    .Append("where rc.iResourceCollectionId is null ")
                    .Append(" group by r.iResourceId")
                    .ToString();


                inserted = ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);

                sql = new StringBuilder()
                    .Append(" INSERT INTO tResourceAudit ")
                    .AppendFormat(" select r.iResourceId, {0}, 'UpdateDct', getdate() ",
                        (int)ResourceAuditType.Unspecificed)
                    .Append(" , '[Resource has become a Doody Core Title]' ")
                    .Append(" from tResource r ")
                    .Append(" join tResourceCollection rc on r.iResourceId = rc.iResourceId ")
                    .AppendFormat(
                        " where rc.vchCreatorId = 'UpdateDct' and rc.iCollectionId = 5 and rc.dtCreationDate = '{0}' ",
                        date)
                    .ToString();

                ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);


                //--DCT Update
                sql = new StringBuilder()
                    .Append("Update tResourceCollection ")
                    .Append("set tiRecordStatus = 1, ")
                    .Append("vchUpdaterId = 'UpdateDct', ")
                    .AppendFormat("dtLastUpdate = '{0}' ", date)
                    .Append("from tResource r ")
                    .Append(
                        "join tResourceCollection rc on r.iResourceId = rc.iResourceId and rc.iCollectionId = 5 and rc.tiRecordStatus = 0 ")
                    .Append("join RittenhouseWeb..DoodyCoreTitleScore dct on r.vchIsbn13 = dct.isbn13 ")
                    .ToString();

                updated = ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);


                sql = new StringBuilder()
                    .Append(" INSERT INTO tResourceAudit ")
                    .AppendFormat(" select r.iResourceId, {0}, 'UpdateDct', getdate() ",
                        (int)ResourceAuditType.Unspecificed)
                    .Append(" , '[Resource has become a Doody Core Title]' ")
                    .Append(" from tResource r ")
                    .Append(" join tResourceCollection rc on r.iResourceId = rc.iResourceId ")
                    .AppendFormat(
                        " where rc.vchUpdaterId = 'UpdateDct' and rc.iCollectionId = 5 and rc.dtLastUpdate = '{0}' ",
                        date)
                    .ToString();

                ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);

                //--DCT Delete
                sql = new StringBuilder()
                    .Append("Update tResourceCollection ")
                    .Append("set tiRecordStatus = 0, ")
                    .Append("vchUpdaterId = 'UpdateDct', ")
                    .AppendFormat("dtLastUpdate = '{0}' ", date)
                    .Append("from tResource r ")
                    .Append(
                        "join tResourceCollection rc on r.iResourceId = rc.iResourceId and rc.iCollectionId = 5 and rc.tiRecordStatus = 1 ")
                    .Append("left join RittenhouseWeb..DoodyCoreTitleScore dct on r.vchIsbn13 = dct.isbn13 ")
                    .Append("where  dct.isbn13 is null ")
                    .ToString();

                deleted = ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }

        public void UpdateDctEssential(out int inserted, out int updated, out int deleted)
        {
            try
            {
                var date = DateTime.Now;

                //--DCT Insert
                var sql = new StringBuilder()
                    .Append(
                        "Insert into tResourceCollection (iCollectionId, iResourceId, vchCreatorId, dtCreationDate, tiRecordStatus) ")
                    .AppendFormat("select 6, r.iResourceId, 'UpdateDctEssential', '{0}', 1 ", date)
                    .Append("from tResource r ")
                    .Append(
                        "join RittenhouseWeb..DoodyCoreTitleScore dct on r.vchIsbn13 = dct.isbn13 and dct.isEssential = 1")
                    .Append(
                        "left join tResourceCollection rc on r.iResourceId = rc.iResourceId and rc.iCollectionId = 6 ")
                    .Append("where rc.iResourceCollectionId is null ")
                    .Append(" group by r.iResourceId")
                    .ToString();

                inserted = ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);

                //Inserts DCT for all DCT Essential
                sql = new StringBuilder()
                    .Append(
                        "Insert into tResourceCollection (iCollectionId, iResourceId, vchCreatorId, dtCreationDate, tiRecordStatus) ")
                    .AppendFormat("select 5, r.iResourceId, 'UpdateDctEssential', '{0}', 1 ", date)
                    .Append("from tResource r ")
                    .Append(
                        "join RittenhouseWeb..DoodyCoreTitleScore dct on r.vchIsbn13 = dct.isbn13 and dct.isEssential = 1")
                    .Append(
                        "left join tResourceCollection rc on r.iResourceId = rc.iResourceId and rc.iCollectionId = 5 ")
                    .Append("where rc.iResourceCollectionId is null ")
                    .Append(" group by r.iResourceId")
                    .ToString();
                ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);

                sql = new StringBuilder()
                    .Append(" INSERT INTO tResourceAudit ")
                    .AppendFormat(" select r.iResourceId, {0}, 'UpdateDctEssential', getdate() ",
                        (int)ResourceAuditType.Unspecificed)
                    .Append(" , '[Resource has become a Doody Core Essential Purchase]' ")
                    .Append(" from tResource r ")
                    .Append(" join tResourceCollection rc on r.iResourceId = rc.iResourceId ")
                    .AppendFormat(" where rc.vchCreatorId = 'UpdateDctEssential' and rc.dtCreationDate = '{0}'", date)
                    .ToString();

                ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);


                //--DCT Update
                sql = new StringBuilder()
                    .Append("Update tResourceCollection ")
                    .Append("set tiRecordStatus = 1, ")
                    .Append("vchUpdaterId = 'DoodyUpdateTask', ")
                    .Append("dtLastUpdate = GETDATE() ")
                    .Append("from tResource r ")
                    .Append(
                        "join tResourceCollection rc on r.iResourceId = rc.iResourceId and rc.iCollectionId in (5,6)  and rc.tiRecordStatus = 0 ")
                    .Append(
                        "join RittenhouseWeb..DoodyCoreTitleScore dct on r.vchIsbn13 = dct.isbn13 and dct.isEssential = 1 ")
                    .ToString();

                updated = ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);

                sql = new StringBuilder()
                    .Append(" INSERT INTO tResourceAudit ")
                    .AppendFormat(" select r.iResourceId, {0}, 'UpdateDctEssential', getdate() ",
                        (int)ResourceAuditType.Unspecificed)
                    .Append(" , '[Resource has become a Doody Core Essential Purchase]' ")
                    .Append(" from tResource r ")
                    .Append(" join tResourceCollection rc on r.iResourceId = rc.iResourceId ")
                    .AppendFormat(" where rc.vchUpdaterId = 'UpdateDctEssential' and rc.dtLastUpdate = '{0}' ", date)
                    .ToString();

                ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);

                //DCT Essential Delete --No need to delete the DCT for DCT Essentials because DCT update handles that. 
                sql = new StringBuilder()
                    .Append(" Update tResourceCollection ")
                    .Append(" set tiRecordStatus = 0, ")
                    .Append(" vchUpdaterId = 'UpdateDctEssential', ")
                    .AppendFormat(" dtLastUpdate = '{0}' ", date)
                    .Append("from tResource r ")
                    .Append(
                        "join tResourceCollection rc on r.iResourceId = rc.iResourceId and rc.iCollectionId = 6 and rc.tiRecordStatus = 1 ")
                    .Append(
                        "left join RittenhouseWeb..DoodyCoreTitleScore dct on r.vchIsbn13 = dct.isbn13  and dct.isEssential = 1 ")
                    .Append("where dct.isbn13 is null ")
                    .ToString();

                deleted = ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }

        public void UpdateDoodyReview(out int inserted, out int deleted)
        {
            try
            {
                var date = DateTime.Now;

                //--Doody Review Update
                var sql = new StringBuilder()
                    .Append("update tResource ")
                    .Append("set tiDoodyReview = 1 ")
                    .Append(" , vchUpdaterId = 'UpdateDoodyReview' ")
                    .AppendFormat(" , dtLastUpdate = '{0}' ", date)
                    .Append("from tResource r ")
                    .Append("join RittenhouseWeb..Product p on r.vchIsbn13 = p.isbn13 ")
                    .Append("where  p.doodyRating > 0 ")
                    .Append("and p.sku not like 'R2P%' ")
                    .Append("and r.tiDoodyReview = 0 ")
                    .ToString();

                inserted = ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);

                sql = new StringBuilder()
                    .Append(" INSERT INTO tResourceAudit ")
                    .AppendFormat(" select iResourceId, {0}, 'UpdateDoodyReview', getdate() ",
                        (int)ResourceAuditType.Unspecificed)
                    .Append(" , '[tiDoodyReview changed from 0 to 1]' ")
                    .AppendFormat(
                        " from tResource where dtLastUpdate = '{0}' and vchUpdaterId = 'UpdateDoodyReview' and tiDoodyReview = 1",
                        date)
                    .ToString();

                ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);


                //--Doody Review Delete
                sql = new StringBuilder()
                    .Append("update tResource ")
                    .Append("set tiDoodyReview = 0 ")
                    .Append(" , vchUpdaterId = 'UpdateDoodyReview' ")
                    .Append(" , dtLastUpdate = GETDATE() ")
                    .Append("from tResource r ")
                    .Append("join RittenhouseWeb..Product p on r.vchIsbn13 = p.isbn13 ")
                    .Append("where p.doodyRating is null and r.tiDoodyReview = 1 ")
                    .Append("and p.sku not like 'R2P%' ")
                    .Append("and r.tiDoodyReview = 1 ")
                    .ToString();

                deleted = ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);

                sql = new StringBuilder()
                    .Append(" INSERT INTO tResourceAudit ")
                    .AppendFormat(" select iResourceId, {0}, 'UpdateDoodyReview', getdate() ",
                        (int)ResourceAuditType.Unspecificed)
                    .Append(" , '[tiDoodyReview changed from 1 to 0]' ")
                    .AppendFormat(
                        " from tResource where dtLastUpdate = '{0}' and vchUpdaterId = 'UpdateDoodyReview' and tiDoodyReview = 0",
                        date)
                    .ToString();

                ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);


                sql = $@"
INSERT INTO tResourceAudit
select r.iResourceId, {(int)ResourceAuditType.Unspecificed}, 'UpdateDoodyReview', GETDATE(), '[Inserted to iCollectionId 52]'
from tResource r
left join tResourceCollection rc on r.iResourceId = rc.iResourceId and rc.iCollectionId = 52
where rc.iCollectionId is null and r.tiDoodyReview = 1;
Insert into tResourceCollection(iCollectionId, iResourceId, vchCreatorId, dtCreationDate, tiRecordStatus)
select 52, r.iResourceId, 'UpdateDoodyReview', GETDATE(), 1
from tResource r
left join tResourceCollection rc on r.iResourceId = rc.iResourceId and rc.iCollectionId = 52
where rc.iCollectionId is null and r.tiDoodyReview = 1;";
                ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);

                sql = $@"
INSERT INTO tResourceAudit
select r.iResourceId, {(int)ResourceAuditType.Unspecificed}, 'UpdateDoodyReview', GETDATE(), '[Updated to iCollectionId 52]'
from tResourceCollection rc
join tResource r on rc.iResourceId = r.iResourceId and r.tiDoodyReview = 1
where rc.tiRecordStatus = 0 and rc.iCollectionId = 52;
Update tResourceCollection
set tiRecordStatus = 1,
vchUpdaterId = 'UpdateDoodyReview',
dtLastUpdate = GETDATE()
from tResourceCollection rc
join tResource r on rc.iResourceId = r.iResourceId and r.tiDoodyReview = 1
where rc.tiRecordStatus = 0 and rc.iCollectionId = 52;";
                ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);

                sql = $@"
INSERT INTO tResourceAudit
select r.iResourceId, {(int)ResourceAuditType.Unspecificed}, 'UpdateDoodyReview', GETDATE(), '[Deleted from iCollectionId 52]'
from tResourceCollection rc
join tResource r on rc.iResourceId = r.iResourceId and r.tiDoodyReview = 0
where rc.tiRecordStatus = 1 and rc.iCollectionId = 52
Update tResourceCollection
set tiRecordStatus = 0,
vchUpdaterId = 'UpdateDoodyReview',
dtLastUpdate = GETDATE()
from tResourceCollection rc
join tResource r on rc.iResourceId = r.iResourceId and r.tiDoodyReview = 0
where rc.tiRecordStatus = 1 and rc.iCollectionId = 52";
                ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }

        public void UpdateDoodyRating(out int inserted, out int deleted)
        {
            try
            {
                #region Resource.siDoodyRating Handling

                var sql = $@"INSERT INTO tResourceAudit
select iResourceId, {(int)ResourceAuditType.Unspecificed}, 'UpdateDoodyRating', getdate()
, '[siDoodyRating changed from ' + isnull(cast(r.siDoodyRating as varchar(20)), 'null') + ' to ' + cast(p.doodyRating as varchar(20)) + ']'
from tResource r
join RittenhouseWeb..Product p on r.vchIsbn13 = p.isbn13
where p.doodyRating is not null and p.doodyRating > 0 and (r.siDoodyRating is null or r.siDoodyRating <> p.doodyRating) and p.sku not like 'R2P%'";

                ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);

                sql = @"update tResource
set siDoodyRating = p.doodyRating
, vchUpdaterId = 'UpdateDoodyRating'
, dtLastUpdate = GETDATE()
from tResource r
join RittenhouseWeb..Product p on r.vchIsbn13 = p.isbn13
where p.doodyRating is not null and p.doodyRating > 0 and(r.siDoodyRating is null or r.siDoodyRating <> p.doodyRating) and p.sku not like 'R2P%'";

                inserted = ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);

                //--Doody Rating Delete
                sql = $@"INSERT INTO tResourceAudit
select iResourceId, {(int)ResourceAuditType.Unspecificed}, 'UpdateDoodyRating', getdate()
, '[siDoodyRating changed from ' + isnull(cast(r.siDoodyRating as varchar(20)), 'null') + ' to ' + isnull(cast(p.doodyRating as varchar(20)), 'null') + ']'
from tResource r
join RittenhouseWeb..Product p on r.vchIsbn13 = p.isbn13
where (p.doodyRating is null or p.doodyRating = 0) and (r.siDoodyRating is not null)
and p.sku not like 'R2P%'";
                ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);

                sql = @"update tResource
set siDoodyRating = null
, vchUpdaterId = 'UpdateDoodyRating'
, dtLastUpdate = GETDATE()
from tResource r
join RittenhouseWeb..Product p on r.vchIsbn13 = p.isbn13
where (p.doodyRating is null or p.doodyRating = 0) and (r.siDoodyRating is not null)
and p.sku not like 'R2P%'";

                deleted = ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);

                #endregion

                #region ResourceCollection Handling

                sql = $@"
INSERT INTO tResourceAudit
select r.iResourceId, {(int)ResourceAuditType.Unspecificed}, 'UpdateDoodyRating', GETDATE(), '[Inserted to iCollectionId 51]'
from tResource r
left join tResourceCollection rc on r.iResourceId = rc.iResourceId and rc.iCollectionId = 51
where rc.iCollectionId is null and (r.siDoodyRating is not null and r.siDoodyRating >= 90 );
Insert into tResourceCollection(iCollectionId, iResourceId, vchCreatorId, dtCreationDate, tiRecordStatus)
select 51, r.iResourceId, 'UpdateDoodyRating', GETDATE(), 1
from tResource r
left join tResourceCollection rc on r.iResourceId = rc.iResourceId and rc.iCollectionId = 51
where rc.iCollectionId is null and (r.siDoodyRating is not null and r.siDoodyRating >= 90 );";
                ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);

                sql = $@"
INSERT INTO tResourceAudit
select r.iResourceId, {(int)ResourceAuditType.Unspecificed}, 'UpdateDoodyRating', GETDATE(), '[Updated to iCollectionId 51]'
from tResourceCollection rc
join tResource r on rc.iResourceId = r.iResourceId and (r.siDoodyRating is not null and r.siDoodyRating >= 90)
where rc.tiRecordStatus = 0 and rc.iCollectionId = 51;
Update tResourceCollection
set tiRecordStatus = 1,
vchUpdaterId = 'UpdateDoodyRating',
dtLastUpdate = GETDATE()
from tResourceCollection rc
join tResource r on rc.iResourceId = r.iResourceId and (r.siDoodyRating is not null and r.siDoodyRating >= 90)
where rc.tiRecordStatus = 0 and rc.iCollectionId = 51;";
                ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);

                sql = $@"
INSERT INTO tResourceAudit
select r.iResourceId, {(int)ResourceAuditType.Unspecificed}, 'UpdateDoodyRating', GETDATE(), '[Deleted from iCollectionId 51]'
from tResourceCollection rc
join tResource r on rc.iResourceId = r.iResourceId and (r.siDoodyRating is null or r.siDoodyRating < 90)
where rc.tiRecordStatus = 1 and rc.iCollectionId = 51
Update tResourceCollection
set tiRecordStatus = 0,
vchUpdaterId = 'UpdateDoodyRating',
dtLastUpdate = GETDATE()
from tResourceCollection rc
join tResource r on rc.iResourceId = r.iResourceId and (r.siDoodyRating is null or r.siDoodyRating < 90)
where rc.tiRecordStatus = 1 and rc.iCollectionId = 51";
                ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);


                sql = $@"
INSERT INTO tResourceAudit
select r.iResourceId, {(int)ResourceAuditType.Unspecificed}, 'UpdateDoodyRating', GETDATE(), '[Inserted to iCollectionId 50]'
from tResource r
left join tResourceCollection rc on r.iResourceId = rc.iResourceId and rc.iCollectionId = 50
where rc.iCollectionId is null and (r.siDoodyRating is not null and r.siDoodyRating >= 97 );
Insert into tResourceCollection(iCollectionId, iResourceId, vchCreatorId, dtCreationDate, tiRecordStatus)
select 50, r.iResourceId, 'UpdateDoodyRating', GETDATE(), 1
from tResource r
left join tResourceCollection rc on r.iResourceId = rc.iResourceId and rc.iCollectionId = 50
where rc.iCollectionId is null and (r.siDoodyRating is not null and r.siDoodyRating >= 97 );";
                ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);

                sql = $@"
INSERT INTO tResourceAudit
select r.iResourceId, {(int)ResourceAuditType.Unspecificed}, 'UpdateDoodyRating', GETDATE(), '[Updated to iCollectionId 50]'
from tResourceCollection rc
join tResource r on rc.iResourceId = r.iResourceId and (r.siDoodyRating is not null and r.siDoodyRating >= 97)
where rc.tiRecordStatus = 0 and rc.iCollectionId = 50;
Update tResourceCollection
set tiRecordStatus = 1,
vchUpdaterId = 'UpdateDoodyRating',
dtLastUpdate = GETDATE()
from tResourceCollection rc
join tResource r on rc.iResourceId = r.iResourceId and (r.siDoodyRating is not null and r.siDoodyRating >= 97)
where rc.tiRecordStatus = 0 and rc.iCollectionId = 22;";
                ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);

                sql = $@"
INSERT INTO tResourceAudit
select r.iResourceId, {(int)ResourceAuditType.Unspecificed}, 'UpdateDoodyRating', GETDATE(), '[Deleted from iCollectionId 50]'
from tResourceCollection rc
join tResource r on rc.iResourceId = r.iResourceId and (r.siDoodyRating is null or r.siDoodyRating < 97)
where rc.tiRecordStatus = 1 and rc.iCollectionId = 50
Update tResourceCollection
set tiRecordStatus = 0,
vchUpdaterId = 'UpdateDoodyRating',
dtLastUpdate = GETDATE()
from tResourceCollection rc
join tResource r on rc.iResourceId = r.iResourceId and (r.siDoodyRating is null or r.siDoodyRating < 97)
where rc.tiRecordStatus = 1 and rc.iCollectionId = 50;";
                ExecuteStatement(sql, true, _r2UtilitiesSettings.R2DatabaseConnection);

                #endregion
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }
    }
}