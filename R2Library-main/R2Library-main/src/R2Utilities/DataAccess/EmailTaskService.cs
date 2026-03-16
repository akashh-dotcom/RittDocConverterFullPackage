#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;
using NHibernate.Transform;
using NHibernate.Util;
using R2Utilities.Infrastructure.Settings;
using R2V2.Core.Admin;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.CollectionManagement.PatronDrivenAcquisition;
using R2V2.Core.Institution;
using R2V2.Core.Recommendations;
using R2V2.Core.Reports;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Territory;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2Utilities.DataAccess
{
    public class EmailTaskService : R2UtilitiesBase
    {
        /// <summary>
        ///     <para>The replacements in this string are the following:</para>
        ///     <para>{0} = iCartId</para>
        ///     <para>{1} = iResourceId</para>
        ///     <para>{2} = decListPrice</para>
        ///     <para>{3} = decDiscountPrice</para>
        ///     <para>{4} = iResourceInstLicenseId</para>
        /// </summary>
        private static readonly string CartItemInsert = new StringBuilder()
            .Append("INSERT INTO [dbo].[tCartItem] ")
            .Append(
                "([iCartId], [iResourceId], [iNumberOfLicenses], [vchCreatorId], [dtCreationDate], [tiRecordStatus], [decListPrice] ")
            .Append(
                ", [decDiscountPrice], [tiInclude], [tiAgree], [tiLicenseOriginalSourceId], [dtAddedByNewEdition]) ")
            .Append("VALUES")
            .Append("({0}, {1}, 1, 'NewEditionTask', getdate(), 1, {2} ")
            .Append(", {3}, 1, 0, 1, getdate());")
            .ToString();

        /// <summary>
        ///     <para>The replacements in this string are the following:</para>
        ///     <para>{0} = iInstitutionId</para>
        ///     <para>{1} = decInstDiscount</para>
        /// </summary>
        private static readonly string CartInsert = new StringBuilder()
            .Append("INSERT INTO [dbo].[tCart] ")
            .Append("([iInstitutionId], [tiProcessed], [vchCreatorId], [dtCreationDate] ")
            .Append(", [tiRecordStatus], [decInstDiscount], [decPromotionDiscount]) ")
            .Append("VALUES({0}, 0, 'NewEditionTask', getdate() ")
            .Append(", 1, {1}, 0.00); ")
            .Append("SELECT SCOPE_IDENTITY()")
            .ToString();

        private readonly IQueryable<Cart> _carts;
        private readonly IQueryable<IFeaturedTitle> _featuredTitles;
        private readonly IQueryable<InstitutionResourceLicense> _institutionResourceLicenses;
        private readonly IQueryable<Institution> _institutions;
        private readonly ILog<EmailTaskService> _log;
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly IQueryable<Recommendation> _recommendations;
        private readonly ReportService _reportService;
        private readonly ResourceDiscountService _resourceDiscountService;
        private readonly IQueryable<IResource> _resources;
        private readonly IQueryable<SpecialResource> _specialResources;
        private readonly IQueryable<Specialty> _specialties;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IQueryable<User> _users;
        private readonly IQueryable<UserTerritory> _userTerritories;


        public EmailTaskService(
            IQueryable<User> users
            , IQueryable<Cart> carts
            , IQueryable<IResource> resources
            , IQueryable<InstitutionResourceLicense> institutionResourceLicenses
            , ReportService reportService
            , IUnitOfWork unitOfWork
            , ILog<EmailTaskService> log
            , IR2UtilitiesSettings r2UtilitiesSettings
            , IQueryable<UserTerritory> userTerritories
            , IQueryable<Institution> institutions
            , IQueryable<FeaturedTitle> featuredTitles
            , IQueryable<SpecialResource> specialResources
            , IQueryable<Recommendation> recommendations
            , IQueryable<Specialty> specialties
            , ResourceDiscountService resourceDiscountService
        )
        {
            _users = users;
            _carts = carts;
            _resources = resources;
            _institutionResourceLicenses = institutionResourceLicenses;
            _reportService = reportService;
            _unitOfWork = unitOfWork;
            _log = log;
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _userTerritories = userTerritories;
            _institutions = institutions;
            _featuredTitles = featuredTitles;
            _specialResources = specialResources;
            _recommendations = recommendations;
            _specialties = specialties;
            _resourceDiscountService = resourceDiscountService;
        }

        private Dictionary<string, bool> IsbnDictionary { get; set; }

        public List<User> GetUsersForNewResourceEmail()
        {
            return GetBaseUsers()
                .Where(x => x.RecordStatus && (x.ExpirationDate == null || x.ExpirationDate > DateTime.Now) &&
                            //Makes Sure the Instiuttion is Active or not an expired trial
                            x.Institution.RecordStatus &&
                            (x.Institution.AccountStatusId == (int)AccountStatus.Active
                             ||
                             x.Institution.AccountStatusId == (int)AccountStatus.Trial &&
                             x.Institution.Trial.EndDate > DateTime.Now
                            ))
                .HasOptionSelected(UserOptionCode.NewResource)
                .ToList();
        }

        public List<User> GetUsersForPurchasedForthcomingEmail()
        {
            return GetBaseUsers()
                .Where(x => x.Role.Code == RoleCode.INSTADMIN
                            && x.RecordStatus && (x.ExpirationDate == null || x.ExpirationDate > DateTime.Now) &&
                            //Makes Sure the Instiuttion is Active or not an expired trial
                            x.Institution.RecordStatus &&
                            (
                                x.Institution.AccountStatusId == (int)AccountStatus.Active
                                ||
                                x.Institution.AccountStatusId == (int)AccountStatus.Trial &&
                                x.Institution.Trial.EndDate > DateTime.Now
                            )
                )
                .HasOptionSelected(UserOptionCode.ForthcomingPurchase)
                .ToList();
        }

        public List<User> GetUsersForTurnawayEmail()
        {
            return GetBaseUsers()
                .Where(x =>
                    x.Role.Code == RoleCode.INSTADMIN
                    && x.RecordStatus && (x.ExpirationDate == null || x.ExpirationDate > DateTime.Now) &&
                    //Makes Sure the Instiuttion is Active or not an expired trial
                    x.Institution.RecordStatus &&
                    (x.Institution.AccountStatusId == (int)AccountStatus.Active
                     ||
                     x.Institution.AccountStatusId == (int)AccountStatus.Trial &&
                     x.Institution.Trial.EndDate > DateTime.Now)
                )
                .HasOptionSelected(UserOptionCode.AccessDenied)
                .ToList();
        }

        public List<User> GetUsersForNewEditionEmail()
        {
            return GetBaseUsers()
                .Where(x => x.RecordStatus && (x.ExpirationDate == null || x.ExpirationDate > DateTime.Now) &&
                            //Makes Sure the Instiuttion is Active or not an expired trial
                            x.Institution.RecordStatus &&
                            (
                                x.Institution.AccountStatusId == (int)AccountStatus.Active
                                ||
                                x.Institution.AccountStatusId == (int)AccountStatus.Trial &&
                                x.Institution.Trial.EndDate > DateTime.Now
                            )
                )
                .HasOptionSelected(UserOptionCode.NewEdition)
                .ToList();
        }

        public List<User> GetUsersForDctUpdateEmails(int practiceAreaId)
        {
            switch (practiceAreaId)
            {
                case 1:
                    return
                        GetBaseUsers()
                            .Where(x => x.RecordStatus &&
                                        (x.ExpirationDate == null || x.ExpirationDate > DateTime.Now) &&
                                        x.Institution.RecordStatus &&
                                        (x.Institution.AccountStatusId == (int)AccountStatus.Active ||
                                         x.Institution.AccountStatusId == (int)AccountStatus.Trial &&
                                         x.Institution.Trial.EndDate > DateTime.Now))
                            .HasOptionSelected(UserOptionCode.DctMedical)
                            .ToList();
                case 2:
                    return
                        GetBaseUsers()
                            .Where(x => x.RecordStatus &&
                                        (x.ExpirationDate == null || x.ExpirationDate > DateTime.Now) &&
                                        x.Institution.RecordStatus &&
                                        (x.Institution.AccountStatusId == (int)AccountStatus.Active ||
                                         x.Institution.AccountStatusId == (int)AccountStatus.Trial &&
                                         x.Institution.Trial.EndDate > DateTime.Now))
                            .HasOptionSelected(UserOptionCode.DctNursing)
                            .ToList();
                case 3:
                    return
                        GetBaseUsers()
                            .Where(x => x.RecordStatus &&
                                        (x.ExpirationDate == null || x.ExpirationDate > DateTime.Now) &&
                                        x.Institution.RecordStatus &&
                                        (x.Institution.AccountStatusId == (int)AccountStatus.Active ||
                                         x.Institution.AccountStatusId == (int)AccountStatus.Trial &&
                                         x.Institution.Trial.EndDate > DateTime.Now))
                            .HasOptionSelected(UserOptionCode.DctAlliedHealth)
                            .ToList();
            }

            return null;
        }

        public Cart GetCartForShoppingCartTask(int cartId)
        {
            return _carts.FirstOrDefault(x => x.Id == cartId);
        }

        public List<Cart> GetSavedCartsForInstitution(int institutionId)
        {
            return _carts.Where(x =>
                x.InstitutionId == institutionId && !x.Processed && x.CartType == CartTypeEnum.Saved).ToList();
        }

        public AdminInstitution GetAdminInstitution(int institutionId)
        {
            var institution = _institutions.FirstOrDefault(x => x.Id == institutionId);
            return new AdminInstitution(institution);
        }

        public IResource GetResource(int resourceId)
        {
            return _resources.FirstOrDefault(x => x.Id == resourceId);
        }

        public List<TurnawayResource> GetInstitutionTurnaways(string reportDatabaseName, string r2DatabaseName)
        {
            var turnawayResources = _reportService.GetTurnawayResources2(reportDatabaseName, r2DatabaseName);

            foreach (var turnawayResource in turnawayResources)
            {
                turnawayResource.Resource = GetResource(turnawayResource.ResourceId);
            }

            return turnawayResources;
        }

        public Dictionary<string, bool> GetResourceEmailResources(string emailType)
        {
            var sql = new StringBuilder()
                .Append("select r.vchResourceISBN ")
                .Append(", Cast((case when re.id is null then 0 else 1 end)as tinyint) as Found ")
                .Append("from tResource r ")
                .AppendFormat("left outer join [{0}].[dbo].[ResourceEmails] re on r.vchResourceISBN = re.resourceIsbn ",
                    _r2UtilitiesSettings.R2UtilitiesDatabaseName)
                .Append("where r.iResourceStatusId = 6 and r.tiRecordStatus = 1 ")
                //.Append("and r.dtLastPromotionDate > GETDATE() -7 ")
                .AppendFormat("and (re.id is null or re.{0} is null) ", emailType)
                .Append("order by r.iResourceId asc ")
                .ToString();
            var query = _unitOfWork.Session.CreateSQLQuery(sql).List();

            return query.Cast<object[]>().ToDictionary(pair => pair[0].ToString(), pair => pair[1].ToString() == "1");
        }

        private List<string> GetDctUpdateResources(int practiceAreaId)
        {
            var sql = new StringBuilder()
                .Append(" select r.vchResourceISBN ")
                .Append(" from tResource r ")
                .Append(" join tPublisher p on r.iPublisherId = r.iPublisherId and p.tiRecordStatus = 1 ")
                .Append(
                    " join tResourcePracticeArea rpa on r.iResourceId = rpa.iResourceId and rpa.tiRecordStatus = 1 ")
                .AppendFormat(" join {0}..ResourceEmails re on r.vchResourceISBN = re.resourceISBN ",
                    _r2UtilitiesSettings.R2UtilitiesDatabaseName)
                .AppendFormat(" where rpa.iPracticeAreaId = {0} ", practiceAreaId)
                .AppendFormat(" and re.dateNewResourceEmail between (GETDATE()-{0}) and GETDATE() ",
                    _r2UtilitiesSettings.DctUpdateEmailStartDaysAgo)
                .Append(" and r.iDCTStatusId in (158,159) ")
                .Append(" group by r.vchResourceISBN ")
                .ToString();

            var query = _unitOfWork.Session.CreateSQLQuery(sql).List();

            return query.Cast<string>().ToList();
        }


        private void UpdateInsertResourceEmailResources(string emailType)
        {
            if (IsbnDictionary == null)
            {
                IsbnDictionary = GetResourceEmailResources(emailType);
            }

            var insertSql =
                string.Format(
                    "Insert INTO [{1}].[dbo].[ResourceEmails] ([resourceISBN], [{0}]) VALUES ('[ISBN]', GetDate())",
                    emailType, _r2UtilitiesSettings.R2UtilitiesDatabaseName);
            var updateSql =
                string.Format("Update [{1}].[dbo].[ResourceEmails] set {0} = GetDate() Where [resourceISBN] = '[ISBN]'",
                    emailType, _r2UtilitiesSettings.R2UtilitiesDatabaseName);

            var sql = new StringBuilder();
            //-------------UPDATE-------------
            foreach (var pair in IsbnDictionary.Where(x => x.Value))
            {
                sql.Append($"{updateSql}; ".Replace("[ISBN]", pair.Key));
            }

            //-------------INSERT-------------
            foreach (var pair in IsbnDictionary.Where(x => !x.Value))
            {
                sql.Append($"{insertSql}; ".Replace("[ISBN]", pair.Key));
            }

            //Need to add the .List() to make the query Fire. There may be a better way to run this, but I could not find the
            if (sql.Length > 0)
            {
                _unitOfWork.Session.CreateSQLQuery(sql.ToString()).List();
                _log.DebugFormat("UpdateInsertResourceEmailResources {0}", emailType);
                _log.DebugFormat("sql : [[{0}]]", sql.ToString());
            }
        }

        public List<IResource> GetNewResourceEmailResources()
        {
            try
            {
                if (IsbnDictionary == null || IsbnDictionary.Count == 0)
                {
                    IsbnDictionary = GetResourceEmailResources("dateNewResourceEmail");
                }

                var resources = _resources.Where(x => IsbnDictionary.Keys.Contains(x.Isbn)).ToList();

                var sortedResources = resources.OrderBy(x =>
                {
                    var firstOrDefault = x.Specialties.OrderBy(y => y.Name).FirstOrDefault();
                    return firstOrDefault != null
                        ? x.Specialties != null
                            ? firstOrDefault.Name
                            : "aaaaa"
                        : null;
                }).ToList();
                return sortedResources;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }

        public List<IResource> GetDctUpdateResourcesForEmail(int practiceAreaId)
        {
            var isbns = GetDctUpdateResources(practiceAreaId);

            var query = from r in _resources where isbns.Contains(r.Isbn) select r;

            //Used for testing
            //query = _resources.Where(x => x.Id == 1523 || x.Id == 1845);

            return query.ToList();
        }


        public List<IResource> GetPurchasedResourceEmailResources(int institutionId)
        {
            try
            {
                if (IsbnDictionary == null)
                {
                    IsbnDictionary = GetResourceEmailResources("datePurchasedEmail");
                }


                var query = from irl in _institutionResourceLicenses
                    join r in _resources on irl.ResourceId equals r.Id
                    where irl.InstitutionId == institutionId && IsbnDictionary.Keys.Contains(r.Isbn) &&
                          irl.LicenseTypeId != (int)LicenseType.Pda
                    select r;

                var resources = query.ToList();

                var sortedResources = resources.OrderBy(x =>
                {
                    var firstOrDefault = x.Specialties.OrderBy(y => y.Name).FirstOrDefault();
                    return firstOrDefault != null
                        ? x.Specialties != null
                            ? firstOrDefault.Name
                            : "z"
                        : null;
                }).ToList();

                return sortedResources;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }

        public List<IResource> GetNewEditionResourceEmailResources(int institutionId)
        {
            try
            {
                if (IsbnDictionary == null)
                {
                    IsbnDictionary = GetResourceEmailResources("dateNewEditionEmail");
                }
                //TODO: Case #1 They Own the previous edition
                //TODO: Case #2 They own the previous previous edition
                //TODO: Case #3 They PDA the previous edition               9/27/2024 STOPPED adding PDA titles to cart
                //TODO: Case #4 They PDA the previous previous edition      9/27/2024 STOPPED adding PDA titles to cart

                //Gets all new edition resources based on there purchased resources
                var query = from r in _resources
                    join oldR in _resources on r.Id equals oldR.LatestEditResourceId
                    join irl in _institutionResourceLicenses on oldR.Id equals irl.ResourceId
                    //TODO: 2/13/2020 SquishList #1153
                    where irl.InstitutionId == institutionId && IsbnDictionary.Keys.Contains(r.Isbn) &&
                          irl.LicenseTypeId == (int)LicenseType.Purchased
                          && irl.OriginalSourceId == (int)LicenseOriginalSource.FirmOrder
                    select r;

                var resources = query.Distinct().ToList();

                //TODO: Check licenses to make sure they do not already own the new edition.
                var resourcesWithoutLicense = new List<IResource>();

// ReSharper disable once ImplicitlyCapturedClosure
                var institutionResourceLicenses =
                    _institutionResourceLicenses.Where(x => x.InstitutionId == institutionId);

                foreach (var resource in resources)
                {
                    var license = institutionResourceLicenses.FirstOrDefault(x =>
                        x.ResourceId == resource.Id //&& x.FirstPurchaseDate != null
                        &&
                        (x.LicenseTypeId == (int)LicenseType.Purchased
                         ||
                         x.LicenseTypeId == (int)LicenseType.Pda
                        )
                    );
                    if (license == null)
                    {
                        resourcesWithoutLicense.Add(resource);
                    }
                }

                //TODO: Make sure it is not already in Active cart.
                var resourcesWithoutLicenseNotInCart = new List<IResource>();
                var cart = _carts.FetchMany(x => x.CartItems).SingleOrDefault(x =>
                    x.InstitutionId == institutionId && !x.Processed && x.CartType == CartTypeEnum.Active);
                if (cart != null)
                {
                    foreach (var resource in resourcesWithoutLicense)
                    {
                        var cartItem = cart.CartItems.FirstOrDefault(x => x.ResourceId == resource.Id);
                        if (cartItem == null)
                        {
                            resourcesWithoutLicenseNotInCart.Add(resource);
                        }
                    }
                }


                var sortedResources = !resourcesWithoutLicenseNotInCart.Any()
                    ? null
                    : resourcesWithoutLicenseNotInCart.OrderBy(x =>
                    {
                        var firstOrDefault = x.Specialties.OrderBy(y => y.Name).FirstOrDefault();
                        return firstOrDefault != null
                            ? x.Specialties != null
                                ? firstOrDefault.Name
                                : "z"
                            : null;
                    }).ToList();
                return sortedResources;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }

        public void UpdateNewResourceEmailResources()
        {
            UpdateInsertResourceEmailResources("dateNewResourceEmail");
        }

        public void UpdatePurchasedResourceEmailResources()
        {
            UpdateInsertResourceEmailResources("datePurchasedEmail");
        }

        public void UpdateNewEditionResourceEmailResources()
        {
            UpdateInsertResourceEmailResources("dateNewEditionEmail");
        }

        public List<int> GetPdaAddedToCartUserIds()
        {
            var sql = new StringBuilder()
                .Append(" select u.iUserId ")
                .Append(PdaAddedTables())
                .Append(" group by  u.iUserId ")
                .ToString();

            var query = _unitOfWork.Session.CreateSQLQuery(sql).List();
            return query.Cast<int>().ToList();
        }

        public List<PdaResource> GetPdaAddedToCartResources(int userId)
        {
            var sql = new StringBuilder()
                .Append(
                    "select r.iResourceId as [ResourceId], irl.dtCreationDate as [AddedDate], irl.dtPdaAddedToCartDate as [AddedToCartDate]")
                .Append(
                    " , ci.vchSpecialText as [PromotionText], ci.decDiscountPrice as [DiscountPrice], ci.decListPrice as [ListPrice]  ")
                .Append(PdaAddedTables())
                .AppendFormat(" and u.iUserId = {0} ", userId)
                .Append(
                    "group by r.iResourceId, irl.dtPdaAddedToCartDate, irl.dtCreationDate, ci.vchSpecialText, ci.decDiscountPrice, ci.decListPrice ")
                .ToString();

            IList<PdaResource> pdaResources = GetEntityList<PdaResource>(sql, null, true);
            return pdaResources.ToList();
        }

        public int InsertPdaAddedToCartResourceEmail(int userId)
        {
            var sql = new StringBuilder()
                .AppendFormat("Insert Into {0}..PdaResourceEmails (userid, resourceIsbn, dateEmailSent, type) ",
                    _r2UtilitiesSettings.R2UtilitiesDatabaseName)
                .Append("select u.iUserId, r.vchResourceISBN, getdate(), '1' ")
                .Append(PdaAddedTables())
                .AppendFormat(" and u.iUserId = {0} ", userId)
                .Append("group by u.iUserId, r.vchResourceISBN ")
                .ToString();

            var rows = ExecuteInsertStatementReturnRowCount(sql, null, true);
            return rows;
        }

        /// <summary>
        ///     Gives you all the tables and a where clause for selecting the resources or the userids for PDA Resources Added
        /// </summary>
        private string PdaAddedTables()
        {
            var formatted =
                $"{DateTime.Now.AddDays(-_r2UtilitiesSettings.PdaAddedToCartNumberOfDaysAgo).ToShortDateString()} 00:00:00";

            return new StringBuilder()
                .Append("           from tUser u ")
                .Append(
                    "           join tUserOptionValue uov on u.iUserId = uov.iUserId and uov.tiRecordStatus = 1 and uov.vchUserOptionValue = '1' ")
                .Append(
                    "           join tUserOption uo on uov.iUserOptionId = uo.iUserOptionId and uo.tiRecordStatus = 1")
                .Append(
                    "           join tUserOptionRole uor on uo.iUserOptionId = uor.iUserOptionId and u.iRoleId = uor.iRoleId and uor.tiRecordStatus = 1")
                .Append(
                    "           join tInstitution i on u.iInstitutionId = i.iInstitutionId and i.tiRecordStatus = 1 ")
                .Append("           join tCart c on u.iInstitutionId = c.iInstitutionId and c.tiProcessed = 0 ")
                .AppendFormat(
                    "     join tCartItem ci on c.iCartId = ci.iCartId and ci.tiRecordStatus = 1 and ci.tiLicenseOriginalSourceId = {0} ",
                    (int)LicenseOriginalSource.Pda)
                .Append("           join tResource r on ci.iResourceId = r.iResourceId ")
                .AppendFormat(
                    "     join tInstitutionResourceLicense irl on i.iInstitutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 and irl.tiLicenseOriginalSourceId = {0} and r.iResourceId = irl.iResourceId",
                    (int)LicenseOriginalSource.Pda)
                .AppendFormat(
                    "         left outer join {0}..PdaResourceEmails pre on r.vchResourceISBN = pre.resourceIsbn and pre.type = 1 and pre.userId = u.iUserId ",
                    _r2UtilitiesSettings.R2UtilitiesDatabaseName)
                .AppendFormat(
                    "     where irl.dtPdaAddedToCartDate > '{0}' and uov.iUserOptionId = {1}  and pre.id is null",
                    formatted, (int)UserOptionCode.PdaAddToCart)
                .Append(
                    "           and u.tiRecordStatus = 1 and i.tiRecordStatus = 1 and (u.dtExpirationDate is null or u.dtExpirationDate > getdate()) ")
                .AppendFormat(
                    "     and (i.iInstitutionAcctStatusId = {0} or (i.iInstitutionAcctStatusId = {1} and i.dtTrialAcctEnd > GETDATE() ) ) ",
                    (int)AccountStatus.Active, (int)AccountStatus.Trial)
                .ToString();
        }

        public List<int> GetPdaRemovedFromCartUserIds()
        {
            var sql = new StringBuilder()
                .Append(" select u.iUserId ")
                .Append(PdaRemovedTables())
                .Append(" group by  u.iUserId ")
                .ToString();

            var query = _unitOfWork.Session.CreateSQLQuery(sql).List();
            return query.Cast<int>().ToList();
        }

        public List<PdaResource> GetPdaRemovedFromCartResources(int userId)
        {
            var sql = new StringBuilder()
                .Append(
                    "select r.iResourceId as [ResourceId], irl.dtCreationDate as [AddedDate], irl.dtPdaAddedToCartDate as [AddedToCartDate] ")
                .Append(
                    " , ci.vchSpecialText as [PromotionText], ci.decDiscountPrice as [DiscountPrice], ci.decListPrice as [ListPrice]  ")
                .Append(PdaRemovedTables())
                .AppendFormat(" and u.iUserId = {0} ", userId)
                .Append(
                    "group by r.iResourceId, irl.dtPdaAddedToCartDate, irl.dtCreationDate, ci.vchSpecialText, ci.decDiscountPrice, ci.decListPrice ")
                .ToString();

            IList<PdaResource> pdaResources = GetEntityList<PdaResource>(sql, null, true);

            //Used for Testing
            //pdaResources = new List<PdaResource>();
            //pdaResources.Add(new PdaResource { ResourceId = 1523, AddedDate = DateTime.Now, AddedToCartDate = DateTime.Now, PdaPromotionText = "Testing 123", DiscountPrice = 500 });

            return pdaResources.ToList();
        }

        public int InsertPdaRemovedFromCartResourceEmail(int userId)
        {
            var sql = new StringBuilder()
                .AppendFormat("Insert Into [{0}]..PdaResourceEmails (userid, resourceIsbn, dateEmailSent, type) ",
                    _r2UtilitiesSettings.R2UtilitiesDatabaseName)
                .Append("select u.iUserId, r.vchResourceISBN, getdate(), '2' ")
                .Append(PdaRemovedTables())
                .AppendFormat(" and u.iUserId = {0} ", userId)
                .Append("group by u.iUserId, r.vchResourceISBN ")
                .ToString();

            var rows = ExecuteInsertStatementReturnRowCount(sql, null, true);
            return rows;
        }


        /// <summary>
        ///     Gives you all the tables and a where clause for selecting the resources or the userids for PDA Resources Removed
        /// </summary>
        private string PdaRemovedTables()
        {
            return new StringBuilder()
                .Append("           from tUser u ")
                .Append(
                    "           join tUserOptionValue uov on u.iUserId = uov.iUserId and uov.tiRecordStatus = 1 and uov.vchUserOptionValue = '1' ")
                .Append(
                    "           join tUserOption uo on uov.iUserOptionId = uo.iUserOptionId and uo.tiRecordStatus = 1")
                .Append(
                    "           join tUserOptionRole uor on uo.iUserOptionId = uor.iUserOptionId and u.iRoleId = uor.iRoleId and uor.tiRecordStatus = 1")
                .Append(
                    "           join tInstitution i on u.iInstitutionId = i.iInstitutionId and i.tiRecordStatus = 1 ")
                .Append("           join tCart c on i.iInstitutionId = c.iInstitutionId and c.tiProcessed = 0 ")
                .AppendFormat(
                    "     join tCartItem ci on c.iCartId = ci.iCartId and ci.tiRecordStatus = 1 and ci.tiLicenseOriginalSourceId = {0} ",
                    (int)LicenseOriginalSource.Pda)
                .AppendFormat(
                    "     join tInstitutionResourceLicense irl on i.iInstitutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 and irl.tiLicenseOriginalSourceId = {0} ",
                    (int)LicenseOriginalSource.Pda)
                .Append("           join tResource r on irl.iResourceId = r.iResourceId ")
                .AppendFormat(
                    "     left outer join {0}..PdaResourceEmails pre on r.vchResourceISBN = pre.resourceIsbn and pre.type = 2 ",
                    _r2UtilitiesSettings.R2UtilitiesDatabaseName)
                .AppendFormat(
                    "     where dateadd(day, {0}, irl.dtPdaAddedToCartDate) <= GETDATE() and uov.iUserOptionId = {1}  and pre.id is null",
                    _r2UtilitiesSettings.PdaRemovedFromCartNumberOfDays, (int)UserOptionCode.PdaAddToCart)
                .Append(
                    "           and u.tiRecordStatus = 1 and i.tiRecordStatus = 1 and (u.dtExpirationDate is null or u.dtExpirationDate > getdate()) ")
                .AppendFormat(
                    "     and (i.iInstitutionAcctStatusId = {0} or (i.iInstitutionAcctStatusId = {1} and i.dtTrialAcctEnd > GETDATE() ) ) ",
                    (int)AccountStatus.Active, (int)AccountStatus.Trial)
                .ToString();
        }


        public IEnumerable<PdaHistoryResource> GetPdaHistoryResources(int institutionId)
        {
            var institution = _institutions.FirstOrDefault(x => x.Id == institutionId);
            var adminInstitution = new AdminInstitution(institution);

            var collectionManagementResources = new List<PdaHistoryResource>();
            var resources = GetPdaHistoryInstitutionResources(adminInstitution);
            collectionManagementResources.AddRange(resources.Select(resource => new PdaHistoryResource(resource)));

            var savedCarts = GetSavedCartsForInstitution(institutionId);

            foreach (var pdaHistoryResource in collectionManagementResources)
            {
                var resource = pdaHistoryResource;
                var resourceLicenses =
                    adminInstitution.Licenses.Where(x => x.ResourceId == resource.Resource.Id).ToList();
                if (resourceLicenses.Any())
                {
                    var resourceLicense = resourceLicenses.First();
                    if (resourceLicense != null)
                    {
                        pdaHistoryResource.LicenseType = resourceLicense.LicenseType;
                        if (resourceLicense.LicenseType == LicenseType.Purchased)
                        {
                            pdaHistoryResource.LicenseCount = resourceLicense.LicenseCount;
                            pdaHistoryResource.FirstPurchaseDate = resourceLicense.FirstPurchaseDate;
                            if (resourceLicense.OriginalSource == LicenseOriginalSource.Pda)
                            {
                                pdaHistoryResource.PdaAddedDate = resourceLicense.PdaAddedDate;
                                pdaHistoryResource.PdaAddedToCartDate = resourceLicense.PdaAddedToCartDate;
                                pdaHistoryResource.PdaCartDeletedDate = resourceLicense.PdaCartDeletedDate;
                                pdaHistoryResource.PdaCartDeletedByName = resourceLicense.PdaCartDeletedByName;
                                pdaHistoryResource.PdaViewCount = resourceLicense.PdaViewCount;
                                pdaHistoryResource.PdaMaxViews = resourceLicense.PdaMaxViews;
                                pdaHistoryResource.ResourceNotSaleableDate = resource.Resource.NotSaleableDate;
                            }
                        }
                        else if (resourceLicense.LicenseType == LicenseType.Pda)
                        {
                            pdaHistoryResource.LicenseCount = 0;
                            pdaHistoryResource.PdaAddedDate = resourceLicense.PdaAddedDate;
                            pdaHistoryResource.PdaAddedToCartDate = resourceLicense.PdaAddedToCartDate;

                            var pdaCartDeletedDate = resourceLicense.PdaCartDeletedDate;
                            var pdaCartDeletedByName = resourceLicense.PdaCartDeletedByName;
                            if (pdaCartDeletedDate == null && resourceLicense.PdaAddedToCartDate.HasValue)
                            {
                                if (resourceLicense.PdaAddedToCartDate.Value.AddMonths(1) < DateTime.Now)
                                {
                                    pdaCartDeletedDate = resourceLicense.PdaAddedToCartDate.Value.AddMonths(1);
                                    if (string.IsNullOrWhiteSpace(pdaCartDeletedByName))
                                    {
                                        pdaCartDeletedByName = "Expired and automatically removed from cart";
                                    }
                                }
                            }

                            pdaHistoryResource.PdaCartDeletedDate = pdaCartDeletedDate;
                            pdaHistoryResource.PdaCartDeletedByName = pdaCartDeletedByName;
                            pdaHistoryResource.PdaViewCount = resourceLicense.PdaViewCount;
                            pdaHistoryResource.PdaMaxViews = resourceLicense.PdaMaxViews;
                            pdaHistoryResource.ResourceNotSaleableDate = resource.Resource.NotSaleableDate;

                            foreach (var cart in savedCarts)
                            {
                                var cartItem =
                                    cart.CartItems.FirstOrDefault(x => x.ResourceId == pdaHistoryResource.ResourceId);
                                if (cartItem != null)
                                {
                                    var savedDate = cart.ConvertDate.GetValueOrDefault();
                                    pdaHistoryResource.DateOrNameCartWasSaved = savedDate == DateTime.MinValue
                                        ? cart.CartName
                                        : cart.ConvertDate.GetValueOrDefault().ToShortDateString();
                                    pdaHistoryResource.PdaCartDeletedByName = null;
                                    pdaHistoryResource.PdaCartDeletedDate = null;
                                }
                            }
                        }

                        pdaHistoryResource.OriginalSource = resourceLicense.OriginalSource;
                    }
                }
            }


            return collectionManagementResources;
        }

        /// <summary>
        ///     The tables used get the PDA History
        /// </summary>
        private static string PdaHistoryTables()
        {
            return new StringBuilder()
                .Append("           from tUser u ")
                .Append(
                    "           join tUserOptionValue uov on u.iUserId = uov.iUserId and uov.tiRecordStatus = 1 and uov.vchUserOptionValue = '1' ")
                .Append(
                    "           join tUserOption uo on uov.iUserOptionId = uo.iUserOptionId and uo.tiRecordStatus = 1")
                .Append(
                    "           join tUserOptionRole uor on uo.iUserOptionId = uor.iUserOptionId and u.iRoleId = uor.iRoleId and uor.tiRecordStatus = 1")
                .Append(
                    "           join tInstitution i on u.iInstitutionId = i.iInstitutionId and i.tiRecordStatus = 1 ")
                .AppendFormat(
                    "     join tInstitutionResourceLicense irl on u.iInstitutionId = irl.iInstitutionId and irl.tiRecordStatus = 1 and irl.tiLicenseOriginalSourceId = {0} ",
                    (int)LicenseOriginalSource.Pda)
                .Append("           join tResource r on irl.iResourceId = r.iResourceId  and r.tiRecordStatus = 1")
                .Append(
                    "     where u.tiRecordStatus = 1 and i.tiRecordStatus = 1 and (u.dtExpirationDate is null or u.dtExpirationDate > getdate()) ")
                .AppendFormat(
                    "     and uov.iUserOptionId = {2} and (i.iInstitutionAcctStatusId = {0} or (i.iInstitutionAcctStatusId = {1} and i.dtTrialAcctEnd > GETDATE() ) ) ",
                    (int)AccountStatus.Active, (int)AccountStatus.Trial, (int)UserOptionCode.PdaReport)
                .ToString();
        }

        public List<IResource> GetResources(List<int> resourceIds)
        {
            var resources = _resources.Where(x => resourceIds.Contains(x.Id)).ToList();

            var sortedResources = resources.OrderBy(x =>
            {
                var firstOrDefault = x.Specialties.OrderBy(y => y.Name).FirstOrDefault();
                return firstOrDefault != null ? x.Specialties != null ? firstOrDefault.Name : "zzzz" : null;
            }).ToList();

            return sortedResources;
        }

        public List<IResource> GetResources()
        {
            return _resources.ToList();
        }

        public List<IFeaturedTitle> GetFeaturedTitles(DateTime currentDate, int count = 0)
        {
            var featuredTitles = _featuredTitles.Where(x => x.StartDate < currentDate && x.EndDate > currentDate)
                .OrderByDescending(x => x.EndDate);
            if (count > 0)
            {
                if (featuredTitles.Count() > count)
                {
                    return featuredTitles.Take(count).ToList();
                }
            }

            return featuredTitles.ToList();
        }

        public List<SpecialResource> GetSpecials(DateTime currentDate, int count = 0)
        {
            var specialResources = _specialResources.Where(x =>
                x.Discount.Special.StartDate <= currentDate && x.Discount.Special.EndDate >= currentDate &&
                x.RecordStatus && x.Discount.RecordStatus).OrderByDescending(x => x.Discount.Special.EndDate);
            if (count > 0)
            {
                if (specialResources.Count() > count)
                {
                    return specialResources.Take(count).ToList();
                }
            }

            return specialResources.ToList();
        }

        public List<Recommendation> GetRecommendations(int institutionId, int count)
        {
            var query = _recommendations
                    .Fetch(x => x.RecommendedByUser).ThenFetch(d => d.Department)
                    .Fetch(x => x.PurchasedByUser).ThenFetch(d => d.Department)
                    .Fetch(x => x.AddedToCartByUser).ThenFetch(d => d.Department)
                    .Fetch(x => x.DeletedByUser).ThenFetch(d => d.Department)
                    .Where(x => x.InstitutionId == institutionId)
                    .Where(x => x.DeletedDate == null)
                    .Where(x => x.RecordStatus)
                    .Where(x => x.AddedToCartDate == null)
                    .OrderByDescending(x => x.CreationDate)
                ;

            if (count > 0)
            {
                if (query.Count() > count)
                {
                    return query.Take(count).ToList();
                }
            }

            return query.ToList();
        }

        public List<Institution> GetDashboardInstitutions()
        {
            var institutionIds = GetBaseUsers()
                .HasOptionSelected(UserOptionCode.Dashboard).Select(y => y.InstitutionId).Distinct();
            return _institutions
                .Where(x => x.AccountStatusId == (int)AccountStatus.Active && institutionIds.Contains(x.Id)).ToList();
        }

        public List<User> GetDashboardUsers(int institutionId)
        {
            return GetBaseUsers()
                .Where(x => x.InstitutionId == institutionId && x.Role.Code == RoleCode.INSTADMIN)
                .HasOptionSelected(UserOptionCode.Dashboard)
                .ToList();
        }


        public IEnumerable<IResource> GetPdaInstitutionResources(AdminInstitution adminInstitution)
        {
            var test = from r in _resources
                join irl in _institutionResourceLicenses on r.Id equals irl.ResourceId
                where irl.InstitutionId == adminInstitution.Id && irl.LicenseTypeId == (int)LicenseType.Pda
                select r;
            return test;
        }

        public List<IResource> GetPdaHistoryInstitutionResources(AdminInstitution adminInstitution)
        {
            var licenses = _institutionResourceLicenses.Where(x =>
                    x.InstitutionId == adminInstitution.Id && x.OriginalSourceId == (int)LicenseOriginalSource.Pda)
                .ToList();
            var resourceIds = licenses.Select(x => x.ResourceId).ToList();
            //TODO: 4/25/2025 fixed an issue where Institutions with Large PDA accounts fails.
            var resources = GetPdaHistoryInstitutionResourcesByBatching(resourceIds);
            return resources;

            //   return _resources.Where(x => resourceIds.Contains(x.Id)).ToList();
        }

        private List<IResource> GetPdaHistoryInstitutionResourcesByBatching(List<int> resourceIds, int batchSize = 500)
        {
            var resources = new List<IResource>();

            // Process resourceIds in batches
            for (var i = 0; i < resourceIds.Count; i += batchSize)
            {
                // Get the current batch of resourceIds
                var batchIds = resourceIds.Skip(i).Take(batchSize).ToList();

                // Query resources for the current batch
                var batchResources = _resources
                    .Where(x => batchIds.Contains(x.Id))
                    .ToList();

                // Add batch results to the overall collection
                resources.AddRange(batchResources);
            }

            return resources;
        }

        public User GetUser(int userId)
        {
            return GetBaseUsers().FirstOrDefault(x => x.Id == userId);
        }

        public List<User> GetTerritoryOwners(int territoryId)
        {
            return _userTerritories.Where(x => x.TerritoryId == territoryId).Select(y => y.User).ToList();
        }

        public List<int> GetArchivedEmailUserIds()
        {
            var sql = new StringBuilder()
                .Append(" select u.iUserId ")
                .Append(GetArchivedTables())
                .Append(" group by  u.iUserId ")
                .ToString();

            var query = _unitOfWork.Session.CreateSQLQuery(sql).List();
            return query.Cast<int>().ToList();
        }

        public List<int> GetArchivedEmailResourceIds(int userId)
        {
            var sql = new StringBuilder()
                .Append(" Select r.iResourceId ")
                .Append(GetArchivedTables())
                .AppendFormat("and u.iUserId = {0}", userId)
                .Append(" group by  r.iResourceId ")
                .ToString();

            var query = _unitOfWork.Session.CreateSQLQuery(sql).List();
            return query.Cast<int>().ToList();
        }

        private string GetArchivedTables()
        {
            return new StringBuilder()
                .Append("           from tUser u ")
                .Append(
                    "           join tUserOptionValue uov on u.iUserId = uov.iUserId and uov.tiRecordStatus = 1 and uov.vchUserOptionValue = '1' ")
                .Append(
                    "           join tUserOption uo on uov.iUserOptionId = uo.iUserOptionId and uo.tiRecordStatus = 1 ")
                .Append(
                    "           join tUserOptionRole uor on uo.iUserOptionId = uor.iUserOptionId and u.iRoleId = uor.iRoleId and uor.tiRecordStatus = 1 ")
                .Append(
                    "           join tInstitution i on u.iInstitutionId = i.iInstitutionId and i.tiRecordStatus = 1")
                .Append(
                    "           join tInstitutionResourceLicense irl on i.iInstitutionId = irl.iInstitutionId and irl.tiRecordStatus = 1")
                .AppendFormat(
                    "     join tResource r on irl.iResourceId = r.iResourceId and r.tiRecordStatus = 1 and r.iResourceStatusId = {0}",
                    (int)ResourceStatus.Archived)
                .AppendFormat("           join {0}..ResourceEmails re on r.vchResourceISBN = re.resourceISBN ",
                    _r2UtilitiesSettings.R2UtilitiesDatabaseName)
                .AppendFormat("           where uov.iUserOptionId = {0} and re.dateArchivedEmail is null ",
                    (int)UserOptionCode.ArchivedAlert)
                .Append(
                    "           and u.tiRecordStatus = 1 and i.tiRecordStatus = 1 and (u.dtExpirationDate is null or u.dtExpirationDate > getdate()) ")
                .AppendFormat(
                    "     and (i.iInstitutionAcctStatusId = {0} or (i.iInstitutionAcctStatusId = {1} and i.dtTrialAcctEnd > GETDATE() ) ) ",
                    (int)AccountStatus.Active, (int)AccountStatus.Trial)
                .ToString();
        }

        public void UpdateArchivedResourceEmailResources(List<IResource> archivedResources)
        {
            var updateSql =
                $"Update [{_r2UtilitiesSettings.R2UtilitiesDatabaseName}].[dbo].[ResourceEmails] set dateArchivedEmail = GetDate() Where [resourceISBN] = '[ISBN]'";

            var sql = new StringBuilder();
            foreach (var archivedResource in archivedResources)
            {
                sql.Append($"{updateSql}; ".Replace("[ISBN]", archivedResource.Isbn));
            }

            _unitOfWork.Session.CreateSQLQuery(sql.ToString()).List();
        }

        public List<ShoppingCartUser> GetShoppingCartUsers()
        {
            var sql = new StringBuilder()
                .Append(
                    "select u.vchUserEmail, u.iUserId, u.vchUserName, i.iInstitutionId, i.vchInstitutionName, i.vchInstitutionAcctNum, c.iCartId ")
                .Append("from tUser u ")
                .Append(
                    "           join tUserOptionValue uov on u.iUserId = uov.iUserId and uov.tiRecordStatus = 1 and uov.vchUserOptionValue = '1' ")
                .Append(
                    "           join tUserOption uo on uov.iUserOptionId = uo.iUserOptionId and uo.tiRecordStatus = 1 ")
                .Append(
                    "           join tUserOptionRole uor on uo.iUserOptionId = uor.iUserOptionId and u.iRoleId = uor.iRoleId and uor.tiRecordStatus = 1 ")
                .Append("join tInstitution i on u.iInstitutionId = i.iInstitutionId and ")
                .Append("       ((i.iInstitutionAcctStatusId = 1) or ")
                .Append(
                    "       (i.iInstitutionAcctStatusId = 2 and dateadd(dd, 0, datediff(dd, 0, i.dtTrialAcctEnd))  > dateadd(dd, 0, datediff(dd, 0, getdate())))) ")
                .Append(
                    "join tCart c on i.iInstitutionId = c.iInstitutionId and c.tiRecordStatus = 1 and c.iCartTypeId = 1 ")
                .Append("join tCartItem ci on c.iCartId = ci.iCartId and ci.tiRecordStatus = 1  and c.tiProcessed = 0 ")
                .Append("join tResource r on r.iResourceId = ci.iResourceId and r.iResourceStatusId in (6,8)")
                .AppendFormat("where u.iRoleId = 1 and uov.iUserOptionId = {0} and r.tiRecordStatus = 1 ",
                    (int)UserOptionCode.CartRemind)
                .Append(
                    " and u.tiRecordStatus = 1 and i.tiRecordStatus = 1 and (u.dtExpirationDate is null or u.dtExpirationDate > getdate()) ")
                .Append(
                    "and ((ci.iResourceId is null and ci.tiInclude = 1 and ci.iProductId <> 1) or (ci.iResourceId > 0)) ")
                .Append("and ((ci.dtLastUpdate is not null)  ")
                .AppendFormat("and dateadd(dd, 0, datediff(dd, 0, ci.dtLastUpdate))='{0}' ",
                    DateTime.Now.Date.AddDays(-7))
                .AppendFormat("or dateadd(dd, 0, datediff(dd, 0, ci.dtLastUpdate))='{0}' ",
                    DateTime.Now.Date.AddDays(-15))
                .AppendFormat("or dateadd(dd, 0, datediff(dd, 0, ci.dtLastUpdate))='{0}' ",
                    DateTime.Now.Date.AddMonths(-1))
                .Append("or (ci.dtLastUpdate is null) ")
                .AppendFormat("and dateadd(dd, 0, datediff(dd, 0, ci.dtCreationDate))='{0}' ",
                    DateTime.Now.Date.AddDays(-7))
                .AppendFormat("or dateadd(dd, 0, datediff(dd, 0, ci.dtCreationDate))='{0}' ",
                    DateTime.Now.Date.AddDays(-15))
                .AppendFormat("or dateadd(dd, 0, datediff(dd, 0, ci.dtCreationDate))='{0}') ",
                    DateTime.Now.Date.AddMonths(-1))
                .Append(
                    "group by u.vchUserEmail, u.iUserId, u.vchUserName, i.iInstitutionId, i.vchInstitutionName, i.vchInstitutionAcctNum, c.iCartId ")
                .Append("order by i.iInstitutionId, c.iCartId ")
                .ToString();

            IList<ShoppingCartUser> shoppingCartUsers = GetEntityList<ShoppingCartUser>(sql, null, true);
            return shoppingCartUsers.ToList();
        }

        public void AddNewResourcesToCart(int institutionId, List<IResource> resources)
        {
            try
            {
                var institution = _institutions.FirstOrDefault(x => x.Id == institutionId);
                AdminInstitution adminInstitution = null;
                if (institution != null)
                {
                    adminInstitution = new AdminInstitution(institution);
                }

                if (adminInstitution == null)
                {
                    throw new Exception("Institution is null and cannot be");
                }

                var cart = _carts.FetchMany(x => x.CartItems)
                               .SingleOrDefault(x =>
                                   x.InstitutionId == adminInstitution.Id && !x.Processed &&
                                   x.CartType == CartTypeEnum.Active) ??
                           new Cart
                           {
                               InstitutionId = institutionId,
                               Discount = adminInstitution.Discount
                           };

                var sql = new StringBuilder();
                if (cart.Id == 0)
                {
                    sql.AppendFormat(CartInsert, cart.InstitutionId, cart.Discount);

                    var query = _unitOfWork.Session.CreateSQLQuery(sql.ToString()).List();
                    if (query.First() != null)
                    {
                        int.TryParse(query.First().ToString(), out var x);

                        cart.Id = x;
                    }

                    if (cart.Id == 0)
                    {
                        throw new Exception("Could not insert a new cart and return the id of the cart.");
                    }
                }

                var collectionManagementResources =
                    resources.Select(x => new CollectionManagementResource(x, cart.Id)).ToList();

                foreach (var collectionManagementResource in collectionManagementResources)
                {
                    _resourceDiscountService.SetDiscount(collectionManagementResource, adminInstitution);
                }

                var resultsBuilder = new StringBuilder();

                var cartItems = cart.CartItems.ToList();

                foreach (var collectionManagementResource in collectionManagementResources)
                {
                    //MAKE SURE THE NEW EDITION IS NOT PREVIOUS PDA PURCHASED TITLE.
                    //  IT SHOULD BE PUT ON PDA teli
                    //Do not readd the cart item
                    if (!cartItems.Any() ||
                        cartItems.All(x => x.ResourceId != collectionManagementResource.Resource.Id))
                    {
                        sql = new StringBuilder()
                            .AppendFormat(CartItemInsert, cart.Id, collectionManagementResource.Resource.Id,
                                collectionManagementResource.Resource.ListPrice,
                                collectionManagementResource.DiscountPrice);
                        resultsBuilder.AppendFormat("{0},", collectionManagementResource.Resource.Id);
                    }
                }

                if (sql.Length > 0)
                {
                    _unitOfWork.Session.CreateSQLQuery(sql.ToString()).List();
                }

                resultsBuilder.Remove(resultsBuilder.Length - 1, 1);
                _log.InfoFormat("InstitutionId: {0}, CartId: {1}, ResourceIds added: {2} ", institutionId, cart.Id,
                    resultsBuilder.ToString());
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }

        public bool UpdateSavedReportLastUpdate(DateTime lastUpdate, int reportId)
        {
            try
            {
                var sql = $" UPDATE tSavedReports SET [dtLastUpdate] = '{lastUpdate}' WHERE iReportId =  {reportId} ";

                _unitOfWork.Session.CreateSQLQuery(sql).List();
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                return false;
            }

            return true;
        }

        public List<Specialty> GetSpecialties()
        {
            return _specialties.ToList();
        }

        public DateTime? GetAggregateInstitutionResourceStatisticsStartDate()
        {
            var sql = new StringBuilder()
                .AppendFormat(
                    " select max(institutionResourceStatisticsDate) from {0}.dbo.DailyInstitutionResourceStatisticsCount ",
                    _r2UtilitiesSettings.R2ReportsDatabaseName)
                .ToString();
            var results = _unitOfWork.Session.CreateSQLQuery(sql).List();
            var dateString = results.FirstOrNull();

            if (dateString == null)
            {
                return null;
            }

            DateTime.TryParse(dateString.ToString(), out var date);

            Log.InfoFormat("GetAggregateInstitutionResourceStatisticsStartDate StartDate: {0}", date.ToString("d"));

            return date;
        }

        /// <summary>
        ///     Sets the dtAlertSentDate and updater information for recommendation Ids supplied.
        /// </summary>
        public void SetRecommendationsAlertSentDate(IEnumerable<int> recommendationId)
        {
            var sb = new StringBuilder();
            foreach (var id in recommendationId)
            {
                sb.Append("         Update tInstitutionRecommendation   ")
                    .Append("       set dtAlertSentDate = GetDate()     ")
                    .Append("       , vchUpdaterId = 'R2Utilities'      ")
                    .Append("       , dtLastUpdate = GetDate()          ")
                    .AppendFormat(" Where iRecommendationId = {0};      ", id)
                    .AppendLine();
            }

            _unitOfWork.Session.CreateSQLQuery(sb.ToString()).List();
        }

        /// <summary>
        ///     Fully poplates a list of Recommendations for the specificed institution
        /// </summary>
        public List<Recommendation> GetRecommendations(int institutionId)
        {
            var query = _recommendations
                .Fetch(x => x.RecommendedByUser).ThenFetch(d => d.Department)
                .Fetch(x => x.PurchasedByUser).ThenFetch(d => d.Department)
                .Fetch(x => x.AddedToCartByUser).ThenFetch(d => d.Department)
                .Fetch(x => x.DeletedByUser).ThenFetch(d => d.Department)
                .Where(x => x.InstitutionId == institutionId && x.PurchaseDate == null && x.AlertSentDate == null &&
                            x.DeletedDate == null);

            return query.ToList();
        }

        /// <summary>
        ///     Gets the Ids of institutions who have recommendations that need to be emailed.
        /// </summary>
        public IEnumerable<int> GetInstitutionIdsForFacultyRecommentations()
        {
            return (from i in _institutions
                join ir in _recommendations on i.Id equals ir.InstitutionId
                where
                    ir.AlertSentDate == null && ir.RecordStatus && ir.PurchaseDate == null &&
                    ir.DeletedDate == null && i.ExpertReviewerUserEnabled
                select i.Id).Distinct();
        }

        public List<User> GetAnnualMaintenanceFeeUsers()
        {
            return
                GetBaseUsers().Where(x =>
                        x.Role != null && (x.Role.Code == RoleCode.SALESASSOC || x.Role.Code == RoleCode.RITADMIN) &&
                        (x.ExpirationDate < DateTime.Now || x.ExpirationDate == null) && x.RecordStatus)
                    .HasOptionSelected(UserOptionCode.AnnualMaintenanceFee)
                    .ToList();
        }

        /// <summary>
        ///     Populates a list of users from a specific users that will be receiving an email
        /// </summary>
        public List<User> GetFacultyRecommendationUsers(int institutionId)
        {
            var users = GetBaseUsers()
                .Where(x =>
                    x.Institution.Id == institutionId &&
                    (x.Role.Code == RoleCode.ExpertReviewer || x.Role.Code == RoleCode.INSTADMIN))
                .HasOptionSelected(UserOptionCode.ExpertReviewRecommend)
                .ToList();
            return users;
        }

        /// <summary>
        ///     Gets the IA for the institution
        /// </summary>
        public User GetInstitutionAdministrator(int institutionId)
        {
            return GetBaseUsers().Where(x => x.Institution.Id == institutionId && x.Role.Code == RoleCode.INSTADMIN)
                .OrderBy(x => x.CreationDate).FirstOrDefault();
        }

        private IQueryable<User> GetBaseUsers()
        {
            _users
                .FetchMany(x => x.UserTerritories)
                .Fetch(x => x.Institution)
                .Fetch(x => x.Role)
                .Fetch(x => x.Department)
                .ToFuture();
            _users
                .FetchMany(x => x.OptionValues)
                .ThenFetch(x => x.Option)
                .ThenFetch(x => x.Type)
                .ToFuture();

            return _users;
        }

        /// <summary>
        ///     Returns all the User Ids for people who want to receieve the PDA History Report
        /// </summary>
        public List<UserIdAndInstitutionId> GetPdaHistoryUserIds()
        {
            var sql = new StringBuilder()
                .Append(" select u.iUserId as 'UserId', u.iInstitutionId  as 'InstitutionId'")
                .Append(PdaHistoryTables())
                .Append(" group by  u.iUserId, u.iInstitutionId ")
                .ToString();

            var query = _unitOfWork.Session.CreateSQLQuery(sql).List();

            var userIdAndInstitutionIds = query.Cast<object[]>().Select(x => new UserIdAndInstitutionId
                { InstitutionId = (int)x[1], UserId = (int)x[0] });
            return userIdAndInstitutionIds.ToList();
        }

        public PdaHistoryReport GetPdaHistoryReport(int institutionId, List<PdaHistoryResource> pdaResources)
        {
            var sql = new StringBuilder()
                .Append("SELECT irl.iResourceId AS ResourceId, ")
                .Append("SUM(dirsc.contentRetrievalCount) AS ContentRetrievalCount, ")
                .Append("SUM(dirsc.tocRetrievalCount) AS TocRetrievalCount, ")
                .Append("SUM(dirsc.sessionCount) AS SessionCount, ")
                .Append("SUM(dirsc.printCount) AS PrintCount, ")
                .Append("SUM(dirsc.emailCount) AS EmailCount, ")
                .Append("SUM(dirsc.accessTurnawayCount) AS AccessTurnawayCount ")
                .Append("FROM tInstitutionResourceLicense irl ")
                .Append("INNER JOIN tresource r ON irl.iResourceId = r.iResourceId ")
                .Append("LEFT JOIN vDailyInstitutionResourceStatisticsCount dirsc ")
                .Append("ON irl.iInstitutionId = dirsc.institutionId ")
                .Append("AND irl.iResourceId = dirsc.resourceId ")
                .Append("AND dirsc.institutionResourceStatisticsDate >= irl.dtPdaAddedDate ")
                .Append("WHERE r.tiRecordStatus = 1 ")
                .Append("AND r.iResourceStatusId IN (6, 7) ")
                .Append("AND irl.tiRecordStatus = 1 ")
                .Append("AND irl.tiLicenseOriginalSourceId = 2 ")
                .Append("AND irl.iInstitutionId = :institutionId ")
                .Append("GROUP BY irl.iResourceId");

            var queryTimeoutSeconds = 180;
            var query = _unitOfWork.Session.CreateSQLQuery(sql.ToString())
                .SetParameter("institutionId", institutionId)
                .SetTimeout(queryTimeoutSeconds) // Set timeout in seconds
                .SetResultTransformer(Transformers.AliasToBean<PdaHistoryCount>());

            var pdaHistoryCounts = query.List<PdaHistoryCount>();
            if (pdaHistoryCounts.Any())
            {
                var pdaHistoryCountsList = pdaHistoryCounts.ToList();

                foreach (var pdaHistoryCount in pdaHistoryCountsList)
                {
                    var resource = pdaResources.FirstOrDefault(x => x.Resource.Id == pdaHistoryCount.ResourceId);
                    pdaHistoryCount.SetResource(resource);
                }

                var report = new PdaHistoryReport
                {
                    PdaHistoryCounts = pdaHistoryCountsList,
                    InstitutionId = institutionId
                };
                return report;
            }

            return null;
        }
    }

    public class UserIdAndInstitutionId
    {
        public int UserId { get; set; }
        public int InstitutionId { get; set; }
    }
}