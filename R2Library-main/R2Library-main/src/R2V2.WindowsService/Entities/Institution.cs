#region

using System.Data.SqlClient;
using System.Text;
using R2Library.Data.ADO.Core;

#endregion

namespace R2V2.WindowsService.Entities
{
    public class Institution : FactoryBase, IDataEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AccountNumber { get; set; }
        public short AutoAddFreeLicenses { get; set; }
        public int ResourceLicensesAdded { get; set; }

        public void Populate(SqlDataReader reader)
        {
            // iInstitutionId, vchInstitutionName, vchInstitutionAcctNum, tiAutoAddFreeLicenses
            Id = GetInt32Value(reader, "iInstitutionId", -1);
            Name = GetStringValue(reader, "vchInstitutionName");
            AccountNumber = GetStringValue(reader, "vchInstitutionAcctNum");
            AutoAddFreeLicenses = GetByteValue(reader, "tiAutoAddFreeLicenses", 0);
        }

        public string ToDebugString()
        {
            var sb = new StringBuilder();
            sb.Append("Institution = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", Name: {0}", Name);
            sb.AppendFormat(", AccountNumber: {0}", AccountNumber);
            sb.AppendFormat(", AutoAddFreeLicenses: {0}", AutoAddFreeLicenses);
            sb.AppendFormat(", ResourceLicensesAdded: {0}", ResourceLicensesAdded);
            sb.Append("]");
            return sb.ToString();
        }
    }
}