#region

using System.Collections.Generic;
using System.Linq;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.Export.FileTypes
{
    public class RequestedResourcesExcelExport : ExcelBase
    {
        public RequestedResourcesExcelExport(List<ResourceRequestItem> items, List<IResource> resources,
            List<Institution.Institution> institutions)
        {
            SpecifyColumn("Account Number", "String");
            SpecifyColumn("Instituion Name", "String");
            SpecifyColumn("Territory", "String");
            SpecifyColumn("Institution Type", "String");
            SpecifyColumn("Resource Title", "String");
            SpecifyColumn("Isbn 10", "String");
            SpecifyColumn("Isbn 13", "String");
            SpecifyColumn("eIsbn", "String");
            SpecifyColumn("Publisher", "String");
            SpecifyColumn("Last Requested", "DateTime");
            SpecifyColumn("Requested Count", "Int32");
            SpecifyColumn("Purchase Price", "decimal");
            //SpecifyColumn("Cart/Order History Url", "String");
            //SpecifyColumn("Automated Cart Url", "String");

            ResourceRequestItem lastItem = null;

            foreach (var item in items)
            {
                var institution = institutions.FirstOrDefault(x => x.Id == item.InstitutionId);
                var resource = resources.FirstOrDefault(x => x.Id == item.ResourceId);

                if (institution != null && resource != null)
                {
                    //Filters out multitple purchases and added to carts. We only about the first purchase. 
                    if (DuplicateResourceRequestItems(lastItem, item))
                    {
                        continue;
                    }

                    PopulateFirstColumn(institution.AccountNumber);
                    PopulateNextColumn(institution.Name);
                    PopulateNextColumn(institution.Territory?.Code);
                    PopulateNextColumn(institution.Type?.Name);
                    PopulateNextColumn(resource.Title);
                    PopulateNextColumn(resource.Isbn10);
                    PopulateNextColumn(resource.Isbn13);
                    PopulateNextColumn(resource.EIsbn);
                    PopulateNextColumn(resource.Publisher?.Name);

                    PopulateNextColumn(item.LastRequestDate);
                    PopulateNextColumn(item.RequestCount);
                    PopulateLastColumn(item.PurchasePrice.GetValueOrDefault(0));
                    //PopulateNextColumn(item.CartId.HasValue ? $"{cartUrlBase}/{item.InstitutionId}?cartId={item.CartId.Value}" : "");
                    //PopulateLastColumn(item.AutomatedCartId.HasValue ? $"{automatedCartUrlBase}?AutomatedCartId={item.AutomatedCartId.Value}" : "");
                }

                lastItem = item;
            }
        }

        private bool DuplicateResourceRequestItems(ResourceRequestItem lastItem, ResourceRequestItem item)
        {
            if (lastItem == null)
            {
                return false;
            }

            if (lastItem.LastRequestDate != item.LastRequestDate)
            {
                return false;
            }

            if (lastItem.InstitutionId != item.InstitutionId)
            {
                return false;
            }

            if (lastItem.ResourceId != item.ResourceId)
            {
                return false;
            }

            return true;
        }
    }
}