#region

using System;
using System.Linq;
using R2V2.Core.Resource;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.MyR2
{
    [Serializable]
    public abstract class UserContentItem : AuditableEntity, ISoftDeletable
    {
        public virtual UserContentFolder UserContentFolder { get; set; }
        public virtual string Title { get; set; }
        public virtual string ChapterSectionTitle { get; set; }

        public virtual string ChapterSectionId
        {
            get => $"ResourceID={ResourceId}&SectionID={SectionId}&Library={Library}";
            set
            {
                var chapterSectionId = value;

                foreach (var kvp in chapterSectionId.Split('&').Select(x => x.Split('=')).Where(x => x.Length == 2))
                {
                    switch (kvp[0].ToLower())
                    {
                        case "isbn":
                            Isbn = kvp[1];
                            break;

                        case "resourceid":
                            ResourceId = int.Parse(kvp[1]);
                            break;

                        case "otherid":
                        case "sectionid":
                            SectionId = kvp[1];
                            break;

                        case "library":
                            Library = kvp[1];
                            break;

                        case "type":
                            TypeId = kvp[1];
                            break;
                    }
                }
            }
        }

        public virtual int ResourceId { get; set; }
        public virtual IResource Resource { get; set; }
        public virtual string Isbn { get; set; }
        public virtual string SectionId { get; set; }
        public virtual string Library { get; set; }
        public virtual string TypeId { get; set; }
        public virtual string Filename { get; set; }

        public virtual string ImageUrl => $"/{Isbn}/{Filename}";
        public virtual bool RecordStatus { get; set; }
    }
}