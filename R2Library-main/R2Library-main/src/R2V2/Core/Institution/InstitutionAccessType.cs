#region

using System;

#endregion

namespace R2V2.Core.Institution
{
    [Serializable]
    public class InstitutionAccessType : IInstitutionAccessType
    {
        public static InstitutionAccessType IpValidationAnon =>
            new InstitutionAccessType
            {
                Id = AccessType.IpValidationAnon,
                Description = "IP Validation Anon",
                LongDescription = "IP Validation Anonymous-Only"
            };

        public static InstitutionAccessType IpValidationOpt =>
            new InstitutionAccessType
            {
                Id = AccessType.IpValidationOpt,
                Description = "IP Validation, Ind Opt",
                LongDescription = "IP Validation, Individual Accounts Optional"
            };

        public static InstitutionAccessType IpValidationReq =>
            new InstitutionAccessType
            {
                Id = AccessType.IpValidationReq,
                Description = "IP Validation, Ind Req",
                LongDescription = "IP Validation, Individual Account Required"
            };

        public static InstitutionAccessType IpIndependent =>
            new InstitutionAccessType
            {
                Id = AccessType.IpIndependent,
                Description = "IP Independent",
                LongDescription = "IP Independent Account for Individuals"
            };

        public AccessType Id { get; protected set; }
        public string Description { get; protected set; }
        public string LongDescription { get; protected set; }
    }
}