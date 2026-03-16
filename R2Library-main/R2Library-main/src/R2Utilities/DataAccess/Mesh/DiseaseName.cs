using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using R2Library.Data.ADO.Core;
using R2Library.Data.ADO.Core.SqlCommandParameters;

namespace R2Utilities.DataAccess.Mesh
{
	public class DiseaseName : FactoryBase, IDataEntity
	{
		#region Methods
		public void Populate(SqlDataReader reader)
		{
			try
			{
				Id = GetInt32Value(reader, "iDiseaseNameId", -1);
				Name = GetStringValue(reader, "vchDiseaseName");
				DiseaseDescription = GetStringValue(reader, "vchDiseaseDesc");
				DiseaseUrl = GetStringValue(reader, "vchDiseaseUrl");
				CreatorId = GetStringValue(reader, "vchCreatorId");
				CreationDate = GetDateValue(reader, "dtCreationDate");
				UpdaterId = GetStringValue(reader, "vchUpdaterId");
				LastUpdate = GetDateValue(reader, "dtLastUpdate");
				RecordStatus = GetByteValue(reader, "tiRecordStatus", 0);
				ParentDiseaseNameId = GetInt32Value(reader, "iParentDiseaseNameId");
				RelationName = GetStringValue(reader, "vchRelationName");
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
									new Int32NullParameter("DiseaseNameId", Id),
									new StringParameter("DiseaseName", Name),
									new StringParameter("DiseaseDesc", DiseaseDescription),
									new StringParameter("DiseaseUrl", DiseaseUrl),
									new StringParameter("CreatorId", CreatorId),
									new DateTimeParameter("CreationDate", CreationDate),
									new StringParameter("UpdaterId", UpdaterId),
									new DateTimeParameter("LastUpdate", LastUpdate),
									new Int32Parameter("RecordStatus", RecordStatus),
									new Int32NullParameter("ParentDiseaseNameId", ParentDiseaseNameId),
									new StringParameter("RelationName", RelationName)
								}.ToArray();
		}

		public void UpdateFrom(MeshTerm meshTerm, string updaterId)
		{
			DiseaseDescription = meshTerm.ScopeNote;
			Name = meshTerm.DescriptorName;
			RecordStatus = 1;
			RelationName = meshTerm.TreeNumber;
			LastUpdate = DateTime.Now;
			UpdaterId = updaterId;
			DiseaseUrl = null;
		}

		/*private string BuildDiseaseDescription(string diseaseDescription)
		{
			if (Clean(diseaseDescription).Contains(DiseaseDescription))
				return diseaseDescription;

			if (!Clean(DiseaseDescription).Contains(Clean(diseaseDescription)))
				return DiseaseDescription + "; " + diseaseDescription;

			return DiseaseDescription;
		}*/

		/*private static string Clean(string s)
		{
			return s.ToLower().Trim().Trim(new [] {'.'});
		}*/

		public static DiseaseName CreateFrom(MeshTerm meshTerm, string creatorId)
		{
			return new DiseaseName
			{
				CreatorId = creatorId,
				CreationDate = DateTime.Now,
				DiseaseDescription = meshTerm.ScopeNote,
				Name = meshTerm.DescriptorName,
				RecordStatus = 1,
				RelationName = meshTerm.TreeNumber,
				DiseaseUrl = null
			};
		}
		#endregion Methods

		#region Properties
		public int Id { get; set; }
		public string Name { get; set; }
		public string DiseaseDescription { get; set; }
		public string DiseaseUrl { get; set; }
		public string CreatorId { get; set; }
		public DateTime CreationDate { get; set; }
		public string UpdaterId { get; set; }
		public DateTime LastUpdate { get; set; }
		public short RecordStatus { get; set; }
		public int? ParentDiseaseNameId { get; set; }
		public string RelationName { get; set; }
		#endregion Properties
	}

	public static class DiseaseNameExtensions
	{
		public static DiseaseName FindFrom(this IEnumerable<DiseaseName> diseases, MeshTerm meshTerm)
		{
			return diseases.SingleOrDefault(d => d.Name == meshTerm.DescriptorName && d.RelationName == meshTerm.TreeNumber);
		}
	}
}
