#region

using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource
{
    public class EditorAffiliation : Entity
    {
        // -- Table: tEditorAffiliation
        // -- Fields: ea.iEditorAffiliationId, ea.iEditorId, ea.vchJobTitle, ea.vchOrganization, ea.tiAffiliationOrder

        public virtual Editor Editor { get; set; }
        public virtual short Order { get; set; }
        public virtual string JobTitle { get; set; }
        public virtual string Organization { get; set; }
    }
}