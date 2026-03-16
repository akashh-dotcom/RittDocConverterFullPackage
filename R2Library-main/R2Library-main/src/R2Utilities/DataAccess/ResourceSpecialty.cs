#region

using System;
using System.Data.SqlClient;
using R2Library.Data.ADO.Core;

#endregion

namespace R2Utilities.DataAccess
{
    public class ResourceSpecialty : FactoryBase, IDataEntity
    {
        public virtual int ResourceId { get; set; }

        public virtual int Id { get; set; }

        //public virtual Specialty Specialty { get; set; }
        public virtual int RecordStatus { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public void Populate(SqlDataReader reader)
        {
            try
            {
                // s.iSpecialtyId, s.vchSpecialtyCode, s.vchSpecialtyName, s.tiRecordStatus
                Id = GetInt32Value(reader, "iSpecialtyId", -1);
                Code = GetStringValue(reader, "vchSpecialtyCode");
                Name = GetStringValue(reader, "vchSpecialtyName");
                RecordStatus = GetByteValue(reader, "tiRecordStatus", 0);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat(ex.Message, ex);
                throw;
            }
        }
    }
}