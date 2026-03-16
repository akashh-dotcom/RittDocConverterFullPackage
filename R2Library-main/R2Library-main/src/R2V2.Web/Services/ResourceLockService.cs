#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using R2V2.Core.Email;
using R2V2.Core.Institution;
using R2V2.Core.Reports;
using R2V2.Core.RequestLogger;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;
using R2V2.Web.Entities;
using R2V2.Web.Infrastructure.Settings;
using ContentView = R2V2.Core.Reports.ContentView;

#endregion

namespace R2V2.Web.Services
{
    public interface IResourceLockService
    {
        ResourceLockStatus IsPrintEnabled(int institutionId, int? userId, IResource resource,
            IRequestStorageService requestStorage);

        ResourceLockStatus IsEmailEnabled(int institutionId, int? userId, IResource resource,
            IRequestStorageService requestStorage);
    }

    public class ResourceLockService : IResourceLockService
    {
        //private readonly IRequestStorageService _requestStorageService;

        private const string InstitutionResourceLocksKey = "Institution.Resource.Locks";
        private const string InstitutionResourceLockedByUserKey = "Institution.Resource.LockedByUser";
        private readonly IQueryable<ContentView> _contentViews;
        private readonly EmailQueueService _emailQueueService;
        private readonly InstitutionResourceEmailDataService _institutionResourceEmailDataService;
        private readonly IQueryable<InstitutionResourceLock> _institutionResourceLocks;
        private readonly IQueryable<InstitutionResourceLockedPerUser> _institutionResourcesLockedPerUser;
        private readonly ILog<ResourceLockService> _log;
        private readonly IQueryable<PageContentView> _pageContentViews;
        private readonly IQueryable<PageView> _pageViews;
        private readonly ResourceLockEmailBuildService _resourceLockEmailBuildService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly Core.UserService _userService;
        private readonly IWebSettings _webSettings;

        public ResourceLockService(ILog<ResourceLockService> log
            , IUnitOfWorkProvider unitOfWorkProvider
            , IWebSettings webSettings
            , IQueryable<InstitutionResourceLock> institutionResourceLocks
            , IQueryable<InstitutionResourceLockedPerUser> institutionResourcesLockedPerUser
            , IQueryable<ContentView> contentViews
            , IQueryable<PageContentView> pageContentViews
            , IQueryable<PageView> pageViews
            , Core.UserService userService
            , ResourceLockEmailBuildService resourceLockEmailBuildService
            , EmailQueueService emailQueueService
            , InstitutionResourceEmailDataService institutionResourceEmailDataService
        )
        {
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
            _webSettings = webSettings;
            _institutionResourceLocks = institutionResourceLocks;
            _institutionResourcesLockedPerUser = institutionResourcesLockedPerUser;
            _contentViews = contentViews;
            _pageContentViews = pageContentViews;
            _pageViews = pageViews;
            _userService = userService;
            _resourceLockEmailBuildService = resourceLockEmailBuildService;
            _emailQueueService = emailQueueService;
            _institutionResourceEmailDataService = institutionResourceEmailDataService;
        }

        public ResourceLockStatus IsPrintEnabled(int institutionId, int? userId, IResource resource,
            IRequestStorageService requestStorage)
        {
            var resourcePrintLockStatus =
                new ResourceLockStatus(LockType.Print, institutionId, userId, resource, _webSettings);

            var isResourceLockedPerUser =
                IsInstitutionResourceLockedPerUser(institutionId, resource.Id, requestStorage);

            var resourceLocks = GetInstitutionResourceLocks(institutionId, userId, resource.Id, requestStorage);
            if (resourceLocks.Any(x => x.LockType == LockType.Print || x.LockType == LockType.All))
            {
                resourcePrintLockStatus.SetStatus(resourceLocks.FirstOrDefault());
                return resourcePrintLockStatus;
            }

            var distinctPrintRequestCount =
                _contentViews.Where(x =>
                        x.InstitutionId == institutionId && x.ResourceId == resource.Id &&
                        x.Timestamp > resourcePrintLockStatus.MinTimestamp &&
                        x.ActionTypeId == (int)ContentActionType.Print && x.ChapterSectionId != null &&
                        ((isResourceLockedPerUser && x.UserId == userId) || !isResourceLockedPerUser))
                    .Select(x => x.ChapterSectionId).Distinct().Count();

            resourcePrintLockStatus.CalculateStatus(distinctPrintRequestCount);

            _log.Debug(resourcePrintLockStatus.ToDebugString());

            if (resourcePrintLockStatus.LimitReached)
            {
                var lockUserId = isResourceLockedPerUser ? userId : null;
                var institutionResourceLock = InsertInstitutionResourceLocks(institutionId, lockUserId, resource.Id,
                    resourcePrintLockStatus.MinTimestamp, LockType.Print);

                var alertSuccessfullySent = SendResourcePrintLockAlertEmail(institutionResourceLock, resource);
                _log.InfoFormat("alertSuccessfullySent: {0}", alertSuccessfullySent);
            }

            return resourcePrintLockStatus;
        }

