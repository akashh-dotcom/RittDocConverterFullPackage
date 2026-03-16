#region

using System.Collections.Generic;
using System.Text;
using R2Library.Data.ADO.Core;
using R2Library.Data.ADO.Core.SqlCommandParameters;

#endregion

namespace R2Library.Data.ADO.R2.DataServices
{
    public abstract class LicensingDataServiceBase : EntityFactory
    {
        protected LicensingDataServiceBase(string databaseConnectionString)
        {
            ConnectionString = databaseConnectionString;
        }

        public IList<Institution> AddMissingAutoLicenses(bool useNewInstitutionResourceLicenseTable, int licenseCount)
        {
            var institutions = GetAutoFreeLicenseInsitutions();

            foreach (var institution in institutions)
            {
                if (useNewInstitutionResourceLicenseTable)
                {
                    var institutionResourceLicenseRecordsAdded =
                        UpdateIncorrectExistingInstitutionResourceLicenseRecords(institution.Id) +
                        AddMissingInstitutionResourceLicenseRecords(institution.Id, licenseCount);
                    institution.ResourceLicensesAdded = institutionResourceLicenseRecordsAdded;
                    Log.DebugFormat("{0}, institutionResourceLicenseRecordsAdded: {1}", institution.ToDebugString(),
                        institutionResourceLicenseRecordsAdded);
                }
                else
                {
                    var institutionResourceRecordsAdded = AddMistingInstitutionResourceRecords(institution.Id);
                    institution.ResourceLicensesAdded = AddMistingResourceInstLicenseRecords(institution.Id);
                    Log.DebugFormat("{0}, institutionResourceRecordsAdded: {1}", institution.ToDebugString(),
                        institutionResourceRecordsAdded);
                }
            }

            return institutions;
        }

        public IList<Institution> GetAutoFreeLicenseInsitutions()
        {
            var sql =
                "select iInstitutionId, vchInstitutionName, vchInstitutionAcctNum, tiAutoAddFreeLicenses from tInstitution where tiAutoAddFreeLicenses = 1";

            var parameters = new List<ISqlCommandParameter>();
            IList<Institution> list = GetEntityList<Institution>(sql, parameters, true);

            return list;
        }

        /// <param name="licenseCount">Was added because of SCT Labs - default to 15, was previously hardcoded as 3</param>
        private int AddMissingInstitutionResourceLicenseRecords(int institutionId, int licenseCount)
        {
            var insert = new StringBuilder()
                .Append(
                    "insert into tInstitutionResourceLicense (iInstitutionId, iResourceId, iLicenseCount, tiLicenseTypeId, tiLicenseOriginalSourceId ")
                .Append(
                    "        , dtFirstPurchaseDate, dtPdaAddedDate, dtPdaAddedToCartDate, vchPdaAddedToCartById, iPdaViewCount, iPdaMaxViews ")
                .Append("        , dtCreationDate, vchCreatorId, dtLastUpdate, vchUpdaterId, tiRecordStatus) ")
                .Append("    select @InstitutionId, r.iResourceId, @LicenseCount, 1, 1 ")
                .Append(
                    "            , case when (r.dtRISReleaseDate is null) then r.dtCreationDate else r.dtRISReleaseDate end ")
                .Append("            , null, null, null, 0, 0 ")
                .Append("            , getdate(), 'R2PromoteAutoLicense', null, null, 1 ")
                .Append("    from   tResource r  ")
                .Append("    where  r.tiRecordStatus = 1 ")
                .Append("      and  r.iResourceStatusId in (6,7) ")
                .Append("      and  r.NotSaleable = 0 ")
                .Append("      and  not exists ( ")
                .Append("        select * ")
                .Append("        from   tInstitutionResourceLicense irl2 ")
                .Append("         join  dbo.tInstitution i2 on i2.iInstitutionId = irl2.iInstitutionId ")
                .Append("        where  i2.tiAutoAddFreeLicenses = 1 ")
                .Append("          and  i2.iInstitutionId = @InstitutionId ")
                .Append("          and  irl2.tiRecordStatus = 1 ")
                .Append("          and  irl2.iResourceId = r.iResourceId ")
                .Append("        ) ")
                .Append("    order by r.iResourceId ");

            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("InstitutionId", institutionId),
                new Int32Parameter("LicenseCount", licenseCount)
            };
            var rows = ExecuteInsertStatementReturnRowCount(insert.ToString(), parameters.ToArray(), true);

            Log.DebugFormat("AddMissingInstitutionResourceLicenseRecords() - rows: {0}", rows);

            return rows;
        }

