#region

using R2V2.Core.Institution;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class NoteMap : BaseMap<Note>
    {
        public NoteMap()
        {
            Table("tInstitutionComment");

            Id(x => x.Id).Column("iInstitutionCommentID").GeneratedBy.Identity();
            Map(x => x.UserId).Column("iCreatorUserId");
            Map(x => x.InstitutionId).Column("iInstitutionId");
            Map(x => x.Comment).Column("vchComment");
        }
    }
}