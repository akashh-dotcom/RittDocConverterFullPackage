#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Authentication;
using R2V2.Web.Helpers;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Alerts
{
    [Serializable]
    public class AdministratorAlert : AdminBaseModel
    {
        private int _rowNumber;

        public AdministratorAlert()
        {
            PopulateDropDownLists();
        }

        public AdministratorAlert(IAdminAlert alert, string imageLocation, AuthenticatedInstitution institution,
            IResource resource)
        {
            Resource = resource;
            InstitutionId = institution.Id;

            PopulateAlert(alert, imageLocation);
            SetPurchasable(institution);
        }

        public AdministratorAlert(IAdminAlert alert, string imageLocation, IResource resource)
        {
            Resource = resource;
            PopulateDropDownLists();

            PopulateAlert(alert, imageLocation);
        }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal DiscountPrice { get; set; }

        [Display(Name = @"Resource:")] public IResource Resource { get; set; }

        public int? ResourceId { get; set; }
        public int AlertId { get; set; }

        public bool RecordStatus { get; set; }

        [Display(Name = "Only Display Once:")] public bool DisplayOnce { get; set; }

        [Display(Name = "Created By:")] public string CreatedBy { get; set; }

        [Display(Name = "Last Updated By:")] public string UpdatedBy { get; set; }

        [Display(Name = "Last Updated:")]
        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}")]
        public DateTime LastUpdated { get; set; }

        [Display(Name = "Last Updated By:")] public string LastUpdatedBy { get; set; }

        [Display(Name = "Title:")] public string Title { get; set; }

        [Display(Name = "Text:")] [AllowHtml] public string Text { get; set; }

        [Display(Name = "Layout:")] [Required] public AlertLayout AlertLayout { get; set; }

        [Display(Name = "Image:")] public Dictionary<int, string> ImageUrls { get; set; }

        public IList<AlertImage> AlertImages { get; set; }

        public bool AlertImageExists { get; set; }

        [Display(Name = "Alert Name:")]
        [Required]
        public string AlertName { get; set; }

        [Required]
        [Display(Name = "Start Date:")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ConvertEmptyStringToNull = true,
            NullDisplayText = "Not Set", ApplyFormatInEditMode = true)]
        [DateTenYears("StartDate")]
        public DateTime? StartDate { get; set; }

        [Required]
        [Display(Name = "End Date:")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ConvertEmptyStringToNull = true,
            ApplyFormatInEditMode = true)]
        [DateTenYears("EndDate")]
        public DateTime? EndDate { get; set; }

        public Role Role { get; set; }

        [Display(Name = "User Role")] public SelectList RoleSelectList { get; set; }

        [Display(Name = "Allow Purchase")] public bool DisplayPurchase { get; set; }

        [Display(Name = "Allow PDA (Open Access Resources cannot be put on PDA)")]
        public bool DisplayPDA { get; set; }

        public bool CanAddToCollection { get; set; }

        public string ResourceFilter { get; set; }

        public string DisplayOnceString()
        {
            return DisplayOnce ? "Yes" : "No";
        }

        public string SingleImageUrl()
        {
            if (ImageUrls != null)
            {
                var firstImage = ImageUrls.FirstOrDefault();
                if (firstImage.Key > 0)
                {
                    return firstImage.Value;
                }
            }

            return null;
        }


        private void SetPurchasable(AuthenticatedInstitution institution)
        {
            if (
                institution.IsSalesAssociate() ||
                institution.IsInstitutionAdmin() ||
                institution.IsRittenhouseAdmin() ||
                institution.IsPublisherUser()
            )
            {
                CanAddToCollection = true;
                if (Resource != null)
                {
                    var pdaLicense = institution.Licenses.FirstOrDefault(x =>
                        x.LicenseType == LicenseType.Pda && x.ResourceId == ResourceId);
                    if (pdaLicense != null)
                    {
                        DisplayPDA = false;
                    }
                }
            }
            else
            {
                CanAddToCollection = false;
            }
        }

        private void PopulateAlert(IAdminAlert alert, string imageLocation)
        {
            AlertId = alert.Id;
            DisplayOnce = alert.DisplayOnce;
            CreatedBy = alert.CreatedBy;
            UpdatedBy = alert.UpdatedBy;
            LastUpdated = alert.LastUpdated == null
                ? alert.CreationDate
                : alert.LastUpdated.GetValueOrDefault(DateTime.Now);
            Title = alert.Title;
            Text = alert.Text;
            AlertLayout = alert.Layout;

            AlertName = alert.AlertName;
            StartDate = alert.StartDate;
            EndDate = alert.EndDate;

            Role = alert.Role;

            LastUpdatedBy = string.IsNullOrWhiteSpace(UpdatedBy) ? alert.CreatedBy : alert.UpdatedBy;

            if (alert.AlertImages != null)
            {
                foreach (var alertImage in alert.AlertImages)
                {
                    AddAlertImage(alertImage, imageLocation);
                }
            }

            if (alert.ResourceId.HasValue)
            {
                ResourceId = alert.ResourceId.Value;
            }

            PopulateResource(alert);
        }

        private void AddAlertImage(AlertImage alertImage, string imageLocation)
        {
            if (alertImage.RecordStatus)
            {
                if (ImageUrls == null)
                {
                    ImageUrls = new Dictionary<int, string>();
                }

                //Path.Combine(imageLocation, alertImage.ImageFileName)
                //http://dev-images.r2library.com/alerts
                string imageUrl;
                if (imageLocation.Last() == '\\' || imageLocation.Last() == '/')
                {
                    imageUrl = $"{imageLocation}{alertImage.ImageFileName}";
                }
                else
                {
                    imageUrl = string.Format(imageLocation.Contains('\\') ? "{0}\\{1}" : "{0}/{1}", imageLocation,
                        alertImage.ImageFileName);
                }

                ImageUrls.Add(alertImage.Id, imageUrl);
                AlertImageExists = true;
            }
        }

        public void PopulateImageUrls(Dictionary<int, string> idsAndFilenames, string imageLocation)
        {
            if (ImageUrls == null)
            {
                ImageUrls = new Dictionary<int, string>();
            }

            if (idsAndFilenames != null)
            {
                foreach (var idAndFilename in idsAndFilenames)
                {
                    string imageUrl;
                    if (imageLocation.Last() == '\\' || imageLocation.Last() == '/')
                    {
                        imageUrl = $"{imageLocation}{idAndFilename.Value}";
                    }
                    else
                    {
                        imageUrl = string.Format(imageLocation.Contains('\\') ? "{0}\\{1}" : "{0}/{1}", imageLocation,
                            idAndFilename.Value);
                    }

                    ImageUrls.Add(idAndFilename.Key, imageUrl);
                }
            }

            if (ImageUrls.Count > 0)
            {
                AlertImageExists = true;
            }
        }

        public void PopulateDropDownLists()
        {
            var userRoles = new List<UserRole>
            {
                UserRole.User,
                UserRole.SalesAssociate,
                UserRole.PublisherUser,
                UserRole.InstitutionAdministrator,
                UserRole.RittenhouseAdministrator
            };
            RoleSelectList = new SelectList(userRoles, "Id", "Description");
        }

        private void PopulateResource(IAdminAlert alert)
        {
            if (Resource != null)
            {
                ResourceFilter = $"{Resource.Title} ({Resource.Isbn}-Edition: {Resource.Edition})";
            }

            if (alert != null)
            {
                DisplayPurchase = alert.AllowPurchase;
                if (Resource != null && !Resource.IsFreeResource)
                {
                    DisplayPDA = alert.AllowPDA;
                }
            }
        }

        public void SetItemId(int rowNumber)
        {
            _rowNumber = rowNumber;
        }

        public string GetHtmlId()
        {
            return $"item-{_rowNumber}";
        }
    }
}