        public ResourceLockStatus IsEmailEnabled(int institutionId, int? userId, IResource resource,
            IRequestStorageService requestStorage)
        {
            var resourceLockStatus =
                new ResourceLockStatus(LockType.Email, institutionId, userId, resource, _webSettings);

            var isResourceLockedPerUser =
                IsInstitutionResourceLockedPerUser(institutionId, resource.Id, requestStorage);

            var resourceLocks = GetInstitutionResourceLocks(institutionId, userId, resource.Id, requestStorage);
            if (resourceLocks.Any(x => x.LockType == LockType.Email || x.LockType == LockType.All))
            {
                resourceLockStatus.SetStatus(resourceLocks.FirstOrDefault());
                return resourceLockStatus;
            }

            var distinctEmailRequestCount =
                _contentViews.Where(x =>
                        x.InstitutionId == institutionId && x.ResourceId == resource.Id &&
                        x.Timestamp > resourceLockStatus.MinTimestamp &&
                        x.ActionTypeId == (int)ContentActionType.Email && x.ChapterSectionId != null &&
                        ((isResourceLockedPerUser && x.UserId == userId) || !isResourceLockedPerUser))
                    .Select(x => x.ChapterSectionId).Distinct().Count();

            resourceLockStatus.CalculateStatus(distinctEmailRequestCount);

            _log.Debug(resourceLockStatus.ToDebugString());

            if (resourceLockStatus.LimitReached)
            {
                var lockUserId = isResourceLockedPerUser ? userId : null;
                var institutionResourceLock = InsertInstitutionResourceLocks(institutionId, lockUserId, resource.Id,
                    resourceLockStatus.MinTimestamp, LockType.Email);

                var alertSuccessfullySent = SendResourceEmailLockAlertEmail(institutionResourceLock, resource);
                _log.InfoFormat("alertSuccessfullySent: {0}", alertSuccessfullySent);
            }

            return resourceLockStatus;
        }

        private IList<InstitutionResourceLock> GetInstitutionResourceLocks(int institutionId, int? userId,
            int resourceId, IRequestStorageService requestStorage)
        {
            if (requestStorage.Has(InstitutionResourceLocksKey))
            {
                return requestStorage.Get<List<InstitutionResourceLock>>(InstitutionResourceLocksKey);
            }

            var isResourceLockedPerUser = IsInstitutionResourceLockedPerUser(institutionId, resourceId, requestStorage);

            var now = DateTime.Now;
            var locks =
                _institutionResourceLocks.Where(x =>
                        x.InstitutionId == institutionId && x.ResourceId == resourceId && x.LockStartDate < now &&
                        x.LockEndDate > now &&
                        ((isResourceLockedPerUser && x.UserId == userId) || !isResourceLockedPerUser))
                    .ToList();
            requestStorage.Put(InstitutionResourceLocksKey, locks);
            return locks;
        }

        private bool IsInstitutionResourceLockedPerUser(int institutionId, int resourceId,
            IRequestStorageService requestStorage)
        {
            if (!requestStorage.Has(InstitutionResourceLockedByUserKey))
            {
                var resources = _institutionResourcesLockedPerUser
                    .Where(x => x.InstitutionId == institutionId && x.ResourceId == resourceId).ToList();
                requestStorage.Put(InstitutionResourceLockedByUserKey, resources);
            }

            return requestStorage.Get<List<InstitutionResourceLockedPerUser>>(InstitutionResourceLockedByUserKey).Any();
        }

