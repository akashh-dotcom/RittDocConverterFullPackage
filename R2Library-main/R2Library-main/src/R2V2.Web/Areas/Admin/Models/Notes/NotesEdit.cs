#region

using R2V2.Core.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Notes
{
    public class NotesEdit : AdminBaseModel
    {
        public NotesEdit(IAdminInstitution institution, Core.Institution.Note coreNote) : base(institution)
        {
            Note = new Note(coreNote);
        }

        public Note Note { get; set; }
    }
}