#region

using System;
using System.Data.SqlClient;
using R2Library.Data.ADO.Core;

#endregion

namespace R2Library.Data.ADO.R2
{
    public class TermContent : FactoryBase, IDataEntity
    {
        public string Term { get; set; }
        public string Content { get; set; }
        public string SectionId { get; set; }

        public void Populate(SqlDataReader reader)
        {
            try
            {
                Term = GetStringValue(reader, "Term");
                Content = GetStringValue(reader, "Content");
                SectionId = GetStringValue(reader, "SectionId");
            }
            catch (Exception ex)
            {
                Log.ErrorFormat(ex.Message, ex);
                throw;
            }
        }
    }
}