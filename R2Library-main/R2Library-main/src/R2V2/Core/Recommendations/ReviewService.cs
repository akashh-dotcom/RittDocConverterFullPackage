#region

using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Recommendations
{
    public class ReviewService
    {
        private readonly IAuthenticationContext _authenticationContext;
        private readonly ILog<ReviewService> _log;
        private readonly IQueryable<ReviewResource> _reviewResources;
        private readonly IQueryable<Review> _reviews;
        private readonly IQueryable<ReviewUser> _reviewUsers;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public ReviewService(
            ILog<ReviewService> log
            , IUnitOfWorkProvider unitOfWorkProvider
            , IQueryable<Review> reviews
            , IQueryable<ReviewResource> reviewResources
            , IQueryable<ReviewUser> reviewUsers
            , IAuthenticationContext authenticationContext
        )
        {
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
            _reviews = reviews;
            _reviewResources = reviewResources;
            _reviewUsers = reviewUsers;
            _authenticationContext = authenticationContext;
        }

        /// <summary>
        /// </summary>
        public IEnumerable<Review> GetInstititionsReviews(int institutionId)
        {
            if (_authenticationContext.IsExpertReviewer())
            {
                return (from r in _reviews
                        join ru in _reviewUsers on r.Id equals ru.ReviewId
                        where r.InstitutionId == institutionId && r.RecordStatus
                                                               && ru.UserId == _authenticationContext
                                                                   .AuthenticatedInstitution.User.Id
                        select r
                    ).ToList();
            }

            var reviews = (from r in _reviews
                    where institutionId == r.InstitutionId && r.RecordStatus
                    select r
                ).ToList();
            return reviews;
        }

        /// <summary>
        /// </summary>
        public Review GetInstititionsReview(int institutionId, int reviewId)
        {
            var reviewsWithUsers = _reviews.FetchMany(x => x.ReviewUsers)
                .ThenFetch(x => x.User)
                .Where(x => x.InstitutionId == institutionId && x.Id == reviewId)
                .ToFuture();

            var reviews = _reviews.FetchMany(x => x.ReviewResources)
                .ThenFetch(x => x.ActionByUser)
                .Where(x => x.InstitutionId == institutionId && x.Id == reviewId)
                .ToList();

            var review = reviews.FirstOrDefault();

            if (_authenticationContext.IsExpertReviewer())
            {
                if (review != null && review.ReviewUsers.Any(x =>
                        x.UserId == _authenticationContext.AuthenticatedInstitution.User.Id))
                {
                    return review;
                }

                return null;
            }

            return review;
        }

        /// <summary>
        ///     Null Name and Description will remove delete the shelf
        /// </summary>
        public int SaveReviewList(int reviewId, int instititonId, string name, string description,
            int[] selectedExpertReviewerUserIds, int adminUserId, List<User> expertReviewers)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        var review = reviewId == 0
                            ? new Review()
                            : _reviews.SingleOrDefault(x => x.Id == reviewId && x.InstitutionId == instititonId);
                        if (review == null)
                        {
                            _log.ErrorFormat("Could not find review to save | reviewId:{0}  instititonId:{1}", reviewId,
                                instititonId);
                            return 0;
                        }

                        review.Name = name;
                        review.Description = description;
                        review.RecordStatus = true;
                        review.InstitutionId = instititonId;

                        uow.SaveOrUpdate(review);
                        if (expertReviewers != null)
                        {
                            foreach (var expertReviewer in expertReviewers)
                            {
                                var add = selectedExpertReviewerUserIds?.Any(x => x == expertReviewer.Id) ?? false;
                                SaveReviewUser(uow, review.Id, expertReviewer.Id, adminUserId, !add);
                            }
                        }


                        uow.Commit();
                        transaction.Commit();
                        return review.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }

        public ReviewUser SaveReviewUser(IUnitOfWork uow, int reviewId, int expertReviewerUserId, int adminUserId,
            bool delete)
        {
            var reviewUser =
                _reviewUsers.FirstOrDefault(x => x.ReviewId == reviewId && x.UserId == expertReviewerUserId);

            if (reviewUser == null)
            {
                if (delete)
                {
                    return null;
                }

                reviewUser = new ReviewUser
                {
                    AddedByUserId = adminUserId,
                    RecordStatus = true,
                    ReviewId = reviewId,
                    UserId = expertReviewerUserId
                };
            }
            else
            {
                if (delete)
                {
                    reviewUser.RecordStatus = false;
                    reviewUser.DeletedDate = DateTime.Now;
                    reviewUser.DeletedByUserId = adminUserId;
                }
                else
                {
                    reviewUser.RecordStatus = true;
                    reviewUser.DeletedDate = null;
                }
            }

            _log.Debug(reviewUser.ToDebugString());

            uow.Save(reviewUser);

            return reviewUser;
        }

        public int DeleteReviewList(int reviewId, int instititonId, int adminUserId)
        {
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        var review = reviewId == 0
                            ? new Review()
                            : _reviews.SingleOrDefault(x => x.Id == reviewId && x.InstitutionId == instititonId);
                        if (review == null)
                        {
                            _log.ErrorFormat("Could not find review to save | reviewId:{0}  instititonId:{1}", reviewId,
                                instititonId);
                            return 0;
                        }

                        review.RecordStatus = false;
                        review.DeletedDate = DateTime.Now;
                        review.DeletedByUserId = adminUserId;
                        uow.SaveOrUpdate(review);
                        uow.Commit();
                        transaction.Commit();
                    }

                    return reviewId;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// </summary>
        public void DeleteReviewResource(int reviewResourceId, int reviewId, int resourceId, int userId)
        {
            _log.DebugFormat("DeleteReviewResource(reviewResourceId: {0}, reviewId: {1}, resourceId: {2}, userId: {3})",
                reviewResourceId, reviewId, resourceId, userId);

            var reviewResource = _reviewResources.FirstOrDefault(x =>
                x.ReviewId == reviewId && x.Id == reviewResourceId && x.ResourceId == resourceId);

            if (reviewResource == null)
            {
                return;
            }

            reviewResource.DeletedByUserId = userId;
            reviewResource.DeletedDate = DateTime.Now;
            reviewResource.RecordStatus = false;

            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    uow.SaveOrUpdate(reviewResource);
                    uow.Commit();
                    transaction.Commit();
                }
            }
        }

        public void AddReviewResource(int reviewId, int resourceId, int userId)
        {
            _log.DebugFormat("AddReviewResource(reviewId: {0}, resourceId: {1}, userId: {2})", reviewId, resourceId,
                userId);
            using (var uow = _unitOfWorkProvider.Start())
            {
                uow.ExcludeSoftDeletedValues();
                using (var transaction = uow.BeginTransaction())
                {
                    //ReviewResource reviewResource = _reviewResources.FirstOrDefault(x => x.Id == reviewId && x.ResourceId == resourceId);
                    var reviewResource =
                        _reviewResources.FirstOrDefault(x => x.ReviewId == reviewId && x.ResourceId == resourceId);

                    if (reviewResource == null)
                    {
                        reviewResource = new ReviewResource
                        {
                            ReviewId = reviewId,
                            //InstitutionResource = _institutionResources.SingleOrDefault(x => x.InstitutionId == instititonId && x.ResourceId == resourceId),
                            ResourceId = resourceId,
                            RecordStatus = true,
                            ActionTypeId = 0,
                            ActionByUserId = userId,
                            ActionDate = DateTime.Now
                        };
                    }
                    else
                    {
                        reviewResource.DeletedByUserId = null;
                        reviewResource.DeletedDate = null;
                        reviewResource.RecordStatus = true;
                    }

                    uow.SaveOrUpdate(reviewResource);
                    uow.Commit();
                    transaction.Commit();
                }
            }
        }

        public void AddReviewResources(int reviewId, int[] resourceIds, int userId)
        {
            //_log.DebugFormat("AddReviewResource(reviewId: {0}, resourceId: {1}, userId: {2})", reviewId, resourceId, userId);
            using (var uow = _unitOfWorkProvider.Start())
            {
                uow.ExcludeSoftDeletedValues();
                using (var transaction = uow.BeginTransaction())
                {
                    foreach (var resourceId in resourceIds)
                    {
                        var reviewResource =
                            _reviewResources.FirstOrDefault(x => x.ReviewId == reviewId && x.ResourceId == resourceId);

                        if (reviewResource == null)
                        {
                            reviewResource = new ReviewResource
                            {
                                ReviewId = reviewId,
                                //InstitutionResource = _institutionResources.SingleOrDefault(x => x.InstitutionId == instititonId && x.ResourceId == resourceId),
                                ResourceId = resourceId,
                                RecordStatus = true,
                                ActionTypeId = 0,
                                ActionByUserId = userId,
                                ActionDate = DateTime.Now
                            };
                        }
                        else
                        {
                            reviewResource.DeletedByUserId = null;
                            reviewResource.DeletedDate = null;
                            reviewResource.RecordStatus = true;
                        }

                        //reviewResources.Add(reviewResource);
                        uow.SaveOrUpdate(reviewResource);
                    }
                    //ReviewResource reviewResource = _reviewResources.FirstOrDefault(x => x.Id == reviewId && x.ResourceId == resourceId);


                    uow.Commit();
                    transaction.Commit();
                }
            }
        }
    }
}