#region

using System;
using System.Data.SqlClient;
using R2Library.Data.ADO.Core;

#endregion

namespace R2Utilities.DataAccess
{
    public class ResourcePublisher : FactoryBase, IDataEntity
    {
        public virtual int Id { get; set; }
        public virtual string PublisherName { get; set; }
        public virtual int? ParentPublisherId { get; set; }

        public void Populate(SqlDataReader reader)
        {
            try
            {
                // publisher.iPublisherId, publisher.vchPublisherName
                Id = GetInt32Value(reader, "iPublisherId", -1);
                PublisherName = GetStringValue(reader, "vchPublisherName");
                ParentPublisherId = GetInt32Value(reader, "iConsolidatedPublisherId");
            }
            catch (Exception ex)
            {
                Log.ErrorFormat(ex.Message, ex);
                throw;
            }
        }
    }
}