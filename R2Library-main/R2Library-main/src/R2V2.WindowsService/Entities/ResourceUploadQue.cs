#region

using System;
using System.Data.SqlClient;
using System.Text;
using R2Library.Data.ADO.Core;

#endregion

namespace R2V2.WindowsService.Entities
{
    public class ResourceUploadQue : FactoryBase, IDataEntity
    {
        // iResourceUploadQueId, iResourceId, vchResourceISBN, vchResponseEmailId, tiProcessed, iFinalStatus, vchFinalMessage,
        // vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public string Isbn { get; set; }
        public string Email { get; set; }
        public short Processed { get; set; }
        public int FinalStatus { get; set; }
        public string FinalMessage { get; set; }

        public string CreatorId { get; set; }
        public DateTime CreationDate { get; set; }
        public string UpdaterId { get; set; }
        public DateTime? LastUpdate { get; set; }
        public short RecordStatus { get; set; }


        public void Populate(SqlDataReader reader)
        {
            // iResourceUploadQueId, iResourceId, vchResourceISBN, vchResponseEmailId, tiProcessed, iFinalStatus, vchFinalMessage,
            // vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus
            Id = GetInt32Value(reader, "iResourceUploadQueId", -1);
            ResourceId = GetInt32Value(reader, "resourceId", -1);
            FinalStatus = GetInt32Value(reader, "iFinalStatus", -1);
            Isbn = GetStringValue(reader, "vchResourceISBN");
            Email = GetStringValue(reader, "vchResponseEmailId");
            FinalMessage = GetStringValue(reader, "vchFinalMessage");
            CreatorId = GetStringValue(reader, "vchCreatorId");
            UpdaterId = GetStringValue(reader, "vchUpdaterId");
            Processed = GetByteValue(reader, "tiProcessed", 0);
            RecordStatus = GetByteValue(reader, "tiRecordStatus", 0);
            CreationDate = GetDateValue(reader, "dtCreationDate");
            LastUpdate = GetDateValueOrNull(reader, "dtLastUpdate");
        }

        public string ToDebugString()
        {
            var sb = new StringBuilder();
            sb.Append("Promote = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", ResourceId: {0}", ResourceId);
            sb.AppendFormat(", Isbn: {0}", Isbn);
            sb.AppendFormat(", Email: {0}", Email);
            sb.AppendFormat(", Processed: {0}", Processed);
            sb.AppendFormat(", FinalStatus: {0}", FinalStatus);
            sb.AppendFormat(", FinalMessage: {0}", FinalMessage);
            sb.AppendFormat(", CreatorId: {0}", CreatorId);
            sb.AppendFormat(", CreationDate: {0}", CreationDate);
            sb.AppendFormat(", UpdaterId: {0}", UpdaterId);
            sb.AppendFormat(", LastUpdate: {0}", LastUpdate == null ? "null" : LastUpdate.ToString());
            sb.AppendFormat(", RecordStatus: {0}", RecordStatus);
            sb.Append("]");
            return sb.ToString();
        }
    }
}