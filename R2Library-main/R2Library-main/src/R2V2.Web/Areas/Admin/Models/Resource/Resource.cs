#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Publisher;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Resource
{
    public class Resource : IAdminResource
    {
        /// <summary>
        ///     Default constructor
        /// </summary>
        public Resource()
        {
        }

        public Resource(IResource resource)
        {
            if (resource == null)
            {
                return;
            }

            SetResource(resource, null);
        }

        /// <param name="featuredTitle"> </param>
        public Resource(IResource resource, IFeaturedTitle featuredTitle, string specialText, string specialIconName)
        {
            if (resource == null)
            {
                return;
            }

            SetResource(resource, featuredTitle);

            if (specialText != null)
            {
                SpecialText = specialText;
                SpecialIconName = specialIconName;
            }
        }

        public Resource(IResource resource, IFeaturedTitle featuredTitle, ICartItem cartItem)
        {
            if (resource == null)
            {
                return;
            }

            SetResource(resource, featuredTitle);

            if (cartItem != null)
            {
                SpecialText = cartItem.SpecialText;
                SpecialIconName = cartItem.SpecialIconName;
            }
        }

        public Resource(IResource resource, IFeaturedTitle featuredTitle, CartItem cartItem)
        {
            if (resource == null)
            {
                return;
            }

            SetResource(resource, featuredTitle);

            if (cartItem != null)
            {
                SpecialText = cartItem.SpecialText;
                SpecialIconName = cartItem.SpecialIconName;
            }
        }

        [Display(Name = @"3 Bundle Price: ")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal? BundlePrice3 { get; set; }

        public string ConsolidatedPublisherName { get; set; }

        public string PublisherProductStatement { get; set; }

        [Display(Name = @"Collections: ")] public IEnumerable<ICollection> Collections { get; set; }

        [Display(Name = @"Resource has Video: ")]
        public byte ContainsVideo { get; set; }

        [Display(Name = @"Archive Date: ")]
        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}", NullDisplayText = "Before 11/17/2016")]
        public DateTime? ArchiveDate { get; private set; }

        public bool AffiliationUpdatedByPrelude { get; set; }

        [Display(Name = @"Free Resource: ")] public bool IsFreeResource { get; set; }

        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? NotSaleableDate { get; set; }

        [Display(Name = @"Not Saleable: ")] public bool NotSaleable { get; set; }

        [Display(Name = @"Exclude from Auto Archive: ")]
        public bool ExcludeFromAutoArchive { get; set; }

        [Display(Name = @"Promotion Queue: ")] public string PromotionQueueStatus { get; set; }

        [Display(Name = @"Doody Star Rating: ")]
        public int DoodyRating { get; set; }

        public int Id { get; set; }

        [Display(Name = @"R2 Release Date: ")]
        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}", ConvertEmptyStringToNull = true, NullDisplayText = "N/A",
            ApplyFormatInEditMode = true)]
        //[DateTenYears("Resource.ReleaseDate")]
        public DateTime? ReleaseDate { get; set; }

        //[Required]
        [Display(Name = @"Resource Title: ")] public string Title { get; set; }

        //[Required]
        [Display(Name = @"Author(s): ")] public string Authors { get; set; }

        public int? PublicationDateYear { get; set; }

        [Display(Name = @"Publication Date: ")]
        [DisplayFormat(DataFormatString = "{0:yyyy}", ConvertEmptyStringToNull = true, NullDisplayText = "N/A",
            ApplyFormatInEditMode = true)]
        public DateTime? PublicationDate { get; set; }

        [Display(Name = @"Doody Review: ")] public short DoodyReview { get; set; }

        public int StatusId { get; set; }

        public string Status
        {
            get
            {
                switch ((ResourceStatus)StatusId)
                {
                    case ResourceStatus.Active:
                        return "Active";
                    case ResourceStatus.Archived:
                        return "Archived";
                    case ResourceStatus.Forthcoming:
                        return "Pre-Order";
                    case ResourceStatus.Inactive:
                        return "Not Available";
                    default:
                        return "";
                }
            }
        }

        [Required(ErrorMessage =
            @"Price is required. If this is a free resource please enter 0 and check the 'Free Resource' box.")]
        [Display(Name = @"Concurrent User Price: ")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal ListPrice { get; set; }

        public bool IsForSale { get; set; }

        [Display(Name = @"Due Date: ")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "N/A")]
        public string DueDate { get; set; }

        [Display(Name = @"Publisher: ")] public string PublisherName { get; set; }

        [Display(Name = @"Publisher: ")] public int PublisherId { get; set; }

        public string ImageFileName { get; set; }

        [Display(Name = @"ISBN: ")] public string Isbn { get; set; }

        [Display(Name = @"Discipline: ")] public IEnumerable<ISpecialty> Specialties { get; set; }

        [Display(Name = @"Practice Area: ")] public IEnumerable<IPracticeArea> PracticeAreas { get; set; }

        [AllowHtml]
        [Required]
        [Display(Name = @"Description: ")]
        public string ResourceDescription { get; set; }

        [Display(Name = @"Edition: ")] public string Edition { get; set; }

        [Display(Name = @"ISBN 10: ")]
        [StringLength(10, ErrorMessage = @"ISBN 10 must be 10 characters, no dashes.", MinimumLength = 10)]
        public string Isbn10 { get; set; }

        [Display(Name = @"ISBN 13: ")]
        [StringLength(13, ErrorMessage = @"ISBN 13 must be 10 characters, no dashes.", MinimumLength = 13)]
        public string Isbn13 { get; set; }

        [Display(Name = @"eISBN: ")] public string EIsbn { get; set; }

        [Display(Name = @"Secondary Contributors: ")]
        public string AdditionalContributors { get; set; }

        [Display(Name = @"Subtitle: ")] public string Subtitle { get; set; }

        [Display(Name = @"Drug Monograph: ")] public short IsDrugMonograph { get; set; }

        [Display(Name = @"NLM Call Number: ")] public string NlmCall { get; set; }

        public string ImageUrl { get; set; }

        [Display(Name = @"Featured Title: ")] public bool IsFeaturedTitle { get; set; }

        public int FeaturedTitleId { get; set; }

        [Display(Name = @"Start Date: ")]
        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}", ApplyFormatInEditMode = true)]
        //[DateTenYears("Resource.FeaturedTitleStartDate")]
        public DateTime? FeaturedTitleStartDate { get; set; }

        [Display(Name = @"End Date: ")]
        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}", ApplyFormatInEditMode = true)]
        //[DateTenYears("Resource.FeaturedTitleEndDate")]
        public DateTime? FeaturedTitleEndDate { get; set; }

        [Display(Name = @"Last Promotion Date: ")]
        [DisplayFormat(DataFormatString = "{0:G}", ApplyFormatInEditMode = true)]
        public DateTime? LastPromotionDate { get; set; }

        [Display(Name = @"QA Approval Date: ")]
        [DisplayFormat(DataFormatString = "{0:G}", ApplyFormatInEditMode = true)]
        public DateTime? QaApprovalDate { get; set; }

        [Display(Name = @"QA Approval: ")] public bool QaApproval { get; set; }

        [Display(Name = @"Page Count: ")] public string PageCount { get; set; }

        public string FeaturedTitleStatus()
        {
            if (FeaturedTitleStartDate < DateTime.Now && FeaturedTitleEndDate > DateTime.Now)
            {
                return "Featured Title";
            }

            return "Expired Featured Title";
        }

        [Display(Name = @"Primary Author Affiliation: ")]
        public string Affiliation { get; set; }

        public string SpecialText { get; set; }
        public string SpecialIconName { get; set; }

        public string SpecialIconText()
        {
            if (!string.IsNullOrWhiteSpace(SpecialText))
            {
                var splitText = SpecialText.Split(':');
                return splitText.First();
            }

            return null;
        }

        public bool IsBrandonHill()
        {
            return Collections != null && Collections.Select(x => x.Id).Contains((int)CollectionIdentifier.BradonHill);
        }

        public bool IsDct()
        {
            return Collections != null && Collections.Select(x => x.Id).Contains((int)CollectionIdentifier.Dct);
        }

        public bool IsDctEssential()
        {
            return Collections != null &&
                   Collections.Select(x => x.Id).Contains((int)CollectionIdentifier.DctEssential);
        }

        public string ToDebugString()
        {
            var sb = new StringBuilder("Resource = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", Isbn: {0}", Isbn);
            sb.AppendFormat(", Title: {0}", Title);
            sb.Append("]");
            return sb.ToString();
        }

        public string SubTitleDisplay()
        {
            if (string.IsNullOrWhiteSpace(Subtitle))
            {
                return "";
            }

            return $"<h4>{Subtitle}</h4>";
        }

        private void SetResource(IResource resource, IFeaturedTitle featuredTitle)
        {
            ReleaseDate = resource.ReleaseDate;
            Title = resource.Title;
            Authors = resource.Authors;
            PublicationDate = resource.PublicationDate;

            PublicationDateYear = resource.PublicationDate?.Year ?? 0;

            StatusId = resource.StatusId;
            ListPrice = resource.ListPrice;
            BundlePrice3 = resource.BundlePrice3;
            IsForSale = resource.IsForSale;

            DueDate = StatusId == (int)ResourceStatus.Forthcoming ? resource.ForthcomingDate : null;

            PublisherId = resource.PublisherId;

            if (resource.Publisher != null)
            {
                PublisherId = resource.Publisher.Id;
                PublisherName = resource.Publisher.ToName();

                ConsolidatedPublisherName = resource.Publisher.ConsolidatedPublisher != null
                    ? resource.Publisher.Name
                    : null;

                PublisherProductStatement = resource.Publisher.ConsolidatedPublisher != null
                    ? resource.Publisher.ConsolidatedPublisher.ProductDescription
                    : resource.Publisher.ProductDescription;
            }

            ImageFileName = resource.ImageFileName;
            Specialties = resource.Specialties;
            PracticeAreas = resource.PracticeAreas;
            Collections = resource.Collections;
            Isbn = resource.Isbn;

            ResourceDescription = resource.Description;
            Edition = resource.Edition;
            Id = resource.Id;

            Subtitle = resource.SubTitle;

            IsDrugMonograph = resource.DrugMonograph;

            AdditionalContributors = resource.AdditionalContributors;
            DoodyReview = resource.DoodyReview;
            NlmCall = resource.NlmCall;

            Isbn10 = resource.Isbn10;
            Isbn13 = resource.Isbn13;
            EIsbn = resource.EIsbn;

            if (featuredTitle != null)
            {
                FeaturedTitleId = featuredTitle.ResourceId;
                IsFeaturedTitle = featuredTitle.RecordStatus;
                FeaturedTitleStartDate = featuredTitle.StartDate;
                FeaturedTitleEndDate = featuredTitle.EndDate;
            }

            ImageUrl = resource.ImageUrl;

            QaApprovalDate = resource.QaApprovalDate;
            LastPromotionDate = resource.LastPromotionDate;

            PageCount = resource.PageCount;

            Affiliation = resource.Affiliation;
            AffiliationUpdatedByPrelude = resource.AffiliationUpdatedByPrelude;
            ContainsVideo = resource.ContainsVideo;

            NotSaleable = resource.NotSaleable;
            NotSaleableDate = resource.NotSaleableDate;
            IsFreeResource = resource.IsFreeResource;
            DoodyRating = resource.DoodyRating;

            ArchiveDate = resource.ArchiveDate;

            ExcludeFromAutoArchive = resource.ExcludeFromAutoArchive;
        }
    }
}