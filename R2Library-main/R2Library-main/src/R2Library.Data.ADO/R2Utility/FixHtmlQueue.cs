#region

using System;
using System.Data.SqlClient;
using R2Library.Data.ADO.Core;

#endregion

namespace R2Library.Data.ADO.R2Utility
{
    public class FixHtmlQueue : FactoryBase, IDataEntity
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public string Isbn { get; set; }
        public string Status { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateStarted { get; set; }
        public DateTime? DateFinished { get; set; }
        public string StatusMessage { get; set; }

        public void Populate(SqlDataReader reader)
        {
            try
            {
                Id = GetInt32Value(reader, "fixHtmlQueueId", -1);
                ResourceId = GetInt32Value(reader, "resourceId", -1);
                Isbn = GetStringValue(reader, "isbn");
                Status = GetStringValue(reader, "status");
                StatusMessage = GetStringValue(reader, "statusMessage");

                DateAdded = GetDateValue(reader, "dateAdded");
                DateStarted = GetDateValueOrNull(reader, "dateStarted");
                DateFinished = GetDateValueOrNull(reader, "dateFinished");
            }
            catch (Exception ex)
            {
                Log.ErrorFormat(ex.Message, ex);
                throw;
            }
        }
    }
}