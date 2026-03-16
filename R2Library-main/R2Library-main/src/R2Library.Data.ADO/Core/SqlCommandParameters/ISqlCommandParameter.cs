#region

using System.Data.SqlClient;

#endregion

namespace R2Library.Data.ADO.Core.SqlCommandParameters
{
    public interface ISqlCommandParameter
    {
        string Name { get; set; }
        void SetCommandParmater(SqlCommand command);
    }
}