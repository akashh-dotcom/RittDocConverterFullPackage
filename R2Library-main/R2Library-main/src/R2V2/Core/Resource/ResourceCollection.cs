#region

using System.Web.Script.Serialization;
using R2V2.Core.SuperType;
using R2V2.DataAccess;

#endregion

namespace R2V2.Core.Resource
{
    public class ResourceCollection : AuditableEntity, ISoftDeletable
    {
        public virtual int CollectionId { get; set; }
        public virtual Collection.Collection Collection { get; set; }
        public virtual int? ResourceId { get; set; }

        public virtual string DataString { get; set; }

        public virtual ResourceCollectionData Data
        {
            get
            {
                if (DataString == null)
                {
                    return null;
                }

                var js = new JavaScriptSerializer();
                return js.Deserialize<ResourceCollectionData>(DataString);
            }
            set
            {
                if (value == null)
                {
                    DataString = null;
                    return;
                }

                var js = new JavaScriptSerializer();
                DataString = js.Serialize(value);
            }
        }

        public virtual bool RecordStatus { get; set; }
    }
}