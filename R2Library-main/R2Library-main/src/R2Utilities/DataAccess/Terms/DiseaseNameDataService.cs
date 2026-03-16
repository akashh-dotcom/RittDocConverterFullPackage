#region

using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.Core.SqlCommandParameters;
using R2Library.Data.ADO.R2Utility.DataServices;
using R2Utilities.DataAccess.Mesh;
using R2Utilities.Infrastructure.Settings;

#endregion

namespace R2Utilities.DataAccess.Terms
{
    public class DiseaseNameDataService : DataServiceBase
    {
        /// <summary>
        /// </summary>
        public DiseaseNameDataService(IR2UtilitiesSettings r2UtilitiesSettings)
        {
            //_r2UtilitiesSettings = r2UtilitiesSettings;
            ConnectionString = r2UtilitiesSettings.R2DatabaseConnection;
        }

        #region Fields

        private static readonly string SqlSelectAll = new StringBuilder()
            .Append(
                "select iDiseaseNameId, vchDiseaseName, vchDiseaseDesc, vchDiseaseUrl, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus, iParentDiseaseNameId, vchRelationName ")
            .Append("from tdiseasename ")
            .Append("where vchRelationName LIKE 'C%' ")
            .Append("{0} ")
            .ToString();

        private static readonly string SqlInsert = new StringBuilder()
            .Append(
                "insert into tdiseasename (vchDiseaseName, vchDiseaseDesc, vchDiseaseUrl, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus, iParentDiseaseNameId, ")
            .Append("							vchRelationName) ")
            .Append(
                "values(@DiseaseName, @DiseaseDesc, @DiseaseUrl, @CreatorId, @CreationDate, @UpdaterId, @LastUpdate, @RecordStatus, @ParentDiseaseNameId, @RelationName)  ")
            .ToString();

        private static readonly string SqlUpdate = new StringBuilder()
            .Append("update tdiseasename ")
            .Append(
                "set vchDiseaseName = @DiseaseName, vchDiseaseDesc = @DiseaseDesc, vchDiseaseUrl = @DiseaseUrl, vchCreatorId = @CreatorId, dtCreationDate = @CreationDate, ")
            .Append(
                "	vchUpdaterId = @UpdaterId, dtLastUpdate = @LastUpdate, tiRecordStatus = @RecordStatus, iParentDiseaseNameId = @ParentDiseaseNameId, vchRelationName = @RelationName ")
            .Append("where iDiseaseNameId = @DiseaseNameId ")
            .ToString();

        private static readonly string SqlInactivateNonMesh = new StringBuilder()
            .Append("update tdiseasename ")
            .Append("set tiRecordStatus = 0 ")
            .Append("where tiRecordStatus = 1 and vchRelationName like 'C%' and not exists ( ")
            .Append("	select * ")
            .Append("	from MeshDiseaseTerms mdt ")
            .Append("	where mdt.DescriptorName = vchDiseaseName AND mdt.TreeNumber = vchRelationName ")
            .Append(") ")
            .ToString();

        //private readonly IR2UtilitiesSettings _r2UtilitiesSettings;

        #endregion Fields

        #region Methods

        public int UpdateDiseases(string taskName)
        {
            DiseaseNames = SelectAll();

            var count = 0;
            foreach (var meshTerm in MeshTerms)
            {
                var diseaseName = DiseaseNames.Find(meshTerm);

                if (diseaseName == null)
                    Insert(DiseaseName.CreateFrom(meshTerm, taskName));
                else
                {
                    diseaseName.UpdateFrom(meshTerm, taskName);
                    if (!diseaseName.IsChanged) continue;
                    Update(diseaseName);
                }

                count++;
            }

            return count;
        }

        public int InactivateNonMeshDiseases()
        {
            var parameters = new List<ISqlCommandParameter>();
            return ExecuteUpdateStatement(SqlInactivateNonMesh, parameters.ToArray(), true);
        }

        public int UpdateParentDiseaseIds(string taskName)
        {
            DiseaseNames = SelectAll(true);

            var count = 0;
            foreach (var diseaseName in DiseaseNames.Values)
            {
                var parentTreeNumber = MeshTerm.ParentTreeNumber(diseaseName.RelationName);
                var parent = parentTreeNumber == null
                    ? null
                    : DiseaseNames.Values.Single(d => d.RelationName == parentTreeNumber);
                int? parentDiseaseNameId = parent != null ? parent.Id : diseaseName.Id;

                if (diseaseName.ParentDiseaseNameId != parentDiseaseNameId)
                {
                    diseaseName.ParentDiseaseNameId = parentDiseaseNameId;
                    diseaseName.UpdaterId = taskName;

                    Update(diseaseName);
                    count++;
                }
            }

            return count;
        }

        private DiseaseNames SelectAll(bool activeOnly = false)
        {
            var whereClause = activeOnly ? "and tiRecordStatus = 1 " : "";
            IEnumerable<DiseaseName> diseaseNames =
                GetEntityList<DiseaseName>(string.Format(SqlSelectAll, whereClause), null, true);

            return new DiseaseNames(diseaseNames);
        }

        private void Insert(DiseaseName diseaseName)
        {
            ExecuteInsertStatementReturnIdentity(SqlInsert, diseaseName.ToParameters(), true);
        }

        private void Update(DiseaseName diseaseName)
        {
            ExecuteUpdateStatement(SqlUpdate, diseaseName.ToParameters(), true);
        }

        #endregion Methods

        #region Properties

        public IEnumerable<MeshTerm> MeshTerms { get; set; }
        public DiseaseNames DiseaseNames { get; protected set; }

        #endregion
    }
}