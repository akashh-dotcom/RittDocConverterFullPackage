#region

using R2V2.Core.Institution;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Marketing
{
    public class AutomatedCartInstitution : MarketingInstitutionBase
    {
        public AutomatedCartInstitution(IInstitution institution) : base(institution)
        {
        }

        public bool NewEdition { get; set; }
        public bool TriggeredPda { get; set; }
        public bool Reviewed { get; set; }
        public bool Turnaway { get; set; }
        public bool Requested { get; set; }
    }
}