        private ResourceLockData GetResourceLockData(int institutionId, int resourceId, DateTime minTimestamp,
            LockType lockType)
        {
            var actionTypeId = lockType == LockType.Print ? 16 : lockType == LockType.Email ? 17 : 0;
            var pageContentViews = _pageContentViews.Where(x =>
                    x.InstitutionId == institutionId && x.ResourceId == resourceId && x.Timestamp > minTimestamp &&
                    x.ActionTypeId == actionTypeId)
                .OrderBy(x => x.Timestamp)
                .ToList();

            var sessionIds = pageContentViews.Select(x => x.SessionId).Distinct().ToArray();

            var lockData = new ResourceLockData(lockType);
            var requestCount = 0;

            for (var i = 0; i < sessionIds.Length; i++)
            {
                var sessionPageContentViews = pageContentViews.Where(x => x.SessionId == sessionIds[i])
                    .OrderBy(x => x.Timestamp)
                    .ToList();

                //requestCount = requestCount + sessionPageContentViews.Count;
                var firstPageContentView = sessionPageContentViews.First();
                var lastPageContentView = sessionPageContentViews.Last();
                var distinctSections = sessionPageContentViews.Select(x => x.ChapterSectionId).Distinct().ToArray();

                if (i == 0)
                {
                    lockData.FirstRequesTimestamp = firstPageContentView.Timestamp;
                    lockData.RequestCount = pageContentViews.Count;
                    lockData.UserData = new List<ResourceLockUserData>();
                }
                else if (lockData.FirstRequesTimestamp > firstPageContentView.Timestamp)
                {
                    lockData.FirstRequesTimestamp = firstPageContentView.Timestamp;
                }

                var firstPageView = _pageViews.Where(x => x.SessionId == sessionIds[i])
                    .OrderBy(x => x.Timestamp)
                    .Take(1)
                    .FirstOrDefault();

                var resourceLockUserData = new ResourceLockUserData(lockType)
                {
                    UserId = firstPageContentView.UserId,
                    UserFullName = BuildFullName(firstPageContentView),
                    EmailAddress = firstPageContentView.EmailAddress,
                    IpAddress = string.Format("{0}.{1}.{2}.{3}", firstPageContentView.IpAddressOctetA,
                        firstPageContentView.IpAddressOctetB,
                        firstPageContentView.IpAddressOctetC, firstPageContentView.IpAddressOctetD),
                    IpNumber = firstPageContentView.IpAddressInteger,
                    //RequestCount = sessionPageContentViews.Count,
                    RequestCount = distinctSections.Length,
                    FirstRequesTimestamp = firstPageContentView.Timestamp,
                    LastRequesTimestamp = lastPageContentView.Timestamp,
                    SessionId = firstPageContentView.SessionId,
                    SessionStartTime = firstPageView?.Timestamp ?? firstPageContentView.Timestamp
                };
                lockData.UserData.Add(resourceLockUserData);
            }

            return lockData;
        }

        private string BuildFullName(PageContentView pageContentView)
        {
            if (string.IsNullOrWhiteSpace(pageContentView.FirstName))
            {
                return string.IsNullOrWhiteSpace(pageContentView.LastName) ? "N/A" : pageContentView.LastName;
            }

            return string.IsNullOrWhiteSpace(pageContentView.LastName)
                ? pageContentView.FirstName
                : $"{pageContentView.LastName}, {pageContentView.FirstName}";
        }

        private InstitutionResourceLock InsertInstitutionResourceLocks(int institutionId, int? userId, int resourceId,
            DateTime minTimestamp, LockType lockType)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var resourcePrintLockData = GetResourceLockData(institutionId, resourceId, minTimestamp, lockType);
                    var lockDataJson = JsonConvert.SerializeObject(resourcePrintLockData);

                    _log.Debug($"lockDataJson: {lockDataJson}");

                    var institutionResourceLock = new InstitutionResourceLock
                    {
                        InstitutionId = institutionId,
                        UserId = userId,
                        ResourceId = resourceId,
                        LockType = lockType,
                        LockStartDate = DateTime.Now,
                        LockEndDate = DateTime.Now.AddHours(_webSettings.ResourcePrintLockDurationInHours),
                        LockData = lockDataJson
                    };

