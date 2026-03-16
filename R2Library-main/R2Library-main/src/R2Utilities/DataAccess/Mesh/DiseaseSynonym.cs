using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using R2Library.Data.ADO.Core;
using R2Library.Data.ADO.Core.SqlCommandParameters;

namespace R2Utilities.DataAccess.Mesh
{
	public class DiseaseSynonym : FactoryBase, IDataEntity
	{
		#region Methods
		public void Populate(SqlDataReader reader)
		{
			try
			{
				Id = GetInt32Value(reader, "iDiseaseSynonymId", -1);
				Synonym = GetStringValue(reader, "vchDiseaseSynonym");
				DiseaseNameId = GetInt32Value(reader, "iDiseaseNameId", -1);
				CreatorId = GetStringValue(reader, "vchCreatorId");
				CreationDate = GetDateValue(reader, "dtCreationDate");
				UpdaterId = GetStringValue(reader, "vchUpdaterId");
				LastUpdate = GetDateValue(reader, "dtLastUpdate");
				RecordStatus = GetByteValue(reader, "tiRecordStatus", 0);
			}
			catch (Exception ex)
			{
				Log.ErrorFormat(ex.Message, ex);
				throw;
			}
		}

		public ISqlCommandParameter[] ToParameters()
		{
			return new List<ISqlCommandParameter>
                                {
									new Int32NullParameter("DiseaseSynonymId", Id),
									new StringParameter("DiseaseSynonym", Synonym),
									new Int32NullParameter("DiseaseNameId", DiseaseNameId),
									new StringParameter("CreatorId", CreatorId),
									new DateTimeParameter("CreationDate", CreationDate),
									new StringParameter("UpdaterId", UpdaterId),
									new DateTimeParameter("LastUpdate", LastUpdate),
									new Int32Parameter("RecordStatus", RecordStatus)
								}.ToArray();
		}

		public void UpdateFrom(MeshTerm meshTerm, int diseaseNameId, string updaterId)
		{
			Synonym = meshTerm.Term;
			DiseaseNameId = diseaseNameId;
			UpdaterId = updaterId;
			LastUpdate = DateTime.Now;
			RecordStatus = 1;
		}

		public static DiseaseSynonym CreateFrom(MeshTerm meshTerm, int diseaseNameId, string creatorId)
		{
			return new DiseaseSynonym
			{
				Synonym = meshTerm.Term,
				DiseaseNameId = diseaseNameId,
				CreatorId = creatorId,
				CreationDate = DateTime.Now,
				RecordStatus = 1
			};
		}
		#endregion Methods

		#region Properties
		public int Id { get; set; }
		public string Synonym { get; set; }
		public int DiseaseNameId { get; set; }
		public string CreatorId { get; set; }
		public DateTime CreationDate { get; set; }
		public string UpdaterId { get; set; }
		public DateTime LastUpdate { get; set; }
		public short RecordStatus { get; set; }
		#endregion Properties
	}
}
