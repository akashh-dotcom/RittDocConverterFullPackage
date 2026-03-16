#region

using System.Collections.Generic;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource
{
    public class Editor : Entity
    {
        // -- Table: tEditor
        // -- Fields: e.iEditorId, e.iResourceId, e.vchFirstName, e.vchLastName, e.vchMiddleName, e.vchLineage, e.vchDegree, e.tiEditorOrder

        public virtual int ResourceId { get; set; }
        public virtual short Order { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string MiddleName { get; set; }
        public virtual string Lineage { get; set; }
        public virtual string Degrees { get; set; }

        public virtual IList<EditorAffiliation> Affiliations { get; set; }
    }
}