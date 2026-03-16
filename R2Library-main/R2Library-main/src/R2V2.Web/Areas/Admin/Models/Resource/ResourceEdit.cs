#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using R2V2.Core.Authentication;
using R2V2.Core.Promotion;
using R2V2.Core.Publisher;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Web.Areas.Admin.Models.Special;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models.Resource;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Resource
{
    public class ResourceEdit : ResourceDetail
    {
        private SelectList _resourceStatusSelectList;

        public ResourceEdit()
        {
        }

        /// <param name="webSettings"> </param>
        /// <param name="user"> </param>
        public ResourceEdit(IResource resource, IFeaturedTitle featuredTitle, IEnumerable<IPublisher> publishers,
            IPublisher currentPublisher, IEnumerable<ISpecialty> specialties
            , IEnumerable<IPracticeArea> practiceAreas, IEnumerable<ICollection> collections,
            ResourceQuery resourceQuery, IWebSettings webSettings, IUser user
            , List<SpecialResourceModel> currentSpecialResourceModels, List<SpecialAdminModel> specials,
            IList<ResourcePromoteQueue> resourcePromoteQueues, IEnumerable<Core.Authentication.User> raPromotionUsers
        )
            : base(resource, featuredTitle, resourceQuery, webSettings, user, null, resourcePromoteQueues,
                raPromotionUsers)
        {
            PopulatePublishersSelectList(publishers, currentPublisher);
            PopulatePracticeAreaSelectListItems(practiceAreas.ToList());
            PopulateSpecialtiesSelectListItems(specialties.ToList());
            PopulateCollectionsSelectListItems(collections.ToList());
            PopulateSpecialsSelectListItems(currentSpecialResourceModels, specials);
            QaApproval = resource != null && resource.QaApprovalDate != null;
        }

        [Display(Name = @"Resource Status: ")]
        public SelectList ResourceStatusSelectList =>
            _resourceStatusSelectList
            ?? (_resourceStatusSelectList = new SelectList
                (
                    new List<SelectListItem>
                    {
                        new SelectListItem { Text = @"Active", Value = ((int)ResourceStatus.Active).ToString() },
                        new SelectListItem
                            { Text = @"Pre-Order", Value = ((int)ResourceStatus.Forthcoming).ToString() },
                        new SelectListItem { Text = @"Archived", Value = ((int)ResourceStatus.Archived).ToString() },
                        new SelectListItem { Text = @"Inactive", Value = ((int)ResourceStatus.Inactive).ToString() }
                    }
                    , "Value", "Text"
                )
            );

        public string PublisherDisplayText { get; set; }
        [Display(Name = @"Publisher: ")] public List<SelectListItem> PublishersSelectList { get; private set; }

        [Display(Name = @"Assign to Practice Areas: ")]
        public List<SelectListItem> PracticeAreaSelectListItems { get; private set; }

        public List<SelectListItem> SelectedPracticeAreasSelectListItems { get; private set; }
        public int[] PracticeAreaSelected { get; set; }

        [Display(Name = @"Assign Disciplines: ")]
        public List<SelectListItem> SpecialtiesSelectListItems { get; private set; }

        public List<SelectListItem> SelectedSpecialtiesSelectListItems { get; private set; }
        public int[] SpecialtiesSelected { get; set; }


        [Display(Name = @"Assign Collections: ")]
        public List<SelectListItem> CollectionsSelectListItems { get; private set; }

        public List<SelectListItem> SelectedCollectionsSelectListItems { get; private set; }
        public int[] CollectionsSelected { get; set; }

        [Display(Name = @"QA Approval: ")]
        [ResourceMinPrice(ErrorMessage =
            @"Resource price must be greater than ${0:0.00} to mark the resource as QA approved. Price must be greater than ${1:0.00} or marked as Free Resource to be sold.")]
        public bool QaApproval { get; set; }

        public string Title { get; set; }

        public int BookCoverMaxWidth { get; private set; }
        public int BookCoverMaxHeight { get; private set; }

        public int BookCoverMaxSizeInKb { get; private set; }

        [Display(Name = @"Assign Specials: ")] public List<SelectListItem> SpecialsSelectListItems { get; private set; }
        public List<SelectListItem> SelectedSpecialsSelectListItems { get; private set; }
        public int[] SpecialsSelected { get; set; }

        public void PopulatePublishersSelectList(IEnumerable<IPublisher> publishers, IPublisher publisher)
        {
            if (Resource.StatusId == (int)ResourceStatus.Archived || Resource.StatusId == (int)ResourceStatus.Active)
            {
                if (publisher == null || publisher.Id == 0)
                {
                    publisher = publishers.FirstOrDefault(x => x.Id == Resource.PublisherId);
                }

                PopulatePublisher(publisher);
            }
            else
            {
                PopulatePublishers(publishers);
            }
        }

        private void PopulatePublisher(IPublisher publisher)
        {
            if (publisher != null)
            {
                var resourceCount = publisher.ResourceCount + publisher.ParentResourceCount +
                                    publisher.ChildrenResourceCount;

                PublisherDisplayText = new StringBuilder()
                    .AppendFormat("{0} ", publisher.ToName())
                    .AppendFormat("{0}",
                        publisher.RecordStatus ? $"(Resource Count: {resourceCount}) " : "")
                    .AppendFormat("{0}",
                        publisher.ConsolidatedPublisher != null
                            ? $"-- [Consolidated From {publisher.Name}]"
                            : "")
                    .ToString();
            }
        }

        private void PopulatePublishers(IEnumerable<IPublisher> publishers)
        {
            var selectListItems = (from publisher in publishers
                let resourceCount = publisher.ResourceCount + publisher.ParentResourceCount +
                                    publisher.ChildrenResourceCount
                select new SelectListItem
                {
                    Text = $"{publisher.Name} (Resource Count: {resourceCount})", Value = $"{publisher.Id}"
                }).ToList();

            PublishersSelectList = new List<SelectListItem> { new SelectListItem { Text = "", Value = "" } };
            PublishersSelectList.AddRange(selectListItems);
        }


        public void PopulateSpecialsSelectListItems(List<SpecialResourceModel> currentSpecialResourceModels,
            List<SpecialAdminModel> specials)
        {
            SelectedSpecialsSelectListItems = new List<SelectListItem>();

            //Populate the ACTIVE specials
            if (currentSpecialResourceModels != null)
            {
                foreach (var item in currentSpecialResourceModels)
                {
                    SelectedSpecialsSelectListItems.Add(new SelectListItem
                    {
                        Selected = false,
                        Text = $"{item.SpecialName}: {item.DiscountPercentage}%",
                        Value = item.SpecialDiscountId.ToString()
                    });
                }

                CollectionsSelected = currentSpecialResourceModels.Select(x => x.SpecialDiscountId).ToArray();
            }

            //Populate the NON-ACTIVE specials
            SpecialsSelectListItems = new List<SelectListItem>();
            if (specials != null)
            {
                if (currentSpecialResourceModels != null)
                {
                    foreach (var special in specials)
                    {
                        if (special.SpecialDiscounts != null)
                        {
                            foreach (var specialDiscount in special.SpecialDiscounts)
                            {
                                if (currentSpecialResourceModels.All(x =>
                                        x.SpecialDiscountId != specialDiscount.SpecialDiscountId))
                                {
                                    SpecialsSelectListItems.Add(new SelectListItem
                                    {
                                        Selected = false,
                                        Text = $"{special.Name}: {specialDiscount.DiscountPercentage}%",
                                        Value = specialDiscount.SpecialDiscountId.ToString()
                                    });
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var special in specials)
                    {
                        if (special.SpecialDiscounts != null)
                        {
                            foreach (var specialDiscount in special.SpecialDiscounts)
                            {
                                SpecialsSelectListItems.Add(new SelectListItem
                                {
                                    Selected = false,
                                    Text =
                                        string.Format("{0}: {1}%", special.Name,
                                            specialDiscount.DiscountPercentage),
                                    Value = specialDiscount.SpecialDiscountId.ToString()
                                });
                            }
                        }
                    }
                }
            }
        }

        public void Init(IResource resource, IFeaturedTitle featuredTitle, IEnumerable<IPublisher> publishers,
            IPublisher currentPublisher, IEnumerable<ISpecialty> specialties
            , IEnumerable<IPracticeArea> practiceAreas, IEnumerable<ICollection> collections,
            ResourceQuery resourceQuery, IWebSettings webSettings, IUser user
            , List<SpecialResourceModel> currentSpecials, List<SpecialAdminModel> availableSpecials,
            IList<ResourcePromoteQueue> resourcePromoteQueues, IEnumerable<Core.Authentication.User> raPromotionUsers)
        {
            Init(resource, featuredTitle, resourceQuery, webSettings, user,
                currentSpecials != null ? currentSpecials.FirstOrDefault() : null, resourcePromoteQueues,
                raPromotionUsers);
            PopulatePublishersSelectList(publishers, currentPublisher);
            PopulatePracticeAreaSelectListItems(practiceAreas.ToList());
            PopulateSpecialtiesSelectListItems(specialties.ToList());
            PopulateCollectionsSelectListItems(collections.ToList());
            PopulateSpecialsSelectListItems(currentSpecials, availableSpecials);
            QaApproval = resource != null && resource.QaApprovalDate != null;
        }

        public void PopulatePracticeAreaSelectListItems(List<IPracticeArea> practiceAreas)
        {
            SelectedPracticeAreasSelectListItems = new List<SelectListItem>();

            if (Resource.PracticeAreas != null)
            {
                foreach (var pa in Resource.PracticeAreas.Where(x =>
                             !SelectedPracticeAreasSelectListItems.Any(y =>
                                 y.Text == x.Name && y.Value == x.Id.ToString())))
                {
                    SelectedPracticeAreasSelectListItems.Add(new SelectListItem
                        { Text = pa.Name, Value = pa.Id.ToString() });
                }

                PracticeAreaSelected =
                    Resource.PracticeAreas.Select(practiceArea => practiceArea.Id).Distinct().ToArray();
            }

            PracticeAreaSelectListItems = new List<SelectListItem>();
            foreach (var practiceArea in practiceAreas)
            {
                var item = new SelectListItem { Text = practiceArea.Name, Value = practiceArea.Id.ToString() };
                if (!SelectedPracticeAreasSelectListItems.Select(x => x.Value).Contains(item.Value))
                {
                    PracticeAreaSelectListItems.Add(item);
                }
            }
        }

        public void PopulateSpecialtiesSelectListItems(List<ISpecialty> specialties)
        {
            SelectedSpecialtiesSelectListItems = new List<SelectListItem>();
            if (Resource.Specialties != null)
            {
                foreach (var specialty in Resource.Specialties.Where(x =>
                             !SelectedSpecialtiesSelectListItems.Any(y =>
                                 y.Text == x.Name && y.Value == x.Id.ToString())))
                {
                    SelectedSpecialtiesSelectListItems.Add(new SelectListItem
                        { Text = specialty.Name, Value = specialty.Id.ToString() });
                }

                SpecialtiesSelected = Resource.Specialties.Select(specialty => specialty.Id).Distinct().ToArray();
            }

            SpecialtiesSelectListItems = new List<SelectListItem>();
            foreach (var specialty in specialties)
            {
                var item = new SelectListItem { Text = specialty.Name, Value = specialty.Id.ToString() };
                if (!SelectedSpecialtiesSelectListItems.Select(x => x.Value).Contains(item.Value))
                {
                    SpecialtiesSelectListItems.Add(item);
                }
            }
        }

        public void PopulateCollectionsSelectListItems(List<ICollection> collections)
        {
            SelectedCollectionsSelectListItems = new List<SelectListItem>();
            if (Resource.Collections != null)
            {
                foreach (var collection in Resource.Collections.Where(x =>
                             !SelectedCollectionsSelectListItems.Any(y =>
                                 y.Text == x.Name && y.Value == x.Id.ToString())))
                {
                    SelectedCollectionsSelectListItems.Add(new SelectListItem
                        { Text = collection.Name, Value = collection.Id.ToString() });
                }

                CollectionsSelected = Resource.Collections.Select(x => x.Id).Distinct().ToArray();
            }

            CollectionsSelectListItems = new List<SelectListItem>();
            foreach (var collection in collections)
            {
                var item = new SelectListItem { Text = collection.Name, Value = collection.Id.ToString() };
                if (!SelectedCollectionsSelectListItems.Select(x => x.Value).Contains(item.Value))
                {
                    CollectionsSelectListItems.Add(item);
                }
            }
        }

        public bool IsEditable(string objectName)
        {
            if (!string.IsNullOrWhiteSpace(objectName))
            {
                switch (objectName.ToLower())
                {
                    case "duedate":
                        return !DueDateDisabled();
                    case "affiliation":
                        return !AffiliationDisabled();
                    case "authors":
                    case "publisherid":
                    case "title":
                    case "isbn10":
                    case "isbn13":
                    case "publishersselectlist":
                        return !DisabledByStatus();
                }
            }

            return true;
        }

        public bool DueDateDisabled()
        {
            return Resource.StatusId != (int)ResourceStatus.Forthcoming && Resource.StatusId != 0;
        }

        public bool AffiliationDisabled()
        {
            return Resource != null && Resource.AffiliationUpdatedByPrelude;
        }

        public bool DisabledByStatus()
        {
            return Resource.StatusId == (int)ResourceStatus.Archived ||
                   Resource.StatusId == (int)ResourceStatus.Active || Resource.StatusId == (int)ResourceStatus.Inactive;
        }


        public void SetBookCoverLimits(IWebImageSettings webImageSettings)
        {
            // SJS -- 10/1/2012 - there should be a better way
            BookCoverMaxHeight = webImageSettings.BookCoverMaxHeight;
            BookCoverMaxWidth = webImageSettings.BookCoverMaxWidth;
            BookCoverMaxSizeInKb = webImageSettings.BookCoverMaxSizeInKb;
        }
    }
}