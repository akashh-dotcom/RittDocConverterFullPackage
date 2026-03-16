#region

using System.Data.SqlClient;

#endregion

namespace R2Library.Data.ADO.Core
{
    public interface IDataEntity
    {
        void Populate(SqlDataReader reader);
    }
}