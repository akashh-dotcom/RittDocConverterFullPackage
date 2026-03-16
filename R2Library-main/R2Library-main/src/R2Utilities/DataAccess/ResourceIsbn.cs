#region

using System;
using System.Data.SqlClient;
using R2Library.Data.ADO.Core;

#endregion

namespace R2Utilities.DataAccess
{
    public class ResourceIsbn : FactoryBase, IDataEntity
    {
        public int TypeId { get; set; }
        public string Isbn { get; set; }

        public void Populate(SqlDataReader reader)
        {
            try
            {
                // iResourceIsbnTypeId, vchIsbn
                Isbn = GetStringValue(reader, "vchIsbn");
                TypeId = GetByteValue(reader, "iResourceIsbnTypeId", 0);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat(ex.Message, ex);
                throw;
            }
        }
    }
}