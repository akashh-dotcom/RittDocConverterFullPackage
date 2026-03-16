#region

using System;
using System.ComponentModel.DataAnnotations;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public interface IResourceOrderItem : IOrderItem
    {
        IResource CoreResource { get; }
        bool PdaPromotionApplied { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        DateTime AddedToCartDate { get; set; }

        bool WasAddedViaPda { get; set; }
        DateTime? AddedByNewEditionDate { get; set; }
    }
}