                    uow.SaveOrUpdate(institutionResourceLock);
                    uow.Commit();
                    transaction.Commit();
                    return institutionResourceLock;
                }
            }
        }

        private bool SendResourcePrintLockAlertEmail(InstitutionResourceLock institutionResourceLock,
            IResource resource)
        {
            try
            {
                var lockData = JsonConvert.DeserializeObject<ResourceLockData>(institutionResourceLock.LockData);

                var items = _resourceLockEmailBuildService.BuildResourcePrintLockEmailItems(lockData);

                EmailMessage emailMessage;
                var messageSentSuccessfully = true;
                var adminUsers = _userService.GetAdminUsers(institutionResourceLock.InstitutionId).ToList();
                foreach (var adminUser in adminUsers)
                {
                    _log.DebugFormat("adminUser.Email: {0}", adminUser.Email);
                    emailMessage =
                        _resourceLockEmailBuildService.BuildResourcePrintLockEmail(adminUser, resource, items);
                    messageSentSuccessfully =
                        _emailQueueService.QueueEmailMessage(emailMessage) && messageSentSuccessfully;
                }

                _log.DebugFormat("ResourcePrintAlertBcc: {0}", _webSettings.ResourcePrintAlertBcc);
                emailMessage = _resourceLockEmailBuildService.BuildResourcePrintLockInternalEmail(resource, items,
                    adminUsers, _webSettings.ResourcePrintAlertBcc);
                messageSentSuccessfully = _emailQueueService.QueueEmailMessage(emailMessage) && messageSentSuccessfully;
                return messageSentSuccessfully;
            }
            catch (Exception ex)
            {
                var msg = new StringBuilder()
                    .AppendLine("ERROR SENDING RESOURCE PRINT LOCK ALERT EMAIL")
                    .Append(institutionResourceLock.ToDebugString())
                    .AppendFormat("EXCEPTION MESSAGE: {0}", ex.Message).AppendLine()
                    .ToString();
                _log.Error(msg, ex);
                return false;
            }
        }

        private bool SendResourceEmailLockAlertEmail(InstitutionResourceLock institutionResourceLock,
            IResource resource)
        {
            try
            {
                var lockData = JsonConvert.DeserializeObject<ResourceLockData>(institutionResourceLock.LockData);

                var emailAddressCounts = _institutionResourceEmailDataService.GetEmailAddressCount(
                    institutionResourceLock.InstitutionId,
                    institutionResourceLock.ResourceId, _webSettings.ResourcePrintCheckPeriodInHours);

                var items = _resourceLockEmailBuildService.BuildResourceEmailLockEmailItems(lockData,
                    emailAddressCounts);

                //List<InstitutionResourceEmail> institutionResourceEmails = _institutionResourceEmailDataService.GetInstitutionResourceEmail(institutionResourceLock.InstitutionId,
                //    institutionResourceLock.ResourceId, _webSettings.ResourcePrintLockDurationInHours);

                EmailMessage emailMessage;
                var messageSentSuccessfully = true;
                var adminUsers = _userService.GetAdminUsers(institutionResourceLock.InstitutionId).ToList();
                foreach (var adminUser in adminUsers)
                {
                    _log.DebugFormat("adminUser.Email: {0}", adminUser.Email);
                    emailMessage =
                        _resourceLockEmailBuildService.BuildResourceEmailLockEmail(adminUser, resource, items);
                    messageSentSuccessfully =
                        _emailQueueService.QueueEmailMessage(emailMessage) && messageSentSuccessfully;
                }

                _log.DebugFormat("ResourcePrintAlertBcc: {0}", _webSettings.ResourcePrintAlertBcc);
                emailMessage = _resourceLockEmailBuildService.BuildResourceEmailLockInternalEmail(resource, items,
                    adminUsers, _webSettings.ResourcePrintAlertBcc);
                messageSentSuccessfully = _emailQueueService.QueueEmailMessage(emailMessage) && messageSentSuccessfully;
                return messageSentSuccessfully;
            }
            catch (Exception ex)
            {
                var msg = new StringBuilder()
                    .AppendLine("ERROR SENDING RESOURCE PRINT LOCK ALERT EMAIL")
                    .Append(institutionResourceLock.ToDebugString())
                    .AppendFormat("EXCEPTION MESSAGE: {0}", ex.Message).AppendLine()
                    .ToString();
                _log.Error(msg, ex);
                return false;
            }
        }
    }
}