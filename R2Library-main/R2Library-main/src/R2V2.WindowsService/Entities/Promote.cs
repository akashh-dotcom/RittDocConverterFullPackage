#region

using System.Data.SqlClient;
using System.Text;
using R2Library.Data.ADO.Core;

#endregion

namespace R2V2.WindowsService.Entities
{
    public class Promote : FactoryBase, IDataEntity
    {
        public string Isbn { get; set; }
        public int ResourceId { get; set; }
        public int PublisherId { get; set; }
        public int SourceResourceId { get; set; }

        public string TableName { get; set; }
        public string ScriptAction { get; set; }
        public int RowsAffected { get; set; }

        public void Populate(SqlDataReader reader)
        {
            //isbn varchar(20), resourceId int, publisherId int, sourceResourceId int, tableName varchar(50), scriptAction varchar(1024), rowsAffected int
            Isbn = GetStringValue(reader, "isbn");
            ResourceId = GetInt32Value(reader, "resourceId", -1);
            PublisherId = GetInt32Value(reader, "publisherId", -1);
            SourceResourceId = GetInt32Value(reader, "sourceResourceId", -1);
            TableName = GetStringValue(reader, "tableName");
            ScriptAction = GetStringValue(reader, "scriptAction");
            RowsAffected = GetInt32Value(reader, "rowsAffected", -1);
        }

        public string ToDebugString()
        {
            var sb = new StringBuilder();
            sb.Append("Promote = [");
            sb.AppendFormat("Isbn: {0}", Isbn);
            sb.AppendFormat(", ResourceId: {0}", ResourceId);
            sb.AppendFormat(", PublisherId: {0}", PublisherId);
            sb.AppendFormat(", SourceResourceId: {0}", SourceResourceId);
            sb.AppendFormat(", TableName: {0}", TableName);
            sb.AppendFormat(", ScriptAction: {0}", ScriptAction);
            sb.AppendFormat(", RowsAffected: {0}", RowsAffected);
            sb.Append("]");
            return sb.ToString();
        }
    }
}