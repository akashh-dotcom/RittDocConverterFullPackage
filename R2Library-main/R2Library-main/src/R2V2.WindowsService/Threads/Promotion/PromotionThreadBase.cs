#region

using R2V2.Core.Promotion;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.WindowsService.Threads.Promotion
{
    public class PromotionThreadBase : ThreadBase
    {
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public PromotionThreadBase(IUnitOfWorkProvider unitOfWorkProvider)
        {
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        /// <summary>
        /// </summary>
        protected void SetResourcePromoteQueueStatus(ResourcePromoteQueue resourcePromoteQueue,
            ResourcePromoteStatus status)
        {
            resourcePromoteQueue.PromoteStatus = status;
            UpdateResourcePromoteQueue(resourcePromoteQueue);
        }

        /// <summary>
        /// </summary>
        protected void UpdateResourcePromoteQueue(ResourcePromoteQueue resourcePromoteQueue)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    uow.Save(resourcePromoteQueue);
                    transaction.Commit();
                    uow.Commit();
                }
            }
        }
    }
}