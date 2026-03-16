#region

using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.Recommendations;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.Storages;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.CollectionManagement
{
    public class RecommendationsService
    {
        private const string InstitutionRecommendationsKey = "Institutions.Recommendations";
        private readonly IAuthenticationContext _authenticationContext;
        private readonly IQueryable<Cart> _carts;
        private readonly ILocalStorageService _localStorageService;
        private readonly ILog<RecommendationsService> _log;
        private readonly IQueryable<Recommendation> _recommendations;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public RecommendationsService(IUnitOfWorkProvider unitOfWorkProvider
            , ILog<RecommendationsService> log
            , ILocalStorageService localStorageService
            , IAuthenticationContext authenticationContext
            , IQueryable<Recommendation> recommendations
            , IQueryable<Cart> carts
        )
        {
            _unitOfWorkProvider = unitOfWorkProvider;
            _recommendations = recommendations;
            _carts = carts;
            _log = log;
            _localStorageService = localStorageService;
            _authenticationContext = authenticationContext;
            //_log.Debug("RecommendationsService() <<<");
        }

        public IList<Recommendation> GetRecommendations(int institutionId)
        {
            var recommendations =
                _localStorageService.Get<IList<Recommendation>>(InstitutionRecommendationsKey);
            if (recommendations != null)
            {
                return recommendations;
            }

            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;

            using (_unitOfWorkProvider.Start())
            {
                var query = _recommendations
                        .Fetch(x => x.RecommendedByUser).ThenFetch(d => d.Department)
                        .Fetch(x => x.PurchasedByUser).ThenFetch(d => d.Department)
                        .Fetch(x => x.AddedToCartByUser).ThenFetch(d => d.Department)
                        .Fetch(x => x.DeletedByUser).ThenFetch(d => d.Department)
                        .Where(x => x.InstitutionId == institutionId)
                        .Where(x => x.DeletedDate == null)
                    ;

                if (authenticatedInstitution.IsExpertReviewer())
                {
                    query = query.Where(x => x.RecommendedByUserId == authenticatedInstitution.User.Id);
                }

                recommendations = query.ToList();
            }

            _log.DebugFormat("recommendations.Count: {0}", recommendations.Count);
            _localStorageService.Put(InstitutionRecommendationsKey, recommendations);
            return recommendations;
        }

        public IList<Recommendation> GetRecommendationsIncludeDeleted(int institutionId)
        {
            var recommendations =
                _localStorageService.Get<IList<Recommendation>>(InstitutionRecommendationsKey);
            if (recommendations != null)
            {
                return recommendations;
            }

            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;

            using (var uow = _unitOfWorkProvider.Start())
            {
                uow.IncludeSoftDeletedValues();
                var query = _recommendations
                    .Fetch(x => x.RecommendedByUser).ThenFetch(d => d.Department)
                    .Fetch(x => x.PurchasedByUser).ThenFetch(d => d.Department)
                    .Fetch(x => x.AddedToCartByUser).ThenFetch(d => d.Department)
                    .Fetch(x => x.DeletedByUser).ThenFetch(d => d.Department)
                    .Where(x => x.InstitutionId == institutionId)
                    .Where(x => x.RecordStatus);

                if (authenticatedInstitution.IsExpertReviewer())
                {
                    query = query.Where(x => x.RecommendedByUserId == authenticatedInstitution.User.Id);
                }

                recommendations = query.ToList();
                uow.ExcludeSoftDeletedValues();
            }

            _log.DebugFormat("recommendations.Count: {0}", recommendations.Count);
            _localStorageService.Put(InstitutionRecommendationsKey, recommendations);
            return recommendations;
        }

        public IList<Recommendation> GetRecommendations(int institutionId, int resourceId)
        {
            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;

            var query = _recommendations
                .Fetch(x => x.RecommendedByUser).ThenFetch(d => d.Department)
                .Fetch(x => x.PurchasedByUser).ThenFetch(d => d.Department)
                .Fetch(x => x.AddedToCartByUser).ThenFetch(d => d.Department)
                .Fetch(x => x.DeletedByUser).ThenFetch(d => d.Department)
                .Where(x => x.InstitutionId == institutionId)
                .Where(x => x.ResourceId == resourceId);

            if (authenticatedInstitution.IsExpertReviewer())
            {
                query = query.Where(x => x.RecommendedByUserId == authenticatedInstitution.User.Id);
            }

            IList<Recommendation> recommendations = query.ToList();
            _log.DebugFormat("recommendations.Count: {0}", recommendations.Count);
            return recommendations;
        }

        public Recommendation GetRecommendation(int recommendationId)
        {
            var query = _recommendations
                .Fetch(x => x.RecommendedByUser).ThenFetch(d => d.Department)
                .Fetch(x => x.PurchasedByUser).ThenFetch(d => d.Department)
                .Fetch(x => x.AddedToCartByUser).ThenFetch(d => d.Department)
                .Fetch(x => x.DeletedByUser).ThenFetch(d => d.Department)
                .Where(x => x.Id == recommendationId);

            return query.FirstOrDefault();
        }

        public Recommendation GetRecommendation(int institutionId, int userId, int resourceId)
        {
            var recommendation =
                _recommendations.SingleOrDefault(x =>
                    x.InstitutionId == institutionId && x.RecommendedByUserId == userId &&
                    x.ResourceId == resourceId);
            return recommendation;
        }

        public Recommendation SaveRecommendation(int institutionId, int userId, int resourceId, string notes)
        {
            var recommendation = GetRecommendation(institutionId, userId, resourceId);
            if (recommendation == null)
            {
                recommendation = new Recommendation
                {
                    RecommendedByUserId = userId,
                    InstitutionId = institutionId,
                    Notes = notes,
                    RecordStatus = true,
                    ResourceId = resourceId
                };
            }
            else
            {
                recommendation.DeletedDate = null;
            }

            _log.Debug(recommendation.ToDebugString());

            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    uow.Save(recommendation);
                    uow.Commit();
                    transaction.Commit();
                }
            }

            return recommendation;
        }

        public Recommendation UpdateRecommendation(int recommendationId, string notes)
        {
            var recommendation = GetRecommendation(recommendationId);
            recommendation.Notes = notes;
            _log.Debug(recommendation.ToDebugString());

            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    uow.Update(recommendation);
                    uow.Commit();
                    transaction.Commit();
                }
            }

            return recommendation;
        }

        public Recommendation DeleteRecommendation(int recommendationId, int userId, string deletedNote = null)
        {
            var recommendation = GetRecommendation(recommendationId);
            _log.Debug(recommendation.ToDebugString());
            recommendation.DeletedByUserId = userId;
            recommendation.DeletedDate = DateTime.Now;
            recommendation.DeletedNotes = deletedNote;

            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    uow.Update(recommendation);
                    uow.Commit();
                    transaction.Commit();
                }
            }

            return recommendation;
        }

        public bool HasRecommendation(int institutionId, int resourceId)
        {
            var recommendations = GetRecommendations(institutionId, resourceId);
            return recommendations != null && recommendations.Any();
        }

        public void RecommendationAddedToCart(int institutionId, int resourceId, IUser user)
        {
            try
            {
                var recommendations = GetRecommendations(institutionId, resourceId);
                if (recommendations != null && recommendations.Any())
                {
                    using (var uow = _unitOfWorkProvider.Start())
                    {
                        using (var transaction = uow.BeginTransaction())
                        {
                            foreach (var recommendation in recommendations)
                            {
                                recommendation.AddedToCartDate = DateTime.Now;
                                recommendation.AddedToCartByUserId = user.Id;
                                uow.Update(recommendation);
                            }

                            uow.Commit();
                            transaction.Commit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        public void BulkRecommendationsAddedToCart(int institutionId, int[] resourceIds, IUser user)
        {
            try
            {
                var recommendations = GetRecommendations(institutionId);
                if (recommendations != null && recommendations.Any())
                {
                    var recommendationsFound = recommendations.Where(x => resourceIds.Contains(x.ResourceId));
                    var recommendationsToUpdate =
                        recommendationsFound as Recommendation[] ?? recommendationsFound.ToArray();
                    if (recommendationsToUpdate.Any())
                    {
                        using (var uow = _unitOfWorkProvider.Start())
                        {
                            using (var transaction = uow.BeginTransaction())
                            {
                                foreach (var recommendation in recommendationsToUpdate)
                                {
                                    recommendation.AddedToCartDate = DateTime.Now;
                                    recommendation.AddedToCartByUserId = user.Id;
                                    uow.Update(recommendation);
                                }


                                uow.Commit();
                                transaction.Commit();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        public void RecommendationsPurchased(IOrder order, IUser user, IUnitOfWork uow)
        {
            try
            {
                var institutionId = order.InstitutionId;
                var recommendations = GetRecommendations(institutionId);
                if (recommendations != null && recommendations.Any())
                {
                    var cartId = order.OrderId;
                    var cart = _carts.Fetch(x => x.CartItems).FirstOrDefault(x => x.Id == cartId);

                    if (cart != null && cart.CartItems != null)
                    {
                        var cartResources = cart.CartItems.Where(x => x.ResourceId != null);
                        var cartItems = cartResources as CartItem[] ?? cartResources.ToArray();
                        if (cartItems.Any())
                        {
                            var resourceIds = cartItems.Select(x => x.ResourceId).ToArray();
                            if (resourceIds.Any())
                            {
                                var recommendationsFound =
                                    recommendations.Where(x => resourceIds.Contains(x.ResourceId));

                                var recommendationsToUpdate = recommendationsFound as Recommendation[] ??
                                                              recommendationsFound.ToArray();
                                if (recommendationsToUpdate.Any())
                                {
                                    foreach (var recommendation in recommendationsToUpdate)
                                    {
                                        recommendation.PurchaseDate = DateTime.Now;
                                        recommendation.PurchasedByUserId = user.Id;
                                        uow.Update(recommendation);
                                    }
                                }
                            }
                        }
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