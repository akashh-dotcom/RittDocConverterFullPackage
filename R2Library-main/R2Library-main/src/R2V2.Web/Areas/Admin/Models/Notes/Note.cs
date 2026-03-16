#region

using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Notes
{
    public class Note
    {
        public Note()
        {
        }

        public Note(Core.Institution.Note coreNote)
        {
            Id = coreNote.Id;
            InstitutionId = coreNote.InstitutionId;
            LastUpdated = coreNote.LastUpdated.HasValue
                ? coreNote.LastUpdated.GetValueOrDefault()
                : coreNote.CreationDate;

            CreatedBy = coreNote.CreatedBy;
            if (coreNote.CreatedBy != null && coreNote.CreatedBy.Contains("["))
            {
                var startIndex = coreNote.CreatedBy.IndexOf("[", StringComparison.Ordinal) + 1;
                var lenOfUser = coreNote.CreatedBy.IndexOf("]", StringComparison.Ordinal) - startIndex;
                CreatedBy = coreNote.CreatedBy.Substring(startIndex, lenOfUser);
            }

            UpdatedBy = coreNote.UpdatedBy;
            if (coreNote.UpdatedBy != null && coreNote.UpdatedBy.Contains("["))
            {
                var startIndex = coreNote.UpdatedBy.IndexOf("[", StringComparison.Ordinal) + 1;
                var lenOfUser = coreNote.UpdatedBy.IndexOf("]", StringComparison.Ordinal) - startIndex;
                UpdatedBy = coreNote.UpdatedBy.Substring(startIndex, lenOfUser);
            }

            Comment = coreNote.Comment;

            UserId = coreNote.UserId;
        }

        public int Id { get; set; }
        public int InstitutionId { get; set; }

        [Display(Name = "Note:")]
        [Required]
        [AllowHtml]
        public string Comment { get; set; }

        [Display(Name = "Created By:")] public string CreatedBy { get; set; }

        [Display(Name = "Last Updated By:")] public string UpdatedBy { get; set; }

        [Display(Name = "Last Updated:")]
        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}")]
        public DateTime LastUpdated { get; set; }

        public int UserId { get; set; }
    }
}