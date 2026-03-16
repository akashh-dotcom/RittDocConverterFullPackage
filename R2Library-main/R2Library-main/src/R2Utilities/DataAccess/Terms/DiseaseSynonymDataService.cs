#region

using System.Collections.Generic;
using System.Text;
using R2Library.Data.ADO.Core.SqlCommandParameters;
using R2Library.Data.ADO.R2Utility.DataServices;
using R2Utilities.DataAccess.Mesh;
using R2Utilities.Infrastructure.Settings;

#endregion

namespace R2Utilities.DataAccess.Terms
{
    public class DiseaseSynonymDataService : DataServiceBase
    {
        public DiseaseSynonymDataService(IR2UtilitiesSettings r2UtilitiesSettings
        )
        {
            ConnectionString = r2UtilitiesSettings.R2DatabaseConnection;
        }

        #region Fields

        private const int MaxSynonymLength = 100;

        private static readonly string SqlSelectAll = new StringBuilder()
            .Append(
                "select iDiseaseSynonymId, vchDiseaseSynonym, iDiseaseNameId, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus ")
            .Append("from tdiseasesynonym  ")
            .Append("where idiseasenameid is not null  ")
            .ToString();

        private static readonly string SqlInsert = new StringBuilder()
            .Append(
                "insert into tdiseasesynonym (vchDiseaseSynonym, iDiseaseNameId, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) ")
            .Append(
                "values(@DiseaseSynonym, @DiseaseNameId, @CreatorId, @CreationDate, @UpdaterId, @LastUpdate, @RecordStatus)  ")
            .ToString();

        private static readonly string SqlUpdate = new StringBuilder()
            .Append("update tdiseasesynonym ")
            .Append(
                "set vchDiseaseSynonym = @DiseaseSynonym, iDiseaseNameId = @DiseaseNameId, vchCreatorId = @CreatorId, dtCreationDate = @CreationDate, ")
            .Append("	vchUpdaterId = @UpdaterId, dtLastUpdate = @LastUpdate, tiRecordStatus = @RecordStatus ")
            .Append("where iDiseaseSynonymId = @DiseaseSynonymId ")
            .ToString();

        private static readonly string SqlInactivate = new StringBuilder()
            .Append("update tdiseasesynonym ")
            .Append("set tiRecordStatus = 0, vchUpdaterId = @UpdaterId ")
            .Append("from tdiseasesynonym ds ")
            .Append("inner join tdiseasename dn ")
            .Append(
                "on dn.iDiseaseNameId = ds.iDiseaseNameId AND dn.vchRelationName LIKE 'C%' AND dn.tiRecordStatus = 0 AND ds.tiRecordStatus = 1 ")
            .ToString();

        #endregion Fields

        #region Methods

        public int UpdateDiseaseSynonyms(string taskName)
        {
            DiseaseSynonyms = SelectAll();

            var count = 0;
            foreach (var meshTerm in MeshTerms)
            {
                if (meshTerm.Term.Length > MaxSynonymLength) continue;

                var diseaseName = DiseaseNames.Find(meshTerm);
                var synonym = DiseaseSynonyms.Find(meshTerm.Term, diseaseName.Id);

                if (synonym == null)
                {
                    synonym = DiseaseSynonym.CreateFrom(meshTerm, diseaseName.Id, taskName);
                    if (synonym.Synonym == diseaseName.Name) continue;
                    Insert(synonym);
                }
                else
                {
                    synonym.UpdateFrom(meshTerm, diseaseName.Id, taskName);
                    if (synonym.Synonym == diseaseName.Name || !synonym.IsChanged) continue;
                    Update(synonym);
                }

                count++;
            }

            return count;
        }

        public int InactivateSynonymsForInactiveDiseases(string taskName)
        {
            var parameters = new List<ISqlCommandParameter>
            {
                new StringParameter("UpdaterId", taskName)
            };

            return ExecuteUpdateStatement(SqlInactivate, parameters, true);
        }

        private DiseaseSynonyms SelectAll()
        {
            IEnumerable<DiseaseSynonym> diseaseSynonyms = GetEntityList<DiseaseSynonym>(SqlSelectAll, null, true);
            return new DiseaseSynonyms(diseaseSynonyms);
        }

        private void Insert(DiseaseSynonym diseaseSynonym)
        {
            ExecuteInsertStatementReturnIdentity(SqlInsert, diseaseSynonym.ToParameters(), true);
        }

        private void Update(DiseaseSynonym diseaseSynonym)
        {
            ExecuteUpdateStatement(SqlUpdate, diseaseSynonym.ToParameters(), true);
        }

        #endregion

        #region Properties

        public IEnumerable<MeshTerm> MeshTerms { get; set; }
        public DiseaseNames DiseaseNames { get; set; }
        public DiseaseSynonyms DiseaseSynonyms { get; protected set; }

        #endregion
    }
}