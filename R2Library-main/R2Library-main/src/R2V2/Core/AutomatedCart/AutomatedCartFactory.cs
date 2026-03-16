#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Core.Recommendations;
using R2V2.Core.Resource;
using R2V2.Core.Territory;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.AutomatedCart
{
    public class AutomatedCartFactory
    {
        private readonly IQueryable<DbAutomatedCartEvent> _automatedCartEvents;
        private readonly IQueryable<DbAutomatedCartInstitution> _automatedCartInstitutions;

        private readonly IQueryable<DbAutomatedCartResource> _automatedCartResources;
        private readonly IQueryable<DbAutomatedCart> _automatedCarts;
        private readonly IQueryable<Cart> _carts;

        private readonly IQueryable<InstitutionResourceLicense> _institutionResourceLicenses;
        private readonly IQueryable<IInstitution> _institutions;
        private readonly ILog<AutomatedCartFactory> _log;

        private readonly RecommendationsService _recommendationsService;
        private readonly IResourceService _resourceService;
        private readonly IQueryable<ITerritory> _territories;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly IQueryable<UserResourceRequest> _userResourceRequests;
        private readonly IQueryable<User> _users;

        public AutomatedCartFactory(
            ILog<AutomatedCartFactory> log
            , IQueryable<DbAutomatedCartEvent> automatedCartEvents
            , IQueryable<IInstitution> institutions
            , IQueryable<ITerritory> territories
            , IUnitOfWorkProvider unitOfWorkProvider
            , IQueryable<DbAutomatedCart> automatedCarts
            , IQueryable<DbAutomatedCartInstitution> automatedCartInstitutions
            , IResourceService resourceService
            , IQueryable<DbAutomatedCartResource> automatedCartResources
            , RecommendationsService recommendationsService
            , IQueryable<InstitutionResourceLicense> institutionResourceLicenses
            , IQueryable<User> users
            , IQueryable<UserResourceRequest> userResourceRequests
            , IQueryable<Cart> carts
        )
        {
            _log = log;
            _automatedCartEvents = automatedCartEvents;
            _institutions = institutions;
            _territories = territories;
            _unitOfWorkProvider = unitOfWorkProvider;
            _automatedCarts = automatedCarts;
            _automatedCartInstitutions = automatedCartInstitutions;
            _resourceService = resourceService;
            _automatedCartResources = automatedCartResources;
            _recommendationsService = recommendationsService;
            _institutionResourceLicenses = institutionResourceLicenses;
            _users = users;
            _userResourceRequests = userResourceRequests;
            _carts = carts;
        }

        public List<DbAutomatedCartEvent> GetAutomatedCartEvents(AutomatedCartReportQuery reportQuery,
            int[] institutionIds)
        {
            var timer = new Stopwatch();
            timer.Start();
            _log.Debug(">>>>> Start GetAutomatedCartEvents");


            var automatedCartEvents = _automatedCartEvents
                .FilterDate(_institutions, reportQuery.PeriodStartDate.GetValueOrDefault(),
                    reportQuery.PeriodEndDate.GetValueOrDefault());

            if (institutionIds != null)
            {
                automatedCartEvents = automatedCartEvents.FilterInstitutionIds(institutionIds);
            }
            else
            {
                automatedCartEvents = automatedCartEvents
                    .FilterAccountNumbers(_institutions, reportQuery.GetAccountNumberArray())
                    .FilterInstitutionTypes(_institutions, reportQuery.InstitutionTypeIds)
                    .FilterTerritories(_territories, reportQuery.TerritoryCodes);
            }

            var filteredResults = automatedCartEvents.ToList();

            var includeCartEvents = new List<DbAutomatedCartEvent>();

            if (reportQuery.IncludeTurnaway)
            {
                includeCartEvents.AddRange(filteredResults.Where(x => x.Turnaway > 0));
            }

            if (reportQuery.IncludeNewEdition)
            {
                includeCartEvents.AddRange(filteredResults.Where(x => x.NewEdition > 0));
            }

            if (reportQuery.IncludeReviewed)
            {
                includeCartEvents.AddRange(filteredResults.Where(x => x.Reviewed > 0));
            }

            if (reportQuery.IncludeTriggeredPda)
            {
                includeCartEvents.AddRange(filteredResults.Where(x => x.TriggeredPda > 0));
            }

            if (reportQuery.IncludeRequested)
            {
                includeCartEvents.AddRange(filteredResults.Where(x => x.Requested > 0));
            }

            _log.Debug($"<<<<< End GetAutomatedCartEvents -- run time: {timer.ElapsedMilliseconds}ms");
            return includeCartEvents.ToList();
        }

        public void SaveAutomatedCart(DbAutomatedCart automatedCart, int[] institutionIds)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    uow.Save(automatedCart);
                    uow.Commit();
                }

                using (var uow = _unitOfWorkProvider.Start())
                {
                    var counter = 0;
                    var sql = new StringBuilder();
                    foreach (var institutionId in institutionIds)
                    {
                        counter++;
                        sql.Append(
                                "Insert into tAutomatedCartInstitution(iAutomatedCartId, iInstitutionId, vchCreatorId, dtCreationDate, tiRecordStatus)")
                            .Append(
                                $" select {automatedCart.Id}, {institutionId}, '{automatedCart.CreatedBy}', '{automatedCart.CreationDate}', {(automatedCart.RecordStatus ? '1' : '0')};");
                        if (counter == 50)
                        {
                            var query = uow.Session.CreateSQLQuery(sql.ToString());
                            query.ExecuteUpdate();
                            sql = new StringBuilder();
                            counter = 0;
                        }
                    }

                    if (sql.Length > 0)
                    {
                        var query = uow.Session.CreateSQLQuery(sql.ToString());
                        query.ExecuteUpdate();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        public DbAutomatedCart GetAutomatedCart(int automatedCartId)
        {
            return _automatedCarts.FirstOrDefault(x => x.Id == automatedCartId);
        }

        public List<DbAutomatedCartInstitution> GetAutoCartInstitutionsForProcessing(int automatedCartId)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                var automatedCartInstitutions = _automatedCartInstitutions
                    .Where(x => x.AutomatedCartId == automatedCartId && x.CartId == null).ToList();
                foreach (var dbAutomatedCartInstitution in automatedCartInstitutions)
                {
                    uow.Evict(dbAutomatedCartInstitution);
                }

                return automatedCartInstitutions;
            }
        }

        public int CreateAutomatedCartForInstitution(DbAutomatedCart automatedCart, IInstitution institution,
            bool forceCacheReload)
        {
            if (forceCacheReload)
            {
                _resourceService.ReloadResourceCache();
            }

            var highestDiscountPercentage = automatedCart.Discount > institution.Discount
                ? automatedCart.Discount
                : institution.Discount;
            var query = new AutomatedCartReportQuery
            {
                Period = automatedCart.Period,
                PeriodStartDate = automatedCart.StartDate,
                PeriodEndDate = automatedCart.EndDate,
                IncludeNewEdition = automatedCart.NewEdition,
                IncludeTriggeredPda = automatedCart.TriggeredPda,
                IncludeReviewed = automatedCart.Reviewed,
                IncludeTurnaway = automatedCart.Turnaway,
                IncludeRequested = automatedCart.Requested
            };

            var automatedCartResoures = BuildCartResources(query, institution.Id);
            if (automatedCartResoures != null)
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        try
                        {
                            var newCart = new Cart
                            {
                                CartType = CartTypeEnum.AutomatedCart,
                                InstitutionId = institution.Id,
                                Discount = highestDiscountPercentage,
                                CartName = automatedCart.CartName,
                                RecordStatus = true
                            };
                            //newCart.RecordStatus = true;
                            uow.SaveOrUpdate(newCart);


                            var automatedCartInstitution =
                                _automatedCartInstitutions.FirstOrDefault(x => x.AutomatedCartId == automatedCart.Id &&
                                                                               x.InstitutionId == institution.Id);
                            if (automatedCartInstitution == null)
                            {
                                _log.Error(
                                    $"Could not find AutomatedCartInstitution for InstitutionId: {institution.Id} AutomatedCart : {automatedCart.ToDebugString()}");
                                transaction.Rollback();
                                return 0;
                            }

                            automatedCartInstitution.CartId = newCart.Id;
                            //automatedCartInstitution.Cart = newCart;

                            /**
                            Calling SaveOrUpdate on automatedCartInstitution does not work here, but will work if called prior to commit! -DRJ
                            **/
                            //uow.SaveOrUpdate(automatedCartInstitution);

                            foreach (var dbAutomatedCartResource in automatedCartResoures)
                            {
                                var resource = _resourceService.GetResource(dbAutomatedCartResource.ResourceId);
                                var discount = highestDiscountPercentage;
                                var discountPrice = resource.ListPrice -
                                                    highestDiscountPercentage / 100 * resource.ListPrice;

                                var cartItem = new CartItem
                                {
                                    Cart = newCart,
                                    NumberOfLicenses = 1,
                                    ListPrice = resource.ListPrice,
                                    ResourceId = dbAutomatedCartResource.ResourceId,
                                    Include = true,
                                    OriginalSourceId = 1,
                                    Discount = discount,
                                    DiscountPrice = discountPrice
                                };
                                uow.Save(cartItem);

                                dbAutomatedCartResource.CartItemId = cartItem.Id;
                                dbAutomatedCartResource.AutomatedCartInstitutionId = automatedCartInstitution.Id;
                                dbAutomatedCartResource.ListPrice = cartItem.ListPrice;
                                dbAutomatedCartResource.DiscountPrice = cartItem.DiscountPrice;

                                uow.Save(dbAutomatedCartResource);
                            }

                            /**
                            Calling SaveOrUpdate on automatedCartInstitution works here, but will not work if called prior to foreach loop above! -DRJ
                            **/
                            uow.SaveOrUpdate(automatedCartInstitution);

                            uow.Commit();
                            transaction.Commit();
                            return newCart.Id;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _log.Error(ex.Message, ex);
                        }
                    }
                }
            }

            return 0;
        }

        public void UpdateEmailsSent(int automatedCartId, int institutionId, int emailsSents)
        {
            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var automatedCartInstitution =
                        _automatedCartInstitutions.FirstOrDefault(x => x.AutomatedCartId == automatedCartId &&
                                                                       x.InstitutionId == institutionId);

                    if (automatedCartInstitution != null)
                    {
                        automatedCartInstitution.EmailsSent = emailsSents;
                        uow.Update(automatedCartInstitution);
                        uow.Commit();
                        transaction.Commit();
                    }
                }
            }
        }

        public List<AutomatedCartInstitutionSummary> GetAutomatedCartInstitutionSummaries(int automatedCartId)
        {
            var automatedCartInstitutionSummaries = new List<AutomatedCartInstitutionSummary>();

            var automatedCartInstitutions =
                _automatedCartInstitutions.Where(x => x.AutomatedCartId == automatedCartId).ToList();
            
            var institutionIds = automatedCartInstitutions.Select(x => x.InstitutionId).ToList();

            var institutions = _institutions.Where(x => institutionIds.Contains(x.Id)).ToList();

            var users = _users.HasOptionSelected(UserOptionCode.AutomatedShoppingCart).Where(x =>
                institutionIds.Contains(x.InstitutionId.GetValueOrDefault()) &&
                (!x.ExpirationDate.HasValue || x.ExpirationDate.Value > DateTime.Now)).ToList();

            foreach (var dbAutomatedCartInstitution in automatedCartInstitutions)
            {
                var automatedCartResources = _automatedCartResources
                    .Where(x => x.AutomatedCartInstitutionId == dbAutomatedCartInstitution.Id).ToList();
                var institutionUsersCount = dbAutomatedCartInstitution.EmailsSent ??
                                            users.Count(x =>
                                                x.InstitutionId == dbAutomatedCartInstitution.InstitutionId);
                var institutiton = institutions.FirstOrDefault(x => x.Id == dbAutomatedCartInstitution.InstitutionId);
                var cartExists = dbAutomatedCartInstitution.CartId.HasValue &&
                                 _carts.Any(x => x.Id == dbAutomatedCartInstitution.CartId);

                var summary = new AutomatedCartInstitutionSummary(institutiton)
                {
                    CartId = dbAutomatedCartInstitution.CartId.GetValueOrDefault(0),
                    EmailCount = institutionUsersCount,

                    ListPrice = automatedCartResources.Sum(x => x.ListPrice),
                    DiscountPrice = automatedCartResources.Sum(x => x.DiscountPrice),
                    NewEditionCount = automatedCartResources.Sum(x => x.NewEditionCount),
                    PdaCount = automatedCartResources.Sum(x => x.TriggeredPdaCount),
                    ReviewedCount = automatedCartResources.Sum(x => x.ReviewedCount),
                    TurnawayCount = automatedCartResources.Sum(x => x.TurnawayCount),
                    RequestedCount = automatedCartResources.Sum(x => x.RequestedCount),
                    TitleCount = automatedCartResources.Count,
                    CartExists = cartExists
                };

                automatedCartInstitutionSummaries.Add(summary);
            }

            return automatedCartInstitutionSummaries;
        }


        private List<DbAutomatedCartResource> BuildCartResources(AutomatedCartReportQuery query, int institutionId)
        {
            try
            {
                var automatedCartResoures = new List<DbAutomatedCartResource>();
                var institutionEvents = GetAutomatedCartEvents(query, new[] { institutionId });
                foreach (var dbAutomatedCartEvent in institutionEvents)
                {
                    var addToList = false;
                    var automatedCartResoure =
                        automatedCartResoures.FirstOrDefault(x => x.ResourceId == dbAutomatedCartEvent.ResourceId) ??
                        new DbAutomatedCartResource();
                    if (automatedCartResoure.ResourceId == 0)
                    {
                        automatedCartResoure.ResourceId = dbAutomatedCartEvent.ResourceId;
                        addToList = true;
                    }

                    automatedCartResoure.NewEditionCount += dbAutomatedCartEvent.NewEdition;
                    automatedCartResoure.TriggeredPdaCount += dbAutomatedCartEvent.TriggeredPda;
                    automatedCartResoure.ReviewedCount += dbAutomatedCartEvent.Reviewed;
                    automatedCartResoure.TurnawayCount += dbAutomatedCartEvent.Turnaway;
                    automatedCartResoure.RequestedCount += dbAutomatedCartEvent.Requested;

                    if (addToList)
                    {
                        automatedCartResoures.Add(automatedCartResoure);
                    }
                }

                var resourceIdBulder = new StringBuilder();
                foreach (var dbAutomatedCartResource in automatedCartResoures)
                {
                    resourceIdBulder.Append($" {dbAutomatedCartResource.ResourceId}, ");
                }

                _log.Info($"ResourceIds to add to cart -->> ResoruceIds: [{resourceIdBulder}] ");

                return automatedCartResoures;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return null;
        }

        public decimal GetAutomatedCartDiscount(int cartId, int institutionId)
        {
            //var automatedCart = (from aci in _automatedCartInstitutions
            //    join ac in _automatedCarts on aci.AutomatedCartId equals ac.Id
            //    where aci.CartId == cartId && aci.InstitutionId == institutionId
            //    select ac).OrderByDescending(x => x.Discount).FirstOrDefault();

            //return automatedCart?.Discount ?? 0;

            var automatedCartInstitution =
                _automatedCartInstitutions.FirstOrDefault(x => x.CartId == cartId && x.InstitutionId == institutionId);
            if (automatedCartInstitution != null)
            {
                var automatedCart =
                    _automatedCarts.FirstOrDefault(x => x.Id == automatedCartInstitution.AutomatedCartId);
                if (automatedCart != null)
                {
                    return automatedCart.Discount;
                }
            }

            return 0;
        }

        public void PopulateAutomatedCartReasonCodes(CachedCart cart)
        {
            var automatedCartResources = GetCartResources(cart.Id);
            List<UserResourceRequest> userResourceRequests = null;
            IList<Recommendation> recommendations = null;
            foreach (var cachedCartItem in cart.CartItems)
            {
                var automatedCartResource =
                    automatedCartResources.FirstOrDefault(x => x.CartItemId == cachedCartItem.Id);
                if (automatedCartResource != null)
                {
                    //_recommendationsService.GetRecommendationsIncludeDeleted(collectionManagementQuery.InstitutionId)
                    var reasonCodes = new List<string>();
                    if (automatedCartResource.NewEditionCount > 0)
                    {
                        reasonCodes.Add("New edition of title in your collection");
                    }

                    if (automatedCartResource.TriggeredPdaCount > 0)
                    {
                        var license = _institutionResourceLicenses.FirstOrDefault(x =>
                            x.ResourceId == automatedCartResource.ResourceId && x.InstitutionId == cart.InstitutionId);
                        reasonCodes.Add(
                            $"PDA title triggered ({license?.PdaAddedToCartDate.GetValueOrDefault(DateTime.Now):MM/dd/yyyy}); not purchased");
                    }

                    if (automatedCartResource.ReviewedCount > 0)
                    {
                        if (recommendations == null)
                        {
                            recommendations =
                                _recommendationsService.GetRecommendationsIncludeDeleted(cart.InstitutionId);
                        }

                        var recommendationsFound =
                            recommendations.Where(x => x.ResourceId == automatedCartResource.ResourceId);
                        foreach (var recommendation in recommendationsFound)
                        {
                            var sb = new StringBuilder();
                            sb.AppendFormat("Recommended On: {0:MM/dd/yyyy}", recommendation.CreationDate);
                            if (recommendation.RecommendedByUser != null)
                            {
                                sb.AppendFormat(" by {0} {1} ", recommendation.RecommendedByUser.FirstName,
                                    recommendation.RecommendedByUser.LastName);
                                if (!string.IsNullOrWhiteSpace(recommendation.RecommendedByUser.Department.Name))
                                {
                                    sb.AppendFormat("[{0}]", recommendation.RecommendedByUser.Department.Name);
                                }
                            }

                            reasonCodes.Add(sb.ToString());
                        }
                    }

                    if (automatedCartResource.TurnawayCount > 0)
                    {
                        reasonCodes.Add(
                            $"Concurrent User /Access Denied Turnaways ({automatedCartResource.TurnawayCount})");
                    }

                    if (automatedCartResource.RequestedCount > 0)
                    {
                        if (userResourceRequests == null)
                        {
                            userResourceRequests = GetUserResourceRequests(cart.InstitutionId);
                        }

                        var requests =
                            userResourceRequests.Where(x => x.ResourceId == automatedCartResource.ResourceId);

                        foreach (var userResourceRequest in requests)
                        {
                            var sb = new StringBuilder();
                            sb.Append($"Requested on: {userResourceRequest.CreationDate:MM/dd/yyyy}");
                            if (userResourceRequest.Name != "N/A" &&
                                !string.IsNullOrWhiteSpace(userResourceRequest.Name))
                            {
                                sb.Append($" by {userResourceRequest.Name}");
                            }

                            if (!string.IsNullOrWhiteSpace(userResourceRequest.Title))
                            {
                                sb.Append($", {userResourceRequest.Title}");
                            }

                            if (!string.IsNullOrWhiteSpace(userResourceRequest.Comment))
                            {
                                sb.Append($", {userResourceRequest.Comment}");
                            }

                            reasonCodes.Add(sb.ToString());
                        }
                    }

                    cachedCartItem.AutomatedReasonCodes = reasonCodes;
                }
            }
        }

        public List<DbAutomatedCartResource> GetCartResources(int cartId)
        {
            var automatedCartResources = from c in _automatedCartInstitutions
                join ci in _automatedCartResources on c.Id equals ci.AutomatedCartInstitutionId
                where c.CartId == cartId
                select ci;
            return automatedCartResources.ToList();
        }

        public bool IsSourceCartHigherDiscount(Cart sourceCart, Cart destinationCart)
        {
            var sourceAutomatedCart = (from aci in _automatedCartInstitutions
                join ac in _automatedCarts on aci.AutomatedCartId equals ac.Id
                where aci.CartId == sourceCart.Id
                select ac).OrderByDescending(x => x.Discount).FirstOrDefault();

            var destinationAutomatedCart = (from aci in _automatedCartInstitutions
                join ac in _automatedCarts on aci.AutomatedCartId equals ac.Id
                where aci.CartId == destinationCart.Id
                select ac).OrderByDescending(x => x.Discount).FirstOrDefault();

            if (destinationAutomatedCart == null && sourceAutomatedCart != null)
            {
                return true;
            }

            if (sourceAutomatedCart == null)
            {
                return false;
            }

            return sourceAutomatedCart.Discount >= destinationAutomatedCart.Discount;
        }

        public bool CheckAndInsertAutomatedCartMergeRecord(Cart sourceCart, Cart destinationCart, IUnitOfWork uow)
        {
            try
            {
                var sourceAutomatedCart = (from aci in _automatedCartInstitutions
                    join ac in _automatedCarts on aci.AutomatedCartId equals ac.Id
                    where aci.CartId == sourceCart.Id
                    select ac).OrderByDescending(x => x.Discount).FirstOrDefault();

                var destinationAutomatedCart = (from aci in _automatedCartInstitutions
                    join ac in _automatedCarts on aci.AutomatedCartId equals ac.Id
                    where aci.CartId == destinationCart.Id
                    select ac).OrderByDescending(x => x.Discount).FirstOrDefault();

                //Merging None-Automated Cart Into Automated Cart
                if (sourceAutomatedCart == null)
                {
                    return true;
                }

                //Merging Automated Cart into Non-Automated Cart
                //OR
                //Merging higher discount Automated Cart into lower discount Automated Cart
                if (destinationAutomatedCart == null ||
                    sourceAutomatedCart.Discount > destinationAutomatedCart.Discount)
                {
                    return InsertAutomatedCartInstitutionMergeRecord(sourceAutomatedCart.Id, destinationCart.Id,
                        destinationCart.InstitutionId, uow);
                }

                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return false;
        }

        private bool InsertAutomatedCartInstitutionMergeRecord(int automatedCartId, int cartId, int institutionId,
            IUnitOfWork uow)
        {
            try
            {
                var automatedCartInstitution = new DbAutomatedCartInstitution
                {
                    AutomatedCartId = automatedCartId,
                    CartId = cartId,
                    InstitutionId = institutionId,
                    RecordStatus = true
                };

                uow.Save(automatedCartInstitution);
                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return false;
        }

        public List<UserResourceRequest> GetUserResourceRequests(int institutionId)
        {
            return _userResourceRequests.Where(x => x.InstitutionId == institutionId).ToList();
        }


        public List<AutomatedCartHistory> GetAutomatedCartHistories()
        {
            var automatedCartHistories = new List<AutomatedCartHistory>();
            var automatedCarts = _automatedCarts.Where(x => x.RecordStatus).OrderByDescending(x => x.CreationDate);
            foreach (var dbAutomatedCart in automatedCarts)
            {
                var automatedCartHistory = new AutomatedCartHistory();
                automatedCartHistory.CartName = dbAutomatedCart.CartName;
                automatedCartHistory.AutomatedCartId = dbAutomatedCart.Id;
                automatedCartHistory.CreatedDate = dbAutomatedCart.CreationDate;

                ////Filters out all Automated carts merged into other carts, but still need the discount
                var automatedCartInstitutions =
                    _automatedCartInstitutions.Where(x => x.AutomatedCartId == dbAutomatedCart.Id && x.RecordStatus);
                automatedCartHistory.InstitutionCount = automatedCartInstitutions.Count();

                var processedInstitutionCount = automatedCartInstitutions.Count(x => x.CartId.HasValue);
                var notProcessedInstitutionCount = automatedCartInstitutions.Count(x => !x.CartId.HasValue);

                automatedCartHistory.ProcessedCount = processedInstitutionCount;


                automatedCartHistory.WasProcessed = notProcessedInstitutionCount == 0;
                automatedCartHistories.Add(automatedCartHistory);
            }

            return automatedCartHistories;
        }
    }
}