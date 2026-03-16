#region

using System;
using System.Data.SqlClient;
using R2Library.Data.ADO.Core;

#endregion

namespace R2Library.Data.ADO.R2Utility
{
    public class TermHighlightQueue : FactoryBase, IDataEntity
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public int ResourceId { get; set; }
        public string Isbn { get; set; }
        public string TermHighlightStatus { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateStarted { get; set; }
        public DateTime? DateFinished { get; set; }
        public int FirstDocumentId { get; set; }
        public int LastDocumentId { get; set; }
        public string StatusMessage { get; set; }

        public TermHighlightType TermHighlightType { get; set; }

        public void Populate(SqlDataReader reader)
        {
            try
            {
                Id = GetInt32Value(reader, "termHighlightQueueId", -1);
                JobId = GetInt32Value(reader, "jobId", -1);
                ResourceId = GetInt32Value(reader, "resourceId", -1);
                Isbn = GetStringValue(reader, "isbn");
                TermHighlightStatus = GetStringValue(reader, "termHighlightStatus");
                StatusMessage = GetStringValue(reader, "statusMessage");

                FirstDocumentId = GetInt32Value(reader, "firstDocumentId", -1);
                LastDocumentId = GetInt32Value(reader, "lastDocumentId", -1);
                DateAdded = GetDateValue(reader, "dateAdded");
                DateStarted = GetDateValueOrNull(reader, "dateStarted");
                DateFinished = GetDateValueOrNull(reader, "dateFinished");

                TermHighlightType = GetEnumValue<TermHighlightType>(reader, "termHighlightType");
            }
            catch (Exception ex)
            {
                Log.ErrorFormat(ex.Message, ex);
                throw;
            }
        }
    }
}

public enum TermHighlightType
{
    Tabers,
    IndexTerms
}