using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.Core.SqlCommandParameters;
using R2Library.Data.ADO.R2Utility.DataServices;
using R2Utilities.Infrastructure.Settings;

namespace R2Utilities.DataAccess.Mesh
{
    public class DiseaseNameDataService : DataServiceBase
    {
        #region Fields

		private static readonly string SqlSelectAll = new StringBuilder()
			.Append("select iDiseaseNameId, vchDiseaseName, vchDiseaseDesc, vchDiseaseUrl, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus, iParentDiseaseNameId, vchRelationName ")
			.Append("from tdiseasename ")
			.Append("where vchRelationName LIKE 'C%' ")
			.Append("{0} ")
			.ToString();

		private static readonly string SqlInsert = new StringBuilder()
			.Append("insert into tdiseasename (vchDiseaseName, vchDiseaseDesc, vchDiseaseUrl, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus, iParentDiseaseNameId, ")
			.Append("							vchRelationName) ")
			.Append("values(@DiseaseName, @DiseaseDesc, @DiseaseUrl, @CreatorId, @CreationDate, @UpdaterId, @LastUpdate, @RecordStatus, @ParentDiseaseNameId, @RelationName)  ")
			.ToString();

		private static readonly string SqlUpdate = new StringBuilder()
			.Append("update tdiseasename ")
			.Append("set vchDiseaseName = @DiseaseName, vchDiseaseDesc = @DiseaseDesc, vchDiseaseUrl = @DiseaseUrl, vchCreatorId = @CreatorId, dtCreationDate = @CreationDate, ")
			.Append("	vchUpdaterId = @UpdaterId, dtLastUpdate = @LastUpdate, tiRecordStatus = @RecordStatus, iParentDiseaseNameId = @ParentDiseaseNameId, vchRelationName = @RelationName ")
			.Append("where iDiseaseNameId = @DiseaseNameId ")
			.ToString();

		private static readonly string SqlInactivateAll = new StringBuilder()
			.Append("update tdiseasename ")
			.Append("set tiRecordStatus = 0 ")
			.Append("where vchRelationName LIKE 'C%' ")
			.ToString();

	    //private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        #endregion Fields

        /// <summary>
		///
		/// </summary>
		public DiseaseNameDataService(IR2UtilitiesSettings r2UtilitiesSettings)
		{
	        //_r2UtilitiesSettings = r2UtilitiesSettings;
	        ConnectionString = r2UtilitiesSettings.R2DatabaseConnection;
		}

        #region Methods
		public int UpdateDiseases(string taskName)
		{
			InactivateDiseases();

			DiseaseNames = SelectAll().ToList();

			//string userId = _r2UtilitiesSettings.CreatorId;
			int count = 0;
			foreach (MeshTerm meshTerm in MeshTerms)
			{
				DiseaseName diseaseName = DiseaseNames.FindFrom(meshTerm);

				if (diseaseName == null)
					Insert(DiseaseName.CreateFrom(meshTerm, taskName));
				else
				{
					diseaseName.UpdateFrom(meshTerm, taskName);
					Update(diseaseName);
				}

				count++;
			}

			return count;
		}

		private void InactivateDiseases()
		{
			var parameters = new List<ISqlCommandParameter>();
			ExecuteUpdateStatement(SqlInactivateAll, parameters.ToArray(), true);
		}

		public int UpdateParentDiseaseIds(string taskName)
		{
			DiseaseNames = SelectAll(true).ToList();

			int count = 0;
			foreach (DiseaseName diseaseName in DiseaseNames.Where(d => d.RecordStatus == 1))
			{
				string parentTreeNumber = MeshTerm.ParentTreeNumber(diseaseName.RelationName);

				DiseaseName parent = parentTreeNumber == null ? null : DiseaseNames.Single(d => d.RelationName == parentTreeNumber);
				diseaseName.ParentDiseaseNameId = parent != null ? parent.Id : diseaseName.Id;
				diseaseName.UpdaterId = taskName;
				diseaseName.RecordStatus = 1;

				Update(diseaseName);
				count++;
			}

			return count;
		}

		private IEnumerable<DiseaseName> SelectAll(bool activeOnly = false)
		{
			string whereClause = activeOnly ? "and tiRecordStatus = 1 " : "";
			return GetEntityList<DiseaseName>(string.Format(SqlSelectAll, whereClause), null, true);
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
	    public IEnumerable<DiseaseName> DiseaseNames { get; protected set;  }
	    #endregion
    }
}
