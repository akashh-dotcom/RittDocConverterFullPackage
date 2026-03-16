#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using R2V2.Core.Admin;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Web.Helpers;

#endregion

namespace R2V2.Web.Areas.Admin.Models.ProductSubscription
{
    public class ProductSubscriptions : AdminBaseModel
    {
        public ProductSubscriptions(IEnumerable<Core.CollectionManagement.ProductSubscription> productSubscriptions,
            Product product, IAdminInstitution institution)
            : base(institution)
        {
            Subscriptions = productSubscriptions;

            Product = product;
        }

        public Product Product { get; set; }

        public IEnumerable<Core.CollectionManagement.ProductSubscription> Subscriptions { get; }
    }

    public class ProductSubscription : AdminBaseModel
    {
        private SelectList _productSubscriptionStatusSelectList;

        public ProductSubscription()
        {
        }

        public ProductSubscription(IAdminInstitution institution, Product product)
            : base(institution)
        {
            Product = product;
        }

        public Product Product { get; set; }

        [Display(Name = "Start Date:")]
        [Required]
        [DateTenYears("StartDate")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date:")]
        [Required]
        [DateTenYears("EndDate")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Status:")] [Required] public ProductSubscriptionStatus ProductSubscriptionStatus { get; set; }

        public SelectList ProductSubscriptionStatusSelectList =>
            _productSubscriptionStatusSelectList ?? (_productSubscriptionStatusSelectList = new SelectList(
                new List<ProductSubscriptionStatus>
                {
                    ProductSubscriptionStatus.Active,
                    ProductSubscriptionStatus.Disabled,
                    ProductSubscriptionStatus.Trial
                }, ProductSubscriptionStatus));
    }

    public class ProductSubscriptionAdd : ProductSubscription
    {
        public ProductSubscriptionAdd()
        {
        }

        public ProductSubscriptionAdd(IAdminInstitution institution, Product product) : base(institution, product)
        {
        }
    }

    public class ProductSubscriptionEdit : ProductSubscription
    {
        public ProductSubscriptionEdit()
        {
        }

        public ProductSubscriptionEdit(IAdminInstitution institution,
            Core.CollectionManagement.ProductSubscription productSubscription)
            : base(institution, productSubscription.Product)
        {
            StartDate = productSubscription.StartDate;
            EndDate = productSubscription.EndDate;
            ProductSubscriptionStatus = productSubscription.ProductSubscriptionStatus;
        }
    }
}