#region

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using R2Library.Data.ADO.Core;
using R2Library.Data.ADO.Core.SqlCommandParameters;

#endregion

namespace R2Utilities.DataAccess.Terms
{
    public abstract class TermResource : FactoryBase, IDataEntity, IEquatable<TermResource>
    {
        #region Methods

        public virtual void Populate(SqlDataReader reader)
        {
            try
            {
                ResourceIsbn = GetStringValue(reader, "vchResourceISBN");
                ChapterId = GetStringValue(reader, "vchChapterId");
                SectionId = GetStringValue(reader, "vchSectionId");
                CreatorId = GetStringValue(reader, "vchCreatorId");
                CreationDate = GetDateValue(reader, "dtCreationDate");
                UpdaterId = GetStringValue(reader, "vchUpdaterId");
                LastUpdate = GetDateValue(reader, "dtLastUpdate");
                RecordStatus = GetByteValue(reader, "tiRecordStatus", 0);
                Title = GetStringValue(reader, "vchTitle");
            }
            catch (Exception ex)
            {
                Log.ErrorFormat(ex.Message, ex);
                throw;
            }
        }

        public virtual IEnumerable<ISqlCommandParameter> ToParameters(int x)
        {
            return new List<ISqlCommandParameter>
            {
                new StringParameter($"ResourceISBN_{x}", ResourceIsbn),
                new StringParameter($"ChapterId_{x}", ChapterId),
                new StringParameter($"SectionId_{x}", SectionId),
                new StringParameter($"CreatorId_{x}", CreatorId),
                /*new DateTimeParameter(String.Format("CreationDate_{0}", x), CreationDate),
                new StringParameter(String.Format("UpdaterId_{0}", x), UpdaterId),
                new DateTimeParameter(String.Format("LastUpdate_{0}", x), LastUpdate),
                new Int32Parameter(String.Format("RecordStatus_{0}", x), RecordStatus),*/
                new StringParameter($"Title_{x}", Title)
            };
        }

        public bool Equals(TermResource other)
        {
            return TermId == other.TermId && GetType() == other.GetType() && SectionId == other.SectionId;
        }

        public override int GetHashCode()
        {
            var hashTermId = TermId.GetHashCode();
            var hashType = GetType().GetHashCode();
            var hashSectionId = SectionId.GetHashCode();

            return hashTermId ^ hashType ^ hashSectionId;
        }

        #endregion Methods

        #region Properties

        public int Id { get; set; }

        public int TermId { get; set; }

        //public string Term { get; set; }
        public string ResourceIsbn { get; set; }
        public string ChapterId { get; set; }
        public string SectionId { get; set; }
        public string CreatorId { get; set; }
        public DateTime CreationDate { get; set; }
        public string UpdaterId { get; set; }
        public DateTime LastUpdate { get; set; }
        public short RecordStatus { get; set; }
        public string Title { get; set; }

        public abstract string SqlInsert { get; }
        public abstract string SqlInactivate { get; }

        #endregion Properties
    }
}