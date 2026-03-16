#region

using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Core.ReserveShelf
{
    public class ReserveShelfQuery : CollectionManagementQuery, IReserveShelfQuery
    {
        public ReserveShelfQuery()
        {
        }

        public ReserveShelfQuery(IReserveShelfQuery reserveShelfQuery)
            : base(reserveShelfQuery)
        {
            InstitutionId = reserveShelfQuery.InstitutionId;
            ReserveShelfId = reserveShelfQuery.ReserveShelfId;
        }

        public new bool IsReserveShelf
        {
            get => true;
            set { }
        }

        public new bool DefaultQuery =>
            string.IsNullOrWhiteSpace(Query) && string.IsNullOrWhiteSpace(SortBy) &&
            SortDirection == SortDirection.Ascending &&
            ResourceStatus == ResourceStatus.All && ResourceFilterType == ResourceFilterType.All &&
            PracticeAreaFilter <= 0 && SpecialtyFilter <= 0 && CollectionFilter <= 0;
    }
}