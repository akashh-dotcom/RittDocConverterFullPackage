#region

using R2V2.Core.Admin;
using R2V2.Core.Export.FileTypes;
using R2V2.Web.Areas.Admin.Models.OrderHistory;

#endregion

namespace R2V2.Web.Areas.Admin.Services
{
    public interface IOrderHistoryService
    {
        OrderHistoryDetail GetOrderHistoryDetail(int institutionId, int orderId);
        OrderHistoryList GetOrderHistoryList(int institutionId);

        OrderHistoryListExcelExport GetOrderHistoryListExcelExport(int institutionId);

        OrderHistoryExcelExport GetOrderHistoryDetailExcelExport(int orderId, string resourceUrl,
            IAdminInstitution adminInstitution);

        int GetOrderHistoryId(int cartId, int institutionId);
    }
}