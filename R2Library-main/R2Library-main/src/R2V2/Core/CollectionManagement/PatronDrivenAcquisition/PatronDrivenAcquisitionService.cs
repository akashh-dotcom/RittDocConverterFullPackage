#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;
using R2V2.Core.Authentication;
using R2V2.Core.Email;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Email;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Settings;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.CollectionManagement.PatronDrivenAcquisition
{
    public class PatronDrivenAcquisitionService
    {
        public const string PatronDrivenAcquisitionResourceKey = "PatronDrivenAcquisition.Resource.Key";
        private readonly ICartService _cartService;
        private readonly ICollectionManagementSettings _collectionManagementSettings;
        private readonly EmailQueueService _emailQueueService;
        private readonly IEmailSettings _emailSettings;
        private readonly InstitutionResourceAuditFactory _institutionResourceAuditFactory;
        private readonly IQueryable<InstitutionResourceLicense> _institutionResourceLicenses;

        private readonly ILog<PatronDrivenAcquisitionService> _log;
        private readonly PdaEmailBuildService _pdaEmailBuildService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly IQueryable<User> _users;
        private readonly IUserSessionStorageService _userSessionStorageService;

        public PatronDrivenAcquisitionService(ILog<PatronDrivenAcquisitionService> log
            , IUnitOfWorkProvider unitOfWorkProvider
            , IQueryable<InstitutionResourceLicense> institutionResourceLicenses
            , IUserSessionStorageService userSessionStorageService
            , ICollectionManagementSettings collectionManagementSettings
            , PdaEmailBuildService pdaEmailBuildService
            , EmailQueueService emailQueueService
            , IQueryable<User> users
            , InstitutionResourceAuditFactory institutionResourceAuditFactory
            , ICartService cartService
            , IEmailSettings emailSettings
        )
        {
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
            _institutionResourceLicenses = institutionResourceLicenses;
            _userSessionStorageService = userSessionStorageService;
            _collectionManagementSettings = collectionManagementSettings;
            _pdaEmailBuildService = pdaEmailBuildService;
            _emailQueueService = emailQueueService;
            _users = users;
            _institutionResourceAuditFactory = institutionResourceAuditFactory;
            _cartService = cartService;
            _emailSettings = emailSettings;
        }

        public void AddPartonDrivenAcquisition(int resourceId, int institutionId, int userId)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    uow.IncludeSoftDeletedValues();
                    var pdaLicense = GetPartonDrivenAcquisitionLicense(resourceId, institutionId, userId);

                    var audit = _institutionResourceAuditFactory.BuildAuditRecord(
                        InstitutionResourceAuditType.PdaResourceAdded, institutionId, resourceId);

                    _log.Debug($"Inserting - {pdaLicense.ToDebugString()}");
                    _log.Debug($"Inserting - {audit.ToDebugString()}");
                    uow.Save(pdaLicense);
                    uow.Save(audit);

                    uow.ExcludeSoftDeletedValues();

                    uow.Commit();
                    transaction.Commit();
                }
            }
        }

        public InstitutionResourceLicense GetPartonDrivenAcquisitionLicense(int resourceId, int institutionId,
            int userId)
        {
            //Need to include all licenses
            var license =
                _institutionResourceLicenses.SingleOrDefault(x =>
                    x.InstitutionId == institutionId && x.ResourceId == resourceId); // &&
            //x.LicenseTypeId == (short) LicenseType.Pda);
            if (license != null)
            {
                if (license.LicenseTypeId == (int)LicenseType.Purchased && license.LicenseCount == 0)
                {
                    license.LicenseTypeId = (short)LicenseType.Pda;
                    license.OriginalSourceId = (short)LicenseOriginalSource.Pda;
                    license.FirstPurchaseDate = null;
                    license.PdaAddedDate = DateTime.Now;
                    license.PdaAddedToCartDate = null;
                    license.PdaAddedToCartById = null;
                    license.PdaViewCount = 0;
                    license.PdaMaxViews = _collectionManagementSettings.PatronDriveAcquisitionMaxViews;
                    license.RecordStatus = true;
                }

                license.RecordStatus = true;
                license.LicenseCount = 0;
                license.PdaDeletedDate = null;
                license.PdaDeletedById = null;
            }
            else
            {
                license = new InstitutionResourceLicense
                {
                    Id = 0,
                    InstitutionId = institutionId,
                    ResourceId = resourceId,
                    LicenseCount = 0,
                    LicenseTypeId = (short)LicenseType.Pda,
                    OriginalSourceId = (short)LicenseOriginalSource.Pda,
                    FirstPurchaseDate = null,
                    PdaAddedDate = DateTime.Now,
                    PdaAddedToCartDate = null,
                    PdaAddedToCartById = null,
                    PdaViewCount = 0,
                    PdaMaxViews = _collectionManagementSettings.PatronDriveAcquisitionMaxViews,
                    RecordStatus = true
                };
            }

            return license;
        }

        public void AddBuildPdaLicenses(IEnumerable<BulkAddResource> bulkAddResources, int institutionId, int userId)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    uow.IncludeSoftDeletedValues();

                    foreach (var bulkAddResource in bulkAddResources)
                    {
                        var pdaLicense =
                            GetPartonDrivenAcquisitionLicense(bulkAddResource.ResourceId, institutionId, userId);
                        _log.Debug($"Inserting - {pdaLicense.ToDebugString()}");

                        var audit = _institutionResourceAuditFactory.BuildAuditRecord(
                            InstitutionResourceAuditType.PdaResourceAdded, institutionId, bulkAddResource.ResourceId);
                        _log.Debug($"Inserting - {audit.ToDebugString()}");
                        uow.SaveOrUpdate(pdaLicense);
                        uow.Save(audit);
                    }

                    uow.ExcludeSoftDeletedValues();

                    uow.Commit();
                    transaction.Commit();
                }
            }
        }


        public bool DeletePartonDrivenAcquisition(int resourceId, int institutionId, IUser user)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var pdaLicense =
                        _institutionResourceLicenses.SingleOrDefault(x =>
                            x.InstitutionId == institutionId && x.ResourceId == resourceId &&
                            x.LicenseTypeId == (short)LicenseType.Pda);
                    if (pdaLicense != null)
                    {
                        pdaLicense.PdaDeletedDate = DateTime.Now;
                        pdaLicense.PdaDeletedById = $"user id: {user.Id}, [{user.FirstName}]";
                        _log.Debug($"Deleting - {pdaLicense.ToDebugString()}");
                        uow.Save(pdaLicense);

                        var audit = _institutionResourceAuditFactory.BuildAuditRecord(
                            InstitutionResourceAuditType.PdaResourceDeleted, institutionId, resourceId);

                        uow.Save(audit);
                        uow.Commit();
                        transaction.Commit();
                        return true;
                    }

                    return false;
                }
            }
        }

        /// <summary>
        ///     If it is not a PDA item nothing will happen, else
        /// </summary>
        public bool DeletePartonDrivenAcquisitionFromCart(int resourceId, int institutionId, IUser user)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var pdaLicense2 =
                        _institutionResourceLicenses.SingleOrDefault(x =>
                            x.InstitutionId == institutionId && x.ResourceId == resourceId &&
                            x.LicenseTypeId == (short)LicenseType.Pda && x.PdaAddedToCartDate != null);
                    if (pdaLicense2 != null)
                    {
                        pdaLicense2.PdaCartDeletedDate = DateTime.Now;
                        pdaLicense2.PdaCartDeletedById = user.Id;
                        pdaLicense2.PdaCartDeletedByName = $"{user.FirstName} {user.LastName}";

                        _log.Debug($"Deleting - {pdaLicense2.ToDebugString()}");
                        uow.Save(pdaLicense2);

                        var audit2 =
                            _institutionResourceAuditFactory.BuildAuditRecord(
                                InstitutionResourceAuditType.PdaResourceDeletedFromCart,
                                institutionId, resourceId);

                        uow.Save(audit2);
                        uow.Commit();
                        transaction.Commit();
                    }

                    return true;
                }
            }
        }

        public void DeletePartonDrivenAcquisitionsFromCart(int cartId, int institutionId, IUser user)
        {
            var cart = _cartService.GetInstitutionCartFromDatabase(institutionId, cartId);

            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    if (cart.CartItems.Any(x => x.ResourceId.HasValue))
                    {
                        var pdaLicenses =
                            _institutionResourceLicenses.Where(x =>
                                    x.InstitutionId == institutionId &&
                                    x.LicenseTypeId == (short)LicenseType.Pda && x.PdaAddedToCartDate != null)
                                .ToList();

                        foreach (var cartItem in cart.CartItems.Where(x => x.ResourceId.HasValue))
                        {
                            var pdaLicense = pdaLicenses.SingleOrDefault(x => x.ResourceId == cartItem.ResourceId);
                            if (pdaLicense != null)
                            {
                                pdaLicense.PdaCartDeletedDate = DateTime.Now;
                                pdaLicense.PdaCartDeletedById = user.Id;
                                pdaLicense.PdaCartDeletedByName =
                                    $"{user.FirstName} {user.LastName}";

                                _log.Debug($"Deleting - {pdaLicense.ToDebugString()}");
                                uow.Save(pdaLicense);

                                var audit =
                                    _institutionResourceAuditFactory.BuildAuditRecord(
                                        InstitutionResourceAuditType.PdaResourceDeletedFromCart,
                                        institutionId, cartItem.ResourceId.GetValueOrDefault(0));

                                uow.Save(audit);
                            }
                        }

                        uow.Commit();
                        transaction.Commit();
                    }
                }
            }
        }

        /// <summary>
        ///     Deletes an Array of PDA licenses from an institution
        /// </summary>
        public bool BulkDeletePdaLicenses(int[] resourceIds, int institutionId, IUser user)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var pdaLicenses =
                        _institutionResourceLicenses.Where(x =>
                            x.InstitutionId == institutionId && resourceIds.Contains(x.ResourceId) &&
                            x.LicenseTypeId == (short)LicenseType.Pda);
                    if (pdaLicenses.Any())
                    {
                        foreach (var pdaLicense in pdaLicenses)
                        {
                            pdaLicense.PdaDeletedDate = DateTime.Now;
                            pdaLicense.PdaDeletedById = $"user id: {user.Id}, [{user.FirstName}]";

                            _log.Debug($"Deleting - {pdaLicense.ToDebugString()}");
                            uow.Save(pdaLicense);

                            var audit =
                                _institutionResourceAuditFactory.BuildAuditRecord(
                                    InstitutionResourceAuditType.PdaResourceDeleted,
                                    institutionId, pdaLicense.ResourceId);

                            uow.Save(audit);
                        }

                        uow.Commit();
                        transaction.Commit();
                        return true;
                    }

                    return false;
                }
            }
        }

        /// <summary>
        ///     This will update the PDA license view count. Only one update per user session.
        ///     So is a user views the titles 3 times in a session, it only counts as a single view.
        /// </summary>
        public bool UpdatePartonDrivenAcquisitionView(IResource resource,
            AuthenticatedInstitution authenticatedInstitution)
        {
            var pdaResourceIds = GetListOfPdaResources();
            if (!pdaResourceIds.Contains(resource.Id))
            {
                AddPartonDrivenAcquisitionView(resource, authenticatedInstitution.Id,
                    authenticatedInstitution.User == null ? 0 : authenticatedInstitution.User.Id);
                pdaResourceIds.Add(resource.Id);
                return true;
            }

            return false;
        }

        private List<int> GetListOfPdaResources()
        {
            var pdaResourceIds = _userSessionStorageService.Get<List<int>>(PatronDrivenAcquisitionResourceKey);
            if (pdaResourceIds == null)
            {
                pdaResourceIds = new List<int>();
                _userSessionStorageService.Put(PatronDrivenAcquisitionResourceKey, pdaResourceIds);
            }

            return pdaResourceIds;
        }

        private void AddPartonDrivenAcquisitionView(IResource resource, int institutionId, int userId)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                var pdaLicense =
                    _institutionResourceLicenses.SingleOrDefault(x =>
                        x.InstitutionId == institutionId && x.ResourceId == resource.Id &&
                        x.LicenseTypeId == (short)LicenseType.Pda);
                if (pdaLicense == null)
                {
                    _log.Error(
                        $"AddPartonDrivenAcquisitionView(resourceId: {resource.Id}, institutionId; {institutionId}, userId: {userId}) - PDA RESOURCE NOT FOUND!!!");
                    return;
                }


                pdaLicense.PdaViewCount = pdaLicense.PdaViewCount + 1;
                _log.Debug(
                    $"AddPartonDrivenAcquisitionView(resourceId: {resource.Id}, institutionId; {institutionId}, userId: {userId}) - PdaViewCount: {pdaLicense.PdaViewCount}");

                if (pdaLicense.PdaViewCount == pdaLicense.PdaMaxViews)
                {
                    // add resource as PDA and send email to
                    if (resource.NotSaleableDate == null)
                    {
                        _cartService.AddItemToCart(institutionId, resource.Id, 1, LicenseOriginalSource.Pda, 0);
                        pdaLicense.PdaAddedToCartById = "";
                        pdaLicense.PdaAddedToCartDate = DateTime.Now;
                    }
                }

                var audit = _institutionResourceAuditFactory.BuildAuditRecord(
                    InstitutionResourceAuditType.PdaResourceView, institutionId,
                    resource.Id);
                _log.Debug("AddPartonDrivenAcquisitionView");
                _log.Debug($"Inserting - {audit.ToDebugString()}");

                uow.Save(pdaLicense);
                uow.Save(audit);
                uow.Commit();
            }
        }

        /// <summary>
        ///     This method is used to determine if a resource was already accessed as a PDA resource previously within the
        ///     session.
        ///     This is needed so that when a user views a PDA resource for the 3rd time, the user can still access the resource
        ///     after viewing other resources.
        /// </summary>
        public bool WasPdaResourcePreviouslyViewedThisSession(int resourceId)
        {
            var pdaResourceIds = GetListOfPdaResources();
            return pdaResourceIds != null && pdaResourceIds.Contains(resourceId);
        }

        public void SendTrialEndedPdaCreated(int institutionId)
        {
            try
            {
                var users = _users.Fetch(x => x.Institution)
                    .Where(x => x.InstitutionId == institutionId && x.Role.Code == RoleCode.INSTADMIN)
                    .ToList();
                foreach (var user in users)
                {
                    var emailMessage = _pdaEmailBuildService.BuildTrialEndedPdaCreatedEmail(user);
                    if (_emailSettings.SendToCustomers)
                    {
                        _emailQueueService.QueueEmailMessage(emailMessage);
                    }
                    else
                    {
                        var toAddresses = new StringBuilder();
                        foreach (var recipient in emailMessage.ToRecipients)
                        {
                            toAddresses.AppendFormat("{0}{1}", toAddresses.Length > 0 ? ";" : string.Empty, recipient);
                        }

                        _log.Warn(
                            $"Message note send, SendToCustomers is set to 'false'. To: {toAddresses}, Subject: {emailMessage.Subject}");
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }
    }
}