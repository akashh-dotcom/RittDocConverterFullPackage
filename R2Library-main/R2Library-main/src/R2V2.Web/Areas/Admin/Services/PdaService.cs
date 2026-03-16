#region

using System;
using R2V2.Contexts;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;
using R2V2.Core.Institution;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Web.Areas.Admin.Services
{
    public class PdaService
    {
        private readonly IAdminContext _adminContext;
        private readonly AuthenticatedInstitution _authenticatedInstitution;
        private readonly ICartService _cartService;
        private readonly InstitutionService _institutionService;
        private readonly PatronDrivenAcquisitionService _patronDrivenAcquisitionService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public PdaService(
            InstitutionService institutionService
            , IAdminContext adminContext
            , IAuthenticationContext authenticationContext
            , IUnitOfWorkProvider unitOfWorkProvider
            , PatronDrivenAcquisitionService patronDrivenAcquisitionService
            , ICartService cartService)
        {
            _institutionService = institutionService;
            _adminContext = adminContext;
            _unitOfWorkProvider = unitOfWorkProvider;
            _patronDrivenAcquisitionService = patronDrivenAcquisitionService;
            _cartService = cartService;
            _authenticatedInstitution = authenticationContext.AuthenticatedInstitution;
        }

        public bool ShowPdaTrialConvert(IAdminInstitution adminInstitution,
            ICollectionManagementQuery collectionManagementQuery)
        {
            return adminInstitution.AccountStatus.Id == InstitutionAccountStatus.Trial.Id &&
                   !collectionManagementQuery.TrialConvert;
        }

        public bool ShowEula(IAdminInstitution adminInstitution, ICollectionManagementQuery collectionManagementQuery)
        {
            return !adminInstitution.IsEulaSigned && !collectionManagementQuery.EulaSigned;
        }

        public bool ShowPdaEula(IAdminInstitution adminInstitution,
            ICollectionManagementQuery collectionManagementQuery)
        {
            return !adminInstitution.IsPdaEulaSigned && !collectionManagementQuery.PdaEulaSigned;
        }

        public void ConvertAndSignEulasIfNeeded(IAdminInstitution adminInstitution,
            ICollectionManagementQuery collectionManagementQuery)
        {
            if (adminInstitution.AccountStatus.Id == InstitutionAccountStatus.Trial.Id &&
                collectionManagementQuery.TrialConvert)
            {
                ConvertTrial(collectionManagementQuery.InstitutionId);
            }

            if (!adminInstitution.IsEulaSigned && collectionManagementQuery.EulaSigned)
            {
                SignEula(collectionManagementQuery.InstitutionId);
            }

            if (!adminInstitution.IsPdaEulaSigned && collectionManagementQuery.PdaEulaSigned)
            {
                SignPdaEula(collectionManagementQuery.InstitutionId);
            }
        }

        public void ConvertTrial(int institutionId)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                var institution = _institutionService.GetInstitutionForEdit(institutionId);
                institution.AccountStatusId = (int)AccountStatus.Active;

                uow.SaveOrUpdate(institution);

                uow.Commit();

                _patronDrivenAcquisitionService.SendTrialEndedPdaCreated(institutionId);

                _adminContext.ReloadAdminInstitution(institutionId,
                    _authenticatedInstitution.User.Id); // reload cached institution after update
            }
        }

        public void SignEula(int institutionId)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                var institution = _institutionService.GetInstitutionForEdit(institutionId);
                institution.EULASigned = true;
                if (institution.AnnualFee == null)
                {
                    institution.AnnualFee = new AnnualFee();
                }

                institution.AnnualFee.FeeDate = DateTime.Now;

                uow.SaveOrUpdate(institution);

                uow.Commit();

                _cartService.RemoveCartsFromCache();
                _adminContext.ReloadAdminInstitution(institutionId,
                    _authenticatedInstitution.User.Id); // reload cached institution after update
            }
        }

        public void SignPdaEula(int institutionId)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                var institution = _institutionService.GetInstitutionForEdit(institutionId);
                institution.PdaEulaSigned = true;

                uow.SaveOrUpdate(institution);

                uow.Commit();

                _adminContext.ReloadAdminInstitution(institutionId,
                    _authenticatedInstitution.User.Id); // reload cached institution after update
            }
        }
    }
}