#region

using System.Collections.Generic;
using R2V2.Core.Institution;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource.Topic
{
    public class AZIndex : Entity
    {
        public virtual string Name { get; set; }
        public virtual AZIndexTypeEnum Type { get; set; }
        public virtual string AlphaKey { get; set; }
        public virtual int ResourceId { get; set; }
        public virtual string Isbn { get; set; }
        public virtual string ChapterId { get; set; }
        public virtual string SectionId { get; set; }

        public virtual IList<ResourcePracticeArea> ResourcePracticeAreas { get; set; }
        public virtual IList<ResourceSpecialty> ResourceSpecialties { get; set; }
        public virtual IList<InstitutionResourceLicense> InstitutionResourceLicenses { get; set; }
    }
}