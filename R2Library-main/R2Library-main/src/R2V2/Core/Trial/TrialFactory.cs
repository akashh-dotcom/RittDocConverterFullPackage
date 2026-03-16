#region

using System;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.Institution;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Trial
{
    public class TrialFactory
    {
        private readonly ILog<TrialFactory> _log;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly UserOptionService _userOptionService;

        /// <summary>
        ///     Direct SQL calls to insert data into the Institution table and User table when a user is not authenticated.
        ///     I really don't like this. (SJS - 8/17/2012)  But with less than 14 hrs until launch, it will work.
        /// </summary>
        public TrialFactory(ILog<TrialFactory> log, IUnitOfWorkProvider unitOfWorkProvider,
            UserOptionService userOptionService)
        {
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
            _userOptionService = userOptionService;
        }

        public int SaveInstitution(IInstitution institution)
        {
            // SJS - 6/7/2013 - Need to verify we are using all the fields.
            //  iInstitutionId, vchInstitutionName, vchInstitutionAcctNum, vchInstitutionCity, dtTrialAcctStart, dtTrialAcctEnd, tiAllowPPV, tiDisplayPPV
            //, iInstitutionAcctStatusId, iAccessTypeId, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus, vchInstitutionAddr1
            //, vchInstitutionAddr2, vchInstitutionState, vchInstitutionZip, vchInstitutionContactPhone, tiEULASigned, decInstDiscount, dtAnnualFee
            //, vchAnnualFeePO, iAnnualFeePaytype, dtTrialEndEmailWarn, dtTrialEndEmail3DayWarn, dtTrialEndEmailFinal, tiHouseAcct, vchAthensOrgId
            //, vchLogUrl, tiHomePage, tiDemoMode, vchTrustedKey, dtPrecisionSearchExpireDate, tiAutoAddFreeLicenses, iTerritoryId, tiPdaEulaSigned
            var sql = new StringBuilder()
                .Append(
                    "insert into tInstitution (vchInstitutionName, vchInstitutionAcctNum, vchInstitutionCity, dtTrialAcctStart, dtTrialAcctEnd, tiAllowPPV ")
                .Append(
                    "    , tiDisplayPPV, iInstitutionAcctStatusId, iAccessTypeId, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate, tiRecordStatus ")
                .Append(
                    "    , vchInstitutionAddr1, vchInstitutionAddr2, vchInstitutionState, vchInstitutionZip, vchInstitutionContactPhone, tiEULASigned ")
                .Append(
                    "    , decInstDiscount, dtAnnualFee, vchAnnualFeePO, iAnnualFeePaytype, dtTrialEndEmailWarn, dtTrialEndEmail3DayWarn, dtTrialEndEmailFinal ")
                .Append(
                    "    , tiHouseAcct, vchAthensScopedAffiliation, vchLogUrl,tiHomePage, tiDemoMode, vchTrustedKey, dtPrecisionSearchExpireDate, tiAutoAddFreeLicenses ")
                .Append("    , iTerritoryId, tiPdaEulaSigned) ")
                .Append("values (:name, :acctNum, :city, :startDate, :endDate, 0 ")
                .Append("    , 0, :statusId, :accessTypeId, 'trial', getdate(), null, null, 1 ")
                .Append("    , :address1, :address2, :state, :zip, null, 0 ")
                .Append("    , :discount, null, null, null, null, null, null ")
                .Append("    , 0, null, null, :homePage, null, null, null, 0, :territoryId, 0); ")
                .Append("select @@identity;");

            _log.Debug(sql.ToString());

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql.ToString());

                query.SetParameter("name", institution.Name);
                query.SetParameter("acctNum", institution.AccountNumber);
                query.SetParameter("city", institution.Address.City);
                query.SetParameter("startDate", institution.Trial.StartDate);
                query.SetParameter("endDate", institution.Trial.EndDate);
                query.SetParameter("statusId", institution.AccountStatusId);
                query.SetParameter("accessTypeId", institution.AccessTypeId);
                query.SetParameter("address1", institution.Address.Address1);
                query.SetParameter("address2", institution.Address.Address2);
                query.SetParameter("state", institution.Address.State);
                query.SetParameter("zip", institution.Address.Zip);
                query.SetParameter("discount", institution.Discount);
                query.SetParameter("homePage", institution.HomePage.Id);

                if (institution.Territory == null)
                {
                    query.SetParameter("territoryId", null);
                }
                else
                {
                    query.SetParameter("territoryId", institution.Territory.Id);
                }

                var results = query.List();

                decimal id = 0;
                foreach (decimal result in results)
                {
                    id = result;
                }

                _log.DebugFormat("id: {0}", id);

                return decimal.ToInt32(id);
            }
        }

        public int SaveUser(User user)
        {
            var sql = new StringBuilder()
                .Append("insert into tUser (vchFirstName, vchLastName, vchUserName, vchUserPassword, vchUserEmail ")
                .Append("    , iInstitutionId, iRoleId, vchCreatorId, dtCreationDate, tiRecordStatus) ")
                .Append("values (:first, :last, :username, :password, :email ")
                .Append("    , :institutionId, :roleId, 'trial', getdate(), 1); ")
                .Append("select  CAST(@@identity AS INT);");

            _log.Debug(sql.ToString());

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql.ToString());

                query.SetParameter("first", user.FirstName);
                query.SetParameter("last", user.LastName);
                query.SetParameter("username", user.UserName);
                query.SetParameter("password", user.Password);
                query.SetParameter("email", user.Email);
                query.SetParameter("institutionId", user.Institution.Id);
                query.SetParameter("roleId", user.Role.Id);

                var results = query.List();

                var id = 0;
                foreach (int result in results)
                {
                    id = result;
                }

                _log.DebugFormat("id: {0}", id);

                return id;
            }
        }

        public bool SaveUserOptionValues(User user)
        {
            try
            {
                var sql = new StringBuilder()
                    .Append(
                        "INSERT INTO tUserOptionValue (iUserId, iUserOptionId, vchUserOptionValue, vchCreatorId, dtCreationDate, tiRecordStatus) ")
                    .Append("       select :userId, uo.iUserOptionId, uor.vchDefaultValue, 'trial', GETDATE(), 1 ")
                    .Append("       from tUserOption uo  ")
                    .Append("       join tUserOptionRole uor on uo.iUserOptionId = uor.iUserOptionId ")
                    .Append("       where uor.iRoleId = :roleId ")
                    .ToString();

                using (var uow = _unitOfWorkProvider.Start())
                {
                    var query = uow.Session.CreateSQLQuery(sql);

                    query.SetParameter("userId", user.Id);
                    query.SetParameter("roleId", user.Role.Id);

                    var results = query.List();

                    return true;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return false;
        }
    }
}