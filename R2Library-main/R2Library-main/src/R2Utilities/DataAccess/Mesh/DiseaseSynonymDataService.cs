using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.Core.SqlCommandParameters;
using R2Library.Data.ADO.R2Utility.DataServices;
using R2Utilities.Infrastructure.Settings;

namespace R2Utilities.DataAccess.Mesh
{
	public class DiseaseSynonymDataService : DataServiceBase
	{
		#region Fields
		private const int MaxSynonymLength = 100;

		private static readonly string SqlSelectAll = new StringBuilder()
			.Append("select iDiseaseSynonymId, vchDiseaseSynonym, iDiseaseNameId, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus ")
			.Append("from tdiseasesynonym  ")
			.ToString();

		private static readonly string SqlInsert = new StringBuilder()
			.Append("insert into tdiseasesynonym (vchDiseaseSynonym, iDiseaseNameId, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus) ")
			.Append("values(@DiseaseSynonym, @DiseaseNameId, @CreatorId, @CreationDate, @UpdaterId, @LastUpdate, @RecordStatus)  ")
			.ToString();

		private static readonly string SqlUpdate = new StringBuilder()
			.Append("update tdiseasesynonym ")
			.Append("set vchDiseaseSynonym = @DiseaseSynonym, iDiseaseNameId = @DiseaseNameId, vchCreatorId = @CreatorId, dtCreationDate = @CreationDate, ")
			.Append("	vchUpdaterId = @UpdaterId, dtLastUpdate = @LastUpdate, tiRecordStatus = @RecordStatus ")
			.Append("where iDiseaseSynonymId = @DiseaseSynonymId ")
			.ToString();

		private static readonly string SqlInactivate = new StringBuilder()
			.Append("update tdiseasesynonym ")
			.Append("set tiRecordStatus = 0, vchUpdaterId = @UpdaterId ")
			.Append("from tdiseasesynonym ds ")
			.Append("inner join tdiseasename dn ")
			.Append("on dn.iDiseaseNameId = ds.iDiseaseNameId AND dn.vchRelationName LIKE 'C%' AND dn.tiRecordStatus = 0 ")
			.ToString();

		#endregion Fields

		public DiseaseSynonymDataService(IR2UtilitiesSettings r2UtilitiesSettings
		)
		{
	        ConnectionString = r2UtilitiesSettings.R2DatabaseConnection;
		}

		#region Methods
		public int UpdateDiseaseSynonyms(string taskName)
		{
			DiseaseSynonyms = SelectAll().ToList();

			int count = 0;
			foreach (MeshTerm meshTerm in MeshTerms)
			{
				if (meshTerm.Term.Length > MaxSynonymLength) continue;

				DiseaseName diseaseName = DiseaseNames.FindFrom(meshTerm);

				DiseaseSynonym synonym =
					DiseaseSynonyms.SingleOrDefault(s => s.Synonym == meshTerm.Term && s.DiseaseNameId == diseaseName.Id);

				if (synonym == null)
				{
					synonym = DiseaseSynonym.CreateFrom(meshTerm, diseaseName.Id, taskName);
					if (synonym.Synonym == diseaseName.Name) continue;
					Insert(synonym);
				}
				else
				{
					synonym.UpdateFrom(meshTerm, diseaseName.Id, taskName);
					if (synonym.Synonym == diseaseName.Name) continue;
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

		private IEnumerable<DiseaseSynonym> SelectAll()
		{
			return GetEntityList<DiseaseSynonym>(SqlSelectAll, null, true);
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
		public IEnumerable<DiseaseName> DiseaseNames { get; set; }
		public IEnumerable<DiseaseSynonym> DiseaseSynonyms { get; protected set; }
		#endregion
	}
}
