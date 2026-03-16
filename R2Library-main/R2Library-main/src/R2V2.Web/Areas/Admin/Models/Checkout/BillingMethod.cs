#region

using System;
using R2V2.Core.CollectionManagement;

#endregion

namespace R2V2.Web.Areas.Admin.Models.Checkout
{
    public class BillingMethod
    {
        public static BillingMethod ExistingRittenhouseAccount = new BillingMethod
        {
            Id = BillingMethodEnum.ExistingRittenhouseAccount, Description = "Bill to my existing Rittenhouse account"
        };

        public static BillingMethod DepositAccount = new BillingMethod
            { Id = BillingMethodEnum.DepositAccount, Description = "Bill to my deposit account" };

        public static BillingMethod CreditCardOnFile = new BillingMethod
            { Id = BillingMethodEnum.CreditCardOnFile, Description = "Bill to my credit card" };

        public static BillingMethod Reseller = new BillingMethod
            { Id = BillingMethodEnum.Reseller, Description = "Paid for by Reseller" };

        public BillingMethodEnum Id { get; protected set; }

        public string Description { get; protected set; }
    }

    public static class BillingMethodExtensions
    {
        public static BillingMethod ToBillingMethod(this BillingMethodEnum billingMethod)
        {
            switch (billingMethod)
            {
                case BillingMethodEnum.ExistingRittenhouseAccount:
                    return BillingMethod.ExistingRittenhouseAccount;

                case BillingMethodEnum.DepositAccount:
                    return BillingMethod.DepositAccount;

                case BillingMethodEnum.CreditCardOnFile:
                    return BillingMethod.CreditCardOnFile;

                case BillingMethodEnum.Reseller:
                    return BillingMethod.Reseller;

                default:
                    throw new ArgumentOutOfRangeException("billingMethod");
            }
        }
    }
}