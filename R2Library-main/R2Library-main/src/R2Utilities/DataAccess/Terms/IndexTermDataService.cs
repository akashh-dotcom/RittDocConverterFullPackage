#region

using System.Text;
using R2Utilities.Infrastructure.Settings;

#endregion

namespace R2Utilities.DataAccess.Terms
{
    public class IndexTermDataService : TermDataService
    {
        public IndexTermDataService(IR2UtilitiesSettings r2UtilitiesSettings
            , IndexTermHighlightSettings indexTermHighlightSettings /*This needs to be of type IndexTermHighlightSettings, not simply ITermHighlightSettings*/
        ) : base(indexTermHighlightSettings, r2UtilitiesSettings.R2DatabaseConnection)
        {
        }

        private void InsertDiseaseResource(DiseaseResource diseaseResource)
        {
            /*var diseaseResourceParameters = new List<ISqlCommandParameter>();
                                {
                                    new Int32NullParameter("DiseaseNameId", diseaseResource.DiseaseId),
                                    new StringParameter("DiseaseName", diseaseResource.),
                                    new StringParameter("DiseaseDesc", DiseaseDescription),
                                    new StringParameter("DiseaseUrl", DiseaseUrl),
                                    new StringParameter("CreatorId", CreatorId),
                                    new DateTimeParameter("CreationDate", CreationDate),
                                    new StringParameter("UpdaterId", UpdaterId),
                                    new DateTimeParameter("LastUpdate", LastUpdate),
                                    new Int32Parameter("RecordStatus", RecordStatus),
                                    new Int32NullParameter("ParentDiseaseNameId", ParentDiseaseNameId),
                                    new StringParameter("RelationName", RelationName)
                                }.ToArray();*/

            //ExecuteInsertStatementReturnIdentity(SqlDiseaseResourceInsert, diseaseResourceParameters, true);
        }

        #region Fields

        private static readonly string SqlDiseaseResourceInsert = new StringBuilder()
            .Append(
                "insert into tdiseaseresource (iDiseaseNameId, vchResourceISBN, vchChapterId, vchSectionId, vchCreatorId) ")
            .Append("values(@DiseaseNameId, @ResourceISBN, @ChapterId, @SectionId, @CreatorId)  ")
            .ToString();

        private static readonly string SqlDiseaseResourceInactivate = new StringBuilder()
            .Append("update tdiseaseresource set tiRecordStatus = 0 where vchResourceISBN = @ResourceISBN")
            .ToString();

        private static readonly string SqlDrugResourceInsert = new StringBuilder()
            .Append(
                "insert into tdrugresource (iDrugListId, vchResourceISBN, vchChapterId, vchSectionId, vchCreatorId, vchTitle) ")
            .Append("values(@DrugListId, @ResourceISBN, @ChapterId, @SectionId, @CreatorId, @vchTitle)  ")
            .ToString();

        private static readonly string SqlDrugResourceInactivate = new StringBuilder()
            .Append("update tdrugresource set tiRecordStatus = 0 where vchResourceISBN = @ResourceISBN")
            .ToString();

        private static readonly string SqlDrugSynonymResourceInsert = new StringBuilder()
            .Append(
                "insert into tdrugresource (iDrugSynonymId, vchResourceISBN, vchChapterId, vchSectionId, vchCreatorId, vchTitle) ")
            .Append("values(@DrugSynonymId, @ResourceISBN, @ChapterId, @SectionId, @CreatorId, @vchTitle)  ")
            .ToString();

        private static readonly string SqlDrugSynonymResourceInactivate = new StringBuilder()
            .Append("update tdrugsynonymresource set tiRecordStatus = 0 where vchResourceISBN = @ResourceISBN")
            .ToString();

        private static readonly string SqlKeywordResourceInsert = new StringBuilder()
            .Append(
                "insert into tdrugresource (iKeywordId, vchResourceISBN, vchChapterId, vchSectionId, vchCreatorId) ")
            .Append("values(@KeywordId, @ResourceISBN, @ChapterId, @SectionId, @CreatorId)  ")
            .ToString();

        private static readonly string SqlKeywordResourceInactivate = new StringBuilder()
            .Append("update tdrugsynonymresource set tiRecordStatus = 0 where vchResourceISBN = @ResourceISBN")
            .ToString();

        #endregion
    }
}