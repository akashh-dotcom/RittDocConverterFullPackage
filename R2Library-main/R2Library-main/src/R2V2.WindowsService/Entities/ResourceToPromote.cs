#region

using System.Data.SqlClient;
using System.Text;
using R2Library.Data.ADO.Core;

#endregion

namespace R2V2.WindowsService.Entities
{
    public class ResourceToPromote : FactoryBase, IDataEntity
    {
        public int Id { get; set; }
        public virtual string Title { get; set; }
        public virtual string Isbn { get; set; }
        public virtual string ImageName { get; set; }
        public virtual short RecordStatus { get; set; }
        public virtual string Isbn10 { get; set; }
        public virtual string Isbn13 { get; set; }
        public virtual string EIsbn { get; set; }

        public void Populate(SqlDataReader reader)
        {
            //select r.iResourceId, r.vchResourceDesc, r.vchResourceTitle, r.vchResourceSubTitle, r.vchResourceAuthors, r.vchResourceAdditionalContributors
            //     , r.vchResourcePublisher, r.dtRISReleaseDate, r.dtResourcePublicationDate, r.tiBrandonHillStatus, r.vchResourceISBN, r.vchResourceEdition
            //     , r.decResourcePrice, r.decPayPerView, r.decSubScriptionPrice, r.vchResourceImageName, r.vchCopyRight, r.tiResourceReady, r.tiAllowSubscriptions
            //     , r.iPublisherId, r.iResourceStatusId, r.tiGloballyAccessible, r.vchCreatorId, r.dtCreationDate, r.vchUpdaterId, r.dtLastUpdate, r.tiRecordStatus
            //     , r.vchMARCRecord, r.vchResourceNLMCall, r.tiDrugMonograph, r.iDCTStatusId, r.tiDoodyReview, r.vchDoodyReviewURL, r.iPrevEditResourceID
            //     , r.vchAuthorXML, r.vchForthcomingDate, r.NotSaleable, r.vchResourceSortTitle, r.chrAlphaKey, r.vchIsbn10, r.vchIsbn13, r.vchEIsbn, r.vchResourceSortAuthor

            Id = GetInt32Value(reader, "iResourceId", -1);
            Title = GetStringValue(reader, "vchResourceTitle");
            Isbn = GetStringValue(reader, "vchResourceISBN");
            ImageName = GetStringValue(reader, "vchResourceImageName");
            RecordStatus = GetByteValue(reader, "tiRecordStatus", 0);
            Isbn10 = GetStringValue(reader, "vchIsbn10");
            Isbn13 = GetStringValue(reader, "vchIsbn13");
            EIsbn = GetStringValue(reader, "vchEIsbn");
        }

        public string ToDebugString()
        {
            var sb = new StringBuilder();
            sb.Append("ResourceToPromote = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", Isbn: {0}", Isbn);
            sb.AppendFormat(", Title: {0}", Title);
            sb.AppendFormat(", ImageName: {0}", ImageName);
            sb.AppendFormat(", RecordStatus: {0}", RecordStatus);
            sb.AppendFormat(", Isbn10: {0}", Isbn10);
            sb.AppendFormat(", Isbn13: {0}", Isbn13);
            sb.AppendFormat(", EIsbn: {0}", EIsbn);
            sb.Append("]");
            return sb.ToString();
        }
    }
}