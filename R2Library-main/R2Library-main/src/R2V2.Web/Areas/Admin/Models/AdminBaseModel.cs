#region

using System;
using System.Collections.Generic;
using R2V2.Core.Admin;
using R2V2.Core.SuperType;
using R2V2.Web.Areas.Admin.Models.Menus;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Areas.Admin.Models
{
    [Serializable]
    public abstract class AdminBaseModel : BaseModel
    {
        protected AdminBaseModel()
        {
        }

        protected AdminBaseModel(IAdminInstitution institution)
        {
            if (institution == null)
            {
                return;
            }

            InstitutionId = institution.Id;
            Institution = institution as AdminInstitution;
        }

        protected AdminBaseModel(IAdminInstitution institution, IQuery query)
        {
            if (institution == null)
            {
                return;
            }

            InstitutionId = institution.Id;
            Institution = institution as AdminInstitution;

            if (query != null)
            {
                PurchasedOnly = query.PurchasedOnly;
                IncludePdaResources = query.IncludePdaResources;
                IncludePdaHistory = query.IncludePdaHistory;
                IncludeSpecialDiscounts = query.IncludeSpecialDiscounts;
                RecommendationsOnly = query.RecommendationsOnly;
                ResourceListType = query.ResourceListType;
                DateRangeStart = query.DateRangeStart;
                DateRangeEnd = query.DateRangeEnd;
                IncludeFreeResources = query.IncludeFreeResources;
                CollectionListFilter = query.CollectionListFilter;
            }
        }

        public int Id { get; set; }
        public int InstitutionId { get; set; }

        public AdminInstitution Institution { get; private set; }

        public bool IsRittenhouseAdmin { get; set; }
        public bool IsInstitutionalAdmin { get; set; }
        public bool IsPublisherUser { get; set; }
        public bool IsSalesAssociate { get; set; }
        public bool IsExpertReviewer { get; set; }

        public IEnumerable<PageLinkSection> NavLinkSections { get; set; }

        public IEnumerable<PageLink> TabLinks { get; set; }

        public ActionsMenu ActionsMenu { get; set; }

        public static string GetSortByDescription(string sortByCode)
        {
            switch (sortByCode)
            {
                case "status":
                    return "Status";
                case "publicationdate":
                    return "Publication Date";
                case "releasedate":
                    return "R2 Release Date";
                case "title":
                    return "Book Title";
                case "publisher":
                    return "Publisher";
                case "author":
                    return "Author";
                case "duedate":
                    return "Due Date";
                case "price":
                    return "Price";
                case "pdadateadded":
                    return "PDA Date Added";
                case "pdadatedeleted":
                    return "PDA Date Removed";
                case "pdaviewcount":
                    return "PDA View Count";
                case "lastname":
                    return "Last Name";
                case "firstname":
                    return "First Name";
                case "email":
                    return "Email";
                case "role":
                    return "Role";
                default:
                    return sortByCode;
            }
        }
    }
}