#region

using R2V2.Core.Email;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class InstitutionResourceEmailMap : BaseMap<InstitutionResourceEmail>
    {
        public InstitutionResourceEmailMap()
        {
            Table("tInstitutionResourceEmail");

            Id(x => x.Id).Column("iInstitutionResourceEmailId").GeneratedBy.Identity();
            Map(x => x.InstitutionId).Column("iInstitutionId");
            Map(x => x.ResourceId).Column("iResourceId");
            Map(x => x.UserId).Column("iUserId");
            Map(x => x.ChapterSectionId).Column("vchChapterSectionId");
            Map(x => x.UserEmailAddress).Column("vchUserEmailAddress");
            Map(x => x.Subject).Column("vchSubject");
            Map(x => x.Comment).Column("vchComments");
            Map(x => x.SessionId).Column("vchSessionId");
            Map(x => x.RequestId).Column("vchRequestId");
            Map(x => x.Queued).Column("bQueued");
            Map(x => x.CreationDate).Column("dtCreationDate");

            HasMany(x => x.Recipients).KeyColumn("iInstitutionResourceEmailId").AsBag().Inverse().Cascade
                .AllDeleteOrphan();
        }
    }
}