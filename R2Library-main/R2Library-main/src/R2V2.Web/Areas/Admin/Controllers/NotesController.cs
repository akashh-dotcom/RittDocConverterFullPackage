#region

using System;
using System.Linq;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Infrastructure.UnitOfWork;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Areas.Admin.Models.Notes;
using Note = R2V2.Web.Areas.Admin.Models.Notes.Note;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    public class NotesController : R2AdminBaseController
    {
        private readonly IAdminContext _adminContext;
        private readonly IQueryable<Core.Institution.Note> _notes;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public NotesController(IAuthenticationContext authenticationContext
            , IAdminContext adminContext
            , IQueryable<Core.Institution.Note> notes
            , IUnitOfWorkProvider unitOfWorkProvider
        )
            : base(authenticationContext)
        {
            _adminContext = adminContext;
            _notes = notes;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        public ActionResult List(int institutionId)
        {
            var adminInstitution = _adminContext.GetAdminInstitution(institutionId);

            var institutionNotes = _notes.Where(x => x.InstitutionId == adminInstitution.Id);

            var notes = institutionNotes.Select(institutionNote => new Note(institutionNote));

            return View(new NotesList(adminInstitution, notes));
        }

        public ActionResult Edit(int institutionId, int id)
        {
            var adminInstitution = _adminContext.GetAdminInstitution(institutionId);

            var institutionNote = _notes.FirstOrDefault(x => x.InstitutionId == adminInstitution.Id && x.Id == id);
            if (institutionNote == null)
            {
                institutionNote = new Core.Institution.Note { InstitutionId = adminInstitution.Id };

                if (AuthenticatedInstitution == null || AuthenticatedInstitution.User == null)
                {
                    return RedirectToAction("List", new { institutionId = adminInstitution.Id });
                }

                institutionNote.UserId = AuthenticatedInstitution.User.Id;
                institutionNote.LastUpdated = DateTime.Now;
            }

            return View(new NotesEdit(adminInstitution, institutionNote));
        }

        [HttpPost]
        public ActionResult Edit(Note note)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var adminInstitution = _adminContext.GetAdminInstitution(note.InstitutionId);

                    var institutionNote =
                        _notes.FirstOrDefault(x => x.InstitutionId == adminInstitution.Id && x.Id == note.Id) ??
                        new Core.Institution.Note();
                    if (institutionNote.UserId == 0)
                    {
                        institutionNote.InstitutionId = adminInstitution.Id;

                        if (AuthenticatedInstitution != null && AuthenticatedInstitution.User != null)
                        {
                            institutionNote.UserId = AuthenticatedInstitution.User.Id;
                        }
                    }

                    var noteToSave = note.ToCoreNote(institutionNote);
                    if (noteToSave != null)
                    {
                        uow.SaveOrUpdate(noteToSave);
                        uow.Commit();
                        transaction.Commit();
                        return RedirectToAction("List", new { note.InstitutionId });
                    }

                    transaction.Rollback();
                    ModelState.AddModelError("Note.Comment",
                        "There was an Error saving the Note. Please try again later.");
                    return View(new NotesEdit(adminInstitution, institutionNote));
                }
            }
        }


        public ActionResult Delete(int institutionId, int id)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var adminInstitution = _adminContext.GetAdminInstitution(institutionId);

                    var institutionNote =
                        _notes.FirstOrDefault(x => x.InstitutionId == adminInstitution.Id && x.Id == id);
                    if (institutionNote != null)
                    {
                        uow.Delete(institutionNote);
                        uow.Commit();
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }
                }
            }

            return RedirectToAction("List", new { InstitutionId = institutionId });
        }
    }
}