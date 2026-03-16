#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Subscriptions
{
    public interface ISubscriptionService
    {
        IList<InstitutionResourceLicense> GetSubscriptionLicenses(IUser user);
        IList<Subscription> GetAvailableSubscriptions();
        Subscription GetSubscription(int id);
        UserSubscription GetUserSubscription(int id);
        UserSubscription GetUserSubscription(int userId, int subscriptionId);
        int CreateUserSubscription(SubscriptionOrderHistory order, IUser currentUser, IUnitOfWork uow);
        int CreateOrderHistory(SubscriptionOrderHistory order, IUser currentUser, IUnitOfWork uow);
        SubscriptionOrderHistory GetOrderHistory(int id, IUser currentUser);
        void SaveUserSubscription(UserSubscription userSubscription);
    }

    public class SubscriptionService : ISubscriptionService
    {
        private readonly ICollectionManagementSettings _collectionManagementSettings;
        private readonly ILog<SubscriptionService> _log;
        private readonly IResourceService _resourceService;
        private readonly IQueryable<SubscriptionOrderHistory> _subscriptionOrderHistory;
        private readonly IQueryable<Subscription> _subscriptions;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly IQueryable<UserSubscription> _userSubscriptions;

        public SubscriptionService(
            ILog<SubscriptionService> log
            , IResourceService resourceService
            , IQueryable<Subscription> subscriptions
            , IQueryable<UserSubscription> userSubscriptions
            , IQueryable<SubscriptionOrderHistory> subscriptionOrderHistory
            , IUnitOfWorkProvider unitOfWorkProvider
            , ICollectionManagementSettings collectionManagementSettings
        )
        {
            _log = log;
            _resourceService = resourceService;
            _subscriptions = subscriptions;
            _userSubscriptions = userSubscriptions;
            _subscriptionOrderHistory = subscriptionOrderHistory;
            _unitOfWorkProvider = unitOfWorkProvider;
            _collectionManagementSettings = collectionManagementSettings;
        }

        public IList<InstitutionResourceLicense> GetSubscriptionLicenses(IUser user)
        {
            var licenses = new List<InstitutionResourceLicense>();

            if (user.Role.Code == RoleCode.SUBUSER)
            {
                var subs = user.Subscriptions.Where(x => x.CanView());
                foreach (var userSubscription in subs)
                {
                    //This is only used when the user needs to get reloaded because they just purchased a new Subscription. 
                    if (userSubscription.Subscription == null)
                    {
                        userSubscription.Subscription = GetSubscription(userSubscription.SubscriptionId);
                    }

                    if (userSubscription.Subscription != null)
                    {
                        foreach (var resource in userSubscription.Subscription.SubResources)
                        {
                            licenses.Add(new InstitutionResourceLicense
                            {
                                FirstPurchaseDate = DateTime.Now,
                                Id = 0,
                                LicenseCount = 1,
                                ResourceId = resource.ResourceId,
                                RecordStatus = true,
                                LicenseTypeId = (int)LicenseType.Trial
                            });
                        }
                    }
                }
            }

            return licenses;
        }

        public IList<Subscription> GetAvailableSubscriptions()
        {
            var subs = _subscriptions.Where(x => x.AllowSubscription).ToList();
            return subs;
        }

        public Subscription GetSubscription(int id)
        {
            return _subscriptions.FirstOrDefault(x => x.AllowSubscription && x.Id == id);
        }

        public UserSubscription GetUserSubscription(int id)
        {
            return _userSubscriptions.FirstOrDefault(x => x.Id == id);
        }

        public UserSubscription GetUserSubscription(int userId, int subscriptionId)
        {
            return _userSubscriptions.FirstOrDefault(x => x.UserId == userId && x.SubscriptionId == subscriptionId);
        }

        public void SaveUserSubscription(UserSubscription userSubscription)
        {
            var id = 0;
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    try
                    {
                        var dbUserSubscription = _userSubscriptions.FirstOrDefault(x => x.Id == userSubscription.Id);
                        dbUserSubscription.ActivationDate = userSubscription.ActivationDate;
                        dbUserSubscription.TrialExpirationDate = userSubscription.TrialExpirationDate;
                        dbUserSubscription.ExpirationDate = userSubscription.ExpirationDate;
                        uow.SaveOrUpdate(dbUserSubscription);
                        id = dbUserSubscription.Id;
                    }
                    catch (Exception ex)
                    {
                        var msg = new StringBuilder();
                        msg.AppendLine("!!!!! ------------------------------------------------ !!!!!");
                        msg.AppendLine("R2 SUBSCRIPTION ERROR -- IMMEDIATE ATTENTION REQUIRED!!!");
                        msg.AppendLine("!!!!! ------------------------------------------------ !!!!!");
                        msg.AppendLine(userSubscription.ToDebugString());
                        msg.AppendLine("!!!!! ------------------------------------------------ !!!!!");
                        msg.AppendLine();
                        msg.AppendFormat("Exception: {0}", ex.Message);

                        _log.Error(msg.ToString(), ex);
                    }
                    finally
                    {
                        if (id > 0)
                        {
                            uow.Commit();
                            transaction.Commit();
                        }
                        else
                        {
                            transaction.Rollback();
                        }
                    }
                }
            }
        }

        public int CreateUserSubscription(SubscriptionOrderHistory order, IUser currentUser, IUnitOfWork uow)
        {
            var trialDays = _collectionManagementSettings.SubscriptionTrialDays;
            var dbUserSubscription = GetUserSubscription(currentUser.Id, order.SubscriptionId);
            var trialEndDate = DateTime.Now.AddDays(trialDays);
            var userSubscription = new UserSubscription
            {
                SubscriptionId = order.SubscriptionId,
                RecordStatus = true,
                Type = order.Type,
                UserId = currentUser.Id,
                TrialExpirationDate = trialEndDate
            };

            if (dbUserSubscription != null)
            {
                trialEndDate = dbUserSubscription.CanTrial()
                    ? DateTime.Now.AddDays(trialDays)
                    : dbUserSubscription.TrialExpirationDate.GetValueOrDefault(DateTime.Now.AddDays(-1));
                userSubscription.ActivationDate = null;
                userSubscription.ExpirationDate = null;
                userSubscription = dbUserSubscription;
                userSubscription.RecordStatus = true;
                userSubscription.Type = order.Type;
                userSubscription.TrialExpirationDate = trialEndDate;
            }

            uow.SaveOrUpdate(userSubscription);
            return userSubscription.Id;
        }

        public int CreateOrderHistory(SubscriptionOrderHistory order, IUser currentUser, IUnitOfWork uow)
        {
            uow.SaveOrUpdate(order);
            return order.Id;
        }

        public SubscriptionOrderHistory GetOrderHistory(int id, IUser currentUser)
        {
            var item = _subscriptionOrderHistory.FirstOrDefault(x => x.Id == id && x.UserId == currentUser.Id);
            return item;
        }

        //public IList<UserSubscription>
    }
}