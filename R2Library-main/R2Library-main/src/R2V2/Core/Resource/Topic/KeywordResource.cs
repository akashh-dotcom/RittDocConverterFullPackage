#region

using System.Collections.Generic;
using R2V2.Core.Institution;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource.Topic
{
    public class KeywordResource : AuditableEntity, ISoftDeletable
    {
        public virtual int KeywordId { get; set; }
        public virtual string Isbn { get; set; }
        public virtual string ChapterId { get; set; }
        public virtual string SectionId { get; set; }

        public virtual IList<InstitutionResourceLicense> InstitutionResourceLicenses { get; set; }
        public virtual bool RecordStatus { get; set; }
    }
}