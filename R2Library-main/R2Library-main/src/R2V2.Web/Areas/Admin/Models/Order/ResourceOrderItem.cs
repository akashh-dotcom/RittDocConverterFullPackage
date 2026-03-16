#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Core.Recommendations;
using R2V2.Core.Resource;
using R2V2.Web.Areas.Admin.Models.Recommendations;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Order
{
    public class ResourceOrderItem : IResourceOrderItem
    {
        public ResourceOrderItem()
        {
        }


        public ResourceOrderItem(ICartItem cartItem, IResource resource, IList<Recommendation> recommendations,
            bool isSavedCart)
        {
            PopulateResource(resource, cartItem);
            PopulateCartItemDetails(cartItem, isSavedCart);
            BuildRecommendation(recommendations);
        }

        public Resource.Resource Resource { get; private set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? ExpireDate { get; private set; }

        public bool IsAvailableForSale { get; private set; }
        public string UnavailableForSaleMessage { get; private set; }

        public string SpecialText { get; set; }
        public string SpecialIconName { get; set; }

        public bool IsBundle { get; set; }
        //public string RecommendationText { get; set; }
        //public string Notes { get; set; }

        public IList<Recommended> Recommendations { get; set; }

        public List<string> AutomatedReasonCodes { get; set; }
        public IResource CoreResource { get; private set; }

        public int Id { get; set; }

        public int ItemId { get; set; }
        public int NumberOfLicenses { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal ListPrice { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal DiscountPrice { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalListPrice => IsBundle ? ListPrice : NumberOfLicenses * ListPrice;

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalDiscountPrice => IsBundle ? DiscountPrice : NumberOfLicenses * DiscountPrice;

        public DateTime? PurchaseDate { get; set; }
        public bool Include { get; private set; }

        public bool PdaPromotionApplied { get; set; }

        [DisplayFormat(DataFormatString = "{0:M/d/yyyy}")]
        public DateTime AddedToCartDate { get; set; }

        public bool WasAddedViaPda { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? AddedByNewEditionDate { get; set; }

        public string SpecialIconText()
        {
            if (!string.IsNullOrWhiteSpace(SpecialText))
            {
                var splitText = SpecialText.Split(':');
                return splitText.First();
            }

            return null;
        }

        private void PopulateCartItemDetails(ICartItem cartItem, bool isSavedCart)
        {
            ItemId = cartItem.Id;
            ListPrice = cartItem.ListPrice;
            DiscountPrice = cartItem.DiscountPrice;
            NumberOfLicenses = cartItem.NumberOfLicenses;
            PurchaseDate = cartItem.PurchaseDate;
            Include = cartItem.Include;
            AddedToCartDate = cartItem.CreationDate;
            PdaPromotionApplied = cartItem.PdaPromotionApplied;
            SetIsAvailableForSale(cartItem.OriginalSourceId, isSavedCart);
            SpecialText = cartItem.SpecialText;
            SpecialIconName = cartItem.SpecialIconName;
            AddedByNewEditionDate = cartItem.AddedByNewEditionDate;
            AutomatedReasonCodes = cartItem.AutomatedReasonCodes;
        }

        public void PopulateResource(IResource resource, ICartItem cartItem)
        {
            CoreResource = resource;
            Resource = new Resource.Resource(resource, null, cartItem);
            IsBundle = cartItem.IsBundle;
            Id = resource.Id;
        }

        private void BuildRecommendation(IList<Recommendation> recommendations)
        {
            if (recommendations != null && recommendations.Any())
            {
                Recommendations = new List<Recommended>();
                foreach (var recommendation in recommendations)
                {
                    Recommendations.Add(new Recommended(recommendation));
                }
            }
        }

        private void SetIsAvailableForSale(short originalSourceId, bool isSavedCart)
        {
            WasAddedViaPda = originalSourceId == (short)LicenseOriginalSource.Pda;
            IsAvailableForSale = true;

            if (WasAddedViaPda && !isSavedCart)
            {
                ExpireDate = AddedToCartDate.AddDays(30);
                if (ExpireDate < DateTime.Now)
                {
                    IsAvailableForSale = false;
                    UnavailableForSaleMessage = $"PDA resource expired on {ExpireDate:MM/dd/yyyy}. ";
                }
            }

            if (Resource.StatusId == (int)ResourceStatus.Archived)
            {
                IsAvailableForSale = false;
                UnavailableForSaleMessage = $"{UnavailableForSaleMessage}Resource has been archived.";
            }

            if (Resource.StatusId == (int)ResourceStatus.Inactive)
            {
                IsAvailableForSale = false;
                UnavailableForSaleMessage = $"{UnavailableForSaleMessage}Resource is inactive.";
            }
        }
    }
}