        /// <summary>
        ///     This method was need to fix either PDA license records or soft deleted records
        /// </summary>
        public int UpdateIncorrectExistingInstitutionResourceLicenseRecords(int institutionId)
        {
            var update = new StringBuilder()
                .Append("update irl ")
                .Append("set    tiRecordStatus = 1 ")
                .Append("     , iLicenseCount = 3 ")
                .Append("     , tiLicenseTypeId = 1 ")
                .Append("     , dtLastUpdate = getdate() ")
                .Append("     , vchUpdaterId = 'R2PromoteAutoLicense' ")
                .Append("from   tInstitutionResourceLicense irl ")
                .Append(
                    "where  irl.iInstitutionId = @InstitutionId and (irl.tiLicenseTypeId <> 1 or tiRecordStatus = 0) ")
                .Append("  and  exists ( ")
                .Append("    select * ")
                .Append("    from tResource r ")
                .Append(
                    "    where r.iResourceId = irl.iResourceId and  r.tiRecordStatus = 1 and r.iResourceStatusId in (6, 7) ")
                .Append("      and r.NotSaleable = 0 and not exists ( ")
                .Append("            select * ")
                .Append("            from tInstitutionResourceLicense irl2 ")
                .Append("            join dbo.tInstitution i2 on i2.iInstitutionId = irl2.iInstitutionId ")
                .Append("            where i2.tiAutoAddFreeLicenses = 1 and i2.iInstitutionId = @InstitutionId ")
                .Append("              and irl2.tiRecordStatus = 1 and irl2.iResourceId = r.iResourceId ")
                .Append("              and irl2.tiLicenseTypeId = 1 ")
                .Append("            ) ")
                .Append("  ) ");

            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("InstitutionId", institutionId)
            };
            var rows = ExecuteInsertStatementReturnRowCount(update.ToString(), parameters.ToArray(), true);

            Log.DebugFormat("UpdateIncorrectExistingInstitutionResourceLicenseRecords() - rows: {0}", rows);

            return rows;
        }

        public int AddMistingResourceInstLicenseRecords(int institutionId)
        {
            var insert = new StringBuilder()
                .Append(
                    "insert into tResourceInstLicense (iNumberLicenses, decLicenseAmt, vchPoNumber, iInstitutionResourceId, vchCreatorId ")
                .Append("        , dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) ")
                .Append(
                    "    select 3, 0.0, 'R2PromoteAutoLicense', ir.iInstitutionResourceId, 'R2Promote', getdate(), null, null, 1 ")
                .Append("    from   tInstitutionResource ir ")
                .Append("    where  ir.iInstitutionId =  @InstitutionId ")
                .Append("      and  ir.tiRecordStatus = 1 ")
                .Append("      and  not exists ( ")
                .Append("        select *  ")
                .Append("        from   tResourceInstLicense ril ")
                .Append(
                    "         join  dbo.tInstitutionResource ir2 on ir2.iInstitutionResourceId = ril.iInstitutionResourceId ")
                .Append("           and ir2.tiRecordStatus = 1 and ir2.iInstitutionId = @InstitutionId ")
                .Append("        where  ril.tiRecordStatus = 1 ")
                .Append("          and  ril.iInstitutionResourceId = ir.iInstitutionResourceId ")
                .Append("        ) ");

            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("InstitutionId", institutionId)
            };
            var rows = ExecuteInsertStatementReturnRowCount(insert.ToString(), parameters.ToArray(), true);

            return rows;
        }

        public int AddMistingInstitutionResourceRecords(int institutionId)
        {
            var insert = new StringBuilder()
                .Append("insert into tInstitutionResource (iResourceId, iInstitutionId, vchCreatorId, dtCreationDate, ")
                .Append("			vchUpdaterId, dtLastUpdate, tiRecordStatus) ")
                .Append("    select r.iResourceId, @InstitutionId, 'R2Promote', getdate(), null, null, 1 ")
                .Append("    from   tResource r ")
                .Append("    where  r.tiRecordStatus = 1 ")
                .Append("      and  r.iResourceStatusId in (6,7) ")
                .Append("      and  r.NotSaleable = 0 ")
                .Append("      and  not exists ( ")
                .Append("        select * ")
                .Append("        from   tInstitutionResource ir ")
                .Append("         join  dbo.tInstitution i2 on i2.iInstitutionId = ir.iInstitutionId ")
                .Append("        where  i2.tiAutoAddFreeLicenses = 1 ")
                .Append("          and  i2.iInstitutionId = @InstitutionId ")
                .Append("          and  ir.tiRecordStatus = 1 ")
                .Append("          and  ir.iResourceId = r.iResourceId ")
                .Append("        ) ");

            var parameters = new List<ISqlCommandParameter>
            {
                new Int32Parameter("InstitutionId", institutionId)
            };
            var rows = ExecuteInsertStatementReturnRowCount(insert.ToString(), parameters.ToArray(), true);

            return rows;
        }
    }
}