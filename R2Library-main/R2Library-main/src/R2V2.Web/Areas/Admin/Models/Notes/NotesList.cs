#region

using System.Collections.Generic;
using R2V2.Core.Admin;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Notes
{
    public class NotesList : AdminBaseModel
    {
        public NotesList(IAdminInstitution institution, IEnumerable<Note> notes)
            : base(institution)
        {
            Notes = notes;
        }

        public IEnumerable<Note> Notes { get; set; }
    }
}