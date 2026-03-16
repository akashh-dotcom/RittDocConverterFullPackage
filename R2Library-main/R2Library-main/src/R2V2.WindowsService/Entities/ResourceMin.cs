#region

using System;
using System.Data.SqlClient;
using System.Text;
using R2Library.Data.ADO.Core;

#endregion

namespace R2V2.WindowsService.Entities
{
    public class ResourceMin : FactoryBase, IDataEntity
    {
        public int Id { get; set; }
        public virtual string Title { get; set; }
        public virtual string SubTitle { get; set; }
        public virtual string Authors { get; set; }
        public virtual DateTime? ReleaseDate { get; set; }
        public virtual DateTime? PublicationDate { get; set; }
        public virtual string Isbn { get; set; }
        public virtual string Edition { get; set; }
        public virtual string CopyRight { get; set; }
        public virtual int PublisherId { get; set; }
        public virtual int StatusId { get; set; }
        public virtual short RecordStatus { get; set; }

        public virtual string Isbn10 { get; set; }
        public virtual string Isbn13 { get; set; }
        public virtual string EIsbn { get; set; }

        public virtual string SortTitle { get; set; }
        public virtual string AlphaKey { get; set; }
        public virtual string PublisherName { get; set; }

        /// <summary>
        /// </summary>
        public void Populate(SqlDataReader reader)
        {
            Id = GetInt32Value(reader, "iResourceId", -1);
            Title = GetStringValue(reader, "vchResourceTitle");
            SubTitle = GetStringValue(reader, "vchResourceSubTitle");
            Authors = GetStringValue(reader, "vchResourceAuthors");
            ReleaseDate = GetDateValueOrNull(reader, "dtRISReleaseDate");
            PublicationDate = GetDateValueOrNull(reader, "dtResourcePublicationDate");

            Isbn = GetStringValue(reader, "vchResourceISBN");
            Edition = GetStringValue(reader, "vchResourceEdition");
            CopyRight = GetStringValue(reader, "vchCopyRight");

            PublisherId = GetInt32Value(reader, "iPublisherId", -1);

            StatusId = GetInt32Value(reader, "iResourceStatusId", 0);
            RecordStatus = GetByteValue(reader, "tiRecordStatus", 0);

            Isbn10 = GetStringValue(reader, "vchIsbn10");
            Isbn13 = GetStringValue(reader, "vchIsbn13");
            EIsbn = GetStringValue(reader, "vchEIsbn");

            SortTitle = GetStringValue(reader, "vchResourceSortTitle");
            AlphaKey = GetStringValue(reader, "chrAlphaKey");
            PublisherName = GetStringValue(reader, "vchPublisherName");
        }

        /// <summary>
        /// </summary>
        public string ToDebugString()
        {
            var sb = new StringBuilder("ResourceCore = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", Isbn: {0}", Isbn);
            sb.AppendFormat(", Title: {0}", Title);
            sb.AppendFormat(", SubTitle: {0}", SubTitle);
            sb.AppendFormat(", Authors: {0}", Authors);
            sb.AppendFormat(", ReleaseDate: {0}", ReleaseDate == null ? "null" : ReleaseDate.Value.ToShortDateString());
            sb.AppendFormat(", PublicationDate: {0}",
                PublicationDate == null ? "null" : PublicationDate.Value.ToShortDateString());
            sb.AppendFormat(", Isbn: {0}", Isbn);
            sb.AppendFormat(", Edition: {0}", Edition);
            sb.AppendFormat(", CopyRight: {0}", CopyRight);
            sb.AppendFormat(", PublisherId: {0}", PublisherId);
            sb.AppendFormat(", StatusId: {0}", StatusId);
            sb.AppendFormat(", RecordStatus: {0}", RecordStatus);
            sb.AppendFormat(", Isbn10: {0}", Isbn10);
            sb.AppendFormat(", Isbn13: {0}", Isbn13);
            sb.AppendFormat(", EIsbn: {0}", EIsbn);
            sb.AppendFormat(", SortTitle: {0}", SortTitle);
            sb.AppendFormat(", AlphaKey: {0}", AlphaKey);
            sb.Append("]");

            return sb.ToString();
        }
    }
}