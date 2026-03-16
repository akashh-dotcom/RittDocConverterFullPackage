#region

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using R2V2.Contexts;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Institution;
using R2V2.Core.Reports;
using R2V2.Core.Resource.Collection;
using R2V2.Infrastructure.Authentication;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Controllers;
using R2V2.Web.Areas.Admin.Controllers.CollectionManagement;
using R2V2.Web.Areas.Admin.Models;
using R2V2.Web.Areas.Admin.Models.CollectionManagement;
using R2V2.Web.Areas.Admin.Models.ExpressCheckout;
using R2V2.Web.Areas.Admin.Models.Report;
using R2V2.Web.Controllers;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.Settings;
using R2V2.Web.Models;
using R2V2.Web.Models.Authentication;

#endregion

//using SubscriptionsController = R2V2.Web.Areas.Admin.Controllers.SubscriptionsController;

namespace R2V2.Web.Infrastructure.MvcFramework.Filters.Admin
{
    public class AdminNavBuildFilter : R2V2ResultFilter
    {
        private readonly IAdminContext _adminContext;
        private readonly IAuthenticationContext _authenticationContext;
        private readonly Func<ICollectionService> _collectionServiceFactory;
        private readonly ILog<AdminNavBuildFilter> _log;
        private readonly IWebSettings _webSettings;

        public AdminNavBuildFilter(IAuthenticationContext authenticationContext
            , ILog<AdminNavBuildFilter> log
            , IWebSettings webSettings
            , IAdminContext adminContext
            , Func<ICollectionService> collectionServiceFactory
        )
            : base(authenticationContext)
        {
            _authenticationContext = authenticationContext;
            _log = log;
            _webSettings = webSettings;
            _adminContext = adminContext;
            _collectionServiceFactory = collectionServiceFactory;
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var controller = filterContext.Controller;

            var actionName = filterContext.RouteData.Values["action"].ToString();
            var codeName = filterContext.RouteData.Values["codeName"]?.ToString();

            var model = controller.ViewData.Model;
            var adminBaseModel = model as AdminBaseModel;
            //Changed this from adminBaseModel == null to the following because it was causing errors for IAs in the BulkAddToPda
            if (adminBaseModel == null || (adminBaseModel.InstitutionId == 0 &&
                                           _authenticationContext.AuthenticatedInstitution != null &&
                                           !_authenticationContext.AuthenticatedInstitution.IsRittenhouseAdmin() &&
                                           !_authenticationContext.AuthenticatedInstitution.IsSalesAssociate()))
            {
                return;
            }

            var urlHelper = new UrlHelper(filterContext.RequestContext);

            // verify that an authenticated institution exists
            var authenticatedInstitution = _authenticationContext.AuthenticatedInstitution;
            if (authenticatedInstitution == null)
            {
                // this should never happen but lets be a good programmer and test for it anyway!
                var redirectUrl = urlHelper.Action("NoAccess", "Authentication",
                    new { Area = "", accessCode = AccessCode.Unauthorized.ToLower() });
                _log.DebugFormat("redirectUrl: {0}", redirectUrl);
                _log.Error(
                    "AuthenticatedInstitution is null in AdminNavBuildFilter.OnResultExecuting() - THIS SHOULD NEVER HAPPEN!");
                filterContext.Result = new RedirectResult(redirectUrl);
                return;
            }

            // Handle special case 1
            if (authenticatedInstitution.IsPublisherUser())
            {
                // for publisher user, display no menu, just the report
                return;
            }

            // Handle special case 2
            if ((authenticatedInstitution.IsRittenhouseAdmin() || authenticatedInstitution.IsSalesAssociate()) &&
                controller is ReportController && adminBaseModel.InstitutionId == 0)
            {
                // for RAs and SAs, display only the reports menu it the main Reports tab was selected
                adminBaseModel.NavLinkSections = new List<PageLinkSection>
                {
                    new PageLinkSection
                    {
                        Title = "Reports",
                        PageLinks = GetInstitutionReportsLinks(urlHelper, controller, adminBaseModel,
                            authenticatedInstitution, actionName)
                    },
                    new PageLinkSection
                    {
                        Title = "Discount Reports",
                        PageLinks = GetDiscountReportsLinks(urlHelper, controller, actionName)
                    }
                };
                return;
            }

            if ((authenticatedInstitution.IsRittenhouseAdmin() || authenticatedInstitution.IsSalesAssociate()) &&
                (controller is MarketingController || controller is SubscriptionManagementController ||
                 controller is PromotionController || controller is SpecialController ||
                 controller is PdaPromotionController) &&
                adminBaseModel.InstitutionId == 0)
            {
                // for RAs and SAs, display only the reports menu it the main Reports tab was selected
                adminBaseModel.NavLinkSections = new List<PageLinkSection>
                {
                    new PageLinkSection
                    {
                        Title = "Discounts",
                        PageLinks = GetDiscountNavLinks(urlHelper, controller)
                    },
                    new PageLinkSection
                    {
                        Title = "Automated Carts",
                        PageLinks = GetMarketingLinks(urlHelper, controller, actionName)
                    },

                    new PageLinkSection
                    {
                        Title = "Reports",
                        PageLinks = GetReportLinks(urlHelper, controller, actionName)
                    },
                    new PageLinkSection
                    {
                        Title = "Utilities",
                        PageLinks = GetCacheLinks(urlHelper, controller, actionName)
                    },
                    new PageLinkSection
                    {
                        Title = "Subscriptions",
                        PageLinks = GetSubscriptionLinks(urlHelper, controller, actionName)
                    }
                };
                return;
            }

            // Handle special case #3 - Expert Reviewer users
            if (authenticatedInstitution.IsExpertReviewer())
            {
                // just display collection management for Expert Reviewer users
                adminBaseModel.NavLinkSections = new List<PageLinkSection>
                {
                    new PageLinkSection
                    {
                        Title = "Browse eBooks",
                        PageLinks = GetExpertReviewerCollectionManagementLinks(urlHelper, controller, adminBaseModel)
                    },
                    new PageLinkSection
                    {
                        Title = "Special Collections",
                        PageLinks = GetAdminCollectionLinks(urlHelper, controller, adminBaseModel)
                    },
                    new PageLinkSection
                    {
                        Title = "System Information",
                        PageLinks = new List<PageLink>
                        {
                            new PageLink
                            {
                                Text = "Expert Reviewer Guide",
                                Href = "/_Static/Downloads/r2-library-expert-review-guide.pdf",
                                Selected = false
                            }
                        }
                    }
                };
                return;
            }

            if (controller is ResourcePromotionController)
            {
                adminBaseModel.NavLinkSections = new List<PageLinkSection>
                {
                    new PageLinkSection
                    {
                        Title = "Resource Promotion",
                        PageLinks = GetResourcePromotionNavLinks(urlHelper, controller, actionName)
                    }
                };
                return;
            }

            // This is the normal case
            var navLinkSections = new List<PageLinkSection>
            {
                new PageLinkSection
                {
                    Title = "Institution Management",
                    PageLinks = GetInstitutionNavLinks(urlHelper, controller, adminBaseModel)
                },
                new PageLinkSection
                {
                    Title = "Collection Management",
                    PageLinks = GetAdminCollectionManagementLinks(urlHelper, controller, adminBaseModel),
                    NumberOfVisibleLinks = 10
                }
            };

            navLinkSections.Add(new PageLinkSection
            {
                Title = "Special Collections",
                PageLinks = GetAdminCollectionLinks(urlHelper, controller, adminBaseModel)
            });

            navLinkSections.Add(new PageLinkSection
            {
                Title = "Usage Reports",
                PageLinks =
                    GetInstitutionReportsLinks(urlHelper, controller, adminBaseModel,
                        authenticatedInstitution, actionName)
            });

            navLinkSections.Add(new PageLinkSection
            {
                Title = "R2 Library Access and Discoverability Support",
                PageLinks = GetAccessAndDiscoverabilityLinks(urlHelper, controller, adminBaseModel, actionName,
                    codeName)
            });

            var institutionId = adminBaseModel.InstitutionId;
            var adminInstitution = _adminContext.GetAdminInstitution(institutionId);
            if (adminInstitution.ExpertReviewerUserEnabled)
            {
                navLinkSections.Add(new PageLinkSection
                {
                    Title = "Expert Reviewer",
                    PageLinks = GetAdminExpertReviewerLinks(urlHelper, controller, adminBaseModel)
                });
            }

            adminBaseModel.NavLinkSections = navLinkSections;
        }

        private IEnumerable<PageLink> GetDiscountNavLinks(UrlHelper urlHelper, ControllerBase controller)
        {
            var promotion = new PageLink
            {
                Text = "Promotions",
                Href = urlHelper.AdminAction<PromotionController>(a => a.List()),
                Selected = controller is PromotionController
            };

            var pdaPromotions = new PageLink
            {
                Text = "PDA Promotions",
                Href = urlHelper.AdminAction<PdaPromotionController>(a => a.List()),
                Selected = controller is PdaPromotionController
            };

            var specials = new PageLink
            {
                Text = "Specials",
                Href = urlHelper.AdminAction<SpecialController>(a => a.List()),
                Selected = controller is SpecialController
            };


            return new List<PageLink>
            {
                promotion,
                pdaPromotions,
                specials
            };
        }

        private IEnumerable<PageLink> GetResourcePromotionNavLinks(UrlHelper urlHelper, ControllerBase controller,
            string actionName)
        {
            var queue = new PageLink
            {
                Text = "Promotion Queue",
                Href = urlHelper.AdminAction<ResourcePromotionController>(a => a.Queue("")),
                Selected = controller is ResourcePromotionController && actionName == "Queue"
            };

            var history = new PageLink
            {
                Text = "Promotion History",
                Href = urlHelper.AdminAction<ResourcePromotionController>(a => a.History(1, 50)),
                Selected = controller is ResourcePromotionController && actionName == "History"
            };

            return new List<PageLink>
            {
                queue,
                history
            };
        }


        private IEnumerable<PageLink> GetInstitutionNavLinks(UrlHelper urlHelper, ControllerBase controller,
            AdminBaseModel adminBaseModel)
        {
            var institutionId = adminBaseModel.InstitutionId;

            var dashboardRouteValues = new RouteValueDictionary { { "InstitutionId", institutionId } };
            var institutionResources = adminBaseModel as InstitutionResources;
            var dashboardPage = "Index";
            if (institutionResources != null && institutionResources.CollectionManagementQuery != null)
            {
                var q = institutionResources.CollectionManagementQuery;

                if (q.ResourceListType == ResourceListType.Archived ||
                    q.ResourceListType == ResourceListType.Purchased ||
                    q.ResourceListType == ResourceListType.NewEditionPurchased)
                {
                    dashboardPage = "EbookCollection";
                }

                if (q.ResourceListType == ResourceListType.PdaAdded ||
                    q.ResourceListType == ResourceListType.PdaAddedToCart ||
                    q.ResourceListType == ResourceListType.PdaNewEdition)
                {
                    dashboardPage = "PdaCollection";
                }

                dashboardRouteValues = q.ToRouteValues();
            }

            var dashboard = new PageLink
            {
                Text = "Dashboard",
                Href = urlHelper.Action(dashboardPage, "Dashboard", dashboardRouteValues),
                Selected = controller is DashboardController
            };

            var profile = new PageLink
            {
                Text = "Profile",
                Href = urlHelper.AdminAction<InstitutionController>(a => a.Detail(institutionId, false)),
                Selected = controller is InstitutionController
            };

            var institutionBranding = new PageLink
            {
                Text = "Institution Branding",
                Href = urlHelper.AdminAction<InstitutionBrandingController>(a => a.Detail(institutionId)),
                Selected = controller is InstitutionBrandingController
            };

            var notes = new PageLink
            {
                Text = "Notes",
                Href = urlHelper.AdminAction<NotesController>(a => a.List(institutionId)),
                Selected = controller is NotesController
            };

            var userManagement = new PageLink
            {
                Text = "User Management",
                Href = urlHelper.Action("List", "User", new { InstitutionId = institutionId }),
                Selected = controller is UserController
            };
            return new List<PageLink>
            {
                dashboard,
                profile,
                institutionBranding,
                notes,
                userManagement
            };
        }

        private IEnumerable<PageLink> GetAdminCollectionManagementLinks(UrlHelper urlHelper, ControllerBase controller,
            AdminBaseModel adminBaseModel)
        {
            var institutionId = adminBaseModel.InstitutionId;
            var institutionResources = adminBaseModel as InstitutionResources;
            var query = new CollectionManagementQuery
            {
                InstitutionId = institutionId,
                Query = "",
                SortBy = "",
                ResourceStatus = 0,
                ResourceFilterType = 0,
                PurchasedOnly = false
            };

            var collectionManagementQuery =
                institutionResources == null ? query : institutionResources.CollectionManagementQuery;

            var purchaseBooks = new PageLink
            {
                Text = "Purchase eBooks",
                Href = urlHelper.Action("List", "CollectionManagement", new { query.InstitutionId }),
                Selected = controller is CollectionManagementController &&
                           !collectionManagementQuery.PurchasedOnly &&
                           !collectionManagementQuery.IncludePdaResources &&
                           !collectionManagementQuery.RecommendationsOnly &&
                           collectionManagementQuery.ResourceListType == ResourceListType.All &&
                           collectionManagementQuery.CollectionFilter == 0 &&
                           collectionManagementQuery.CollectionListFilter == 0 &&
                           !collectionManagementQuery.IncludeSpecialDiscounts &&
                           !collectionManagementQuery.IncludeFreeResources &&
                           institutionResources != null &&
                           collectionManagementQuery.PublisherId == 0
            };

            var myCollection = new PageLink
            {
                Text = "My R2 Collection",
                Href = urlHelper.Action("List", "CollectionManagement",
                    new { query.InstitutionId, PurchasedOnly = true }),
                Selected = controller is CollectionManagementController &&
                           collectionManagementQuery.PurchasedOnly &&
                           collectionManagementQuery.ResourceListType == ResourceListType.All &&
                           institutionResources != null
            };

            var pdaCollection = new PageLink
            {
                Text = "My PDA Collection",
                Href = urlHelper.Action("List", "CollectionManagement",
                    new { query.InstitutionId, IncludePdaResources = true }),
                Selected = controller is CollectionManagementController &&
                           collectionManagementQuery.IncludePdaResources &&
                           !collectionManagementQuery.IncludePdaHistory &&
                           collectionManagementQuery.ResourceListType == ResourceListType.All &&
                           institutionResources != null
            };

            var pdaHistory = new PageLink
            {
                Text = "My PDA History",
                Href = urlHelper.Action("List", "CollectionManagement",
                    new { query.InstitutionId, IncludePdaResources = true, IncludePdaHistory = true }),
                Selected = controller is CollectionManagementController &&
                           collectionManagementQuery.IncludePdaResources &&
                           collectionManagementQuery.IncludePdaHistory &&
                           collectionManagementQuery.ResourceListType == ResourceListType.All &&
                           institutionResources != null
            };

            var pdaProfile = new PageLink
            {
                Text = "PDA Wizard",
                Href = urlHelper.Action("PdaProfile", "Pda", new { query.InstitutionId }),
                Selected = controller is PdaController,
                HoverText = "Use the Wizard to automatically add PDA titles to your collection."
            };


            PageLink reserveShelf = null;
            if (adminBaseModel.Institution != null && adminBaseModel.Institution.AccountStatus != null &&
                adminBaseModel.Institution.AccountStatus.Id == AccountStatus.Active)
            {
                reserveShelf = new PageLink
                {
                    Text = "Reserve Shelf",
                    Href = urlHelper.Action("List", "ReserveShelfManagement", new { institutionId }),
                    Selected = controller is ReserveShelfManagementController
                };
            }

            var pageLinkSection = new PageLinkSection { PageLinks = new List<PageLink> { pdaHistory, pdaProfile } };
            pdaCollection.ChildLinks = new[] { pageLinkSection };


            var pageLinks = new List<PageLink>
            {
                purchaseBooks,
                myCollection
            };

            if (adminBaseModel.IsRittenhouseAdmin || adminBaseModel.IsSalesAssociate ||
                adminBaseModel.IsInstitutionalAdmin)
            {
                var publishers = new PageLink
                {
                    Text = "By Publisher",
                    Href = urlHelper.Action("Publishers", "CollectionManagement", new { query.InstitutionId }),
                    Selected = controller is CollectionManagementController && collectionManagementQuery.PublisherId > 0
                };

                var expressCheckout = new PageLink
                {
                    Text = "Express Check Out",
                    Href = urlHelper.Action("Index", "ExpressCheckout", new { query.InstitutionId }),
                    Selected = adminBaseModel is ExpressCheckout
                };

                PageLink orderHistory = null;
                if (adminBaseModel.IsRittenhouseAdmin || adminBaseModel.IsSalesAssociate ||
                    adminBaseModel.IsInstitutionalAdmin)
                {
                    orderHistory = new PageLink
                    {
                        Text = "Order History",
                        Href = urlHelper.Action("List", "OrderHistory", new { institutionId }),
                        Selected = controller is OrderHistoryController
                    };
                }

                var childPageLinks = new List<PageLink> { publishers, expressCheckout };
                if (orderHistory != null)
                {
                    childPageLinks.Add(orderHistory);
                }

                purchaseBooks.ChildLinks = new[] { new PageLinkSection { PageLinks = childPageLinks } };
            }

            if (_webSettings.EnablePatronDrivenAcquisitions)
            {
                pageLinks.Add(pdaCollection);
            }


            if (reserveShelf != null)
            {
                pageLinks.Add(reserveShelf);
            }


            return pageLinks;
        }

        private IEnumerable<PageLink> GetAdminExpertReviewerLinks(UrlHelper urlHelper, ControllerBase controller,
            AdminBaseModel adminBaseModel)
        {
            var institutionId = adminBaseModel.InstitutionId;
            var institutionResources = adminBaseModel as InstitutionResources;
            var query = new CollectionManagementQuery
            {
                InstitutionId = institutionId,
                Query = "",
                SortBy = "",
                ResourceStatus = 0,
                ResourceFilterType = 0,
                PurchasedOnly = false
            };

            var collectionManagementQuery =
                institutionResources == null ? query : institutionResources.CollectionManagementQuery;

            var pageLinks = new List<PageLink>();

            var adminInsitution = _adminContext.GetAdminInstitution(institutionId);
            if (adminInsitution.ExpertReviewerUserEnabled)
            {
                var expertReviewerRequests = new PageLink
                {
                    Text = "Expert Reviewer Recommendations",
                    Href =
                        urlHelper.Action("List", "CollectionManagement",
                            new { query.InstitutionId, RecommendationsOnly = true }),
                    Selected =
                        controller is CollectionManagementController && institutionResources != null &&
                        collectionManagementQuery.RecommendationsOnly
                };

                var reviewLists = new PageLink
                {
                    Text = "Expert Reviewer Review Lists",
                    Href = urlHelper.Action("List", "Review", new { query.InstitutionId }),
                    Selected = controller is ReviewController
                };

                pageLinks.Add(expertReviewerRequests);
                pageLinks.Add(reviewLists);
            }

            return pageLinks;
        }

        private IEnumerable<PageLink> GetAdminCollectionLinks(UrlHelper urlHelper, ControllerBase controller,
            AdminBaseModel adminBaseModel)
        {
            var institutionId = adminBaseModel.InstitutionId;
            var institutionResources = adminBaseModel as InstitutionResources;
            var query = new CollectionManagementQuery
            {
                InstitutionId = institutionId,
                Query = "",
                SortBy = "",
                ResourceStatus = 0,
                ResourceFilterType = 0,
                PurchasedOnly = false
            };

            var collectionManagementQuery =
                institutionResources == null ? query : institutionResources.CollectionManagementQuery;
            var collectionServiceFactory = _collectionServiceFactory();
            var collectionLists = collectionServiceFactory.GetCollectionLists();
            var pageLinks = new List<PageLink>();

            foreach (var collectionList in collectionLists)
            {
                var selected = collectionManagementQuery.CollectionListFilter == collectionList.Id;
                query.CollectionListFilter = collectionList.Id;

                pageLinks.Add(new PageLink
                {
                    Text = collectionList.Name,
                    Href = urlHelper.Action("List", "CollectionManagement", query.ToRouteValues()),
                    Active = true,
                    Selected = selected
                });
            }


            var freeResources = new PageLink
            {
                Text = "Open Access Resources",
                Href = urlHelper.Action("List", "CollectionManagement",
                    new { query.InstitutionId, IncludeFreeResources = true }),
                Selected = controller is CollectionManagementController &&
                           collectionManagementQuery.IncludeFreeResources && institutionResources != null
            };
            pageLinks.Add(freeResources);

            return pageLinks;
        }

        private IEnumerable<PageLink> GetExpertReviewerCollectionManagementLinks(UrlHelper urlHelper,
            ControllerBase controller, AdminBaseModel adminBaseModel)
        {
            var institutionId = adminBaseModel.InstitutionId;
            var institutionResources = adminBaseModel as InstitutionResources;
            var query = new CollectionManagementQuery
            {
                InstitutionId = institutionId,
                Query = "",
                SortBy = "",
                ResourceStatus = 0,
                ResourceFilterType = 0,
                PurchasedOnly = false
            };

            var collectionManagementQuery =
                institutionResources == null ? query : institutionResources.CollectionManagementQuery;

            var purchaseBooks = new PageLink
            {
                Text = "Recommend eBooks",
                Href = urlHelper.Action("List", "CollectionManagement", new { query.InstitutionId }),
                Selected = controller is CollectionManagementController &&
                           !collectionManagementQuery.PurchasedOnly &&
                           !collectionManagementQuery.IncludePdaResources &&
                           !collectionManagementQuery.RecommendationsOnly &&
                           collectionManagementQuery.ResourceListType == ResourceListType.All &&
                           institutionResources != null
            };

            var featuredTitles = new PageLink
            {
                Text = "Featured Titles",
                Href = urlHelper.Action("List", "CollectionManagement",
                    new { query.InstitutionId, ResourceListType = ResourceListType.FeaturedTitles }),
                Selected = controller is CollectionManagementController &&
                           collectionManagementQuery.ResourceListType == ResourceListType.FeaturedTitles &&
                           institutionResources != null
            };

            var featuredPublisher = new PageLink
            {
                Text = "Featured Publisher",
                Href = urlHelper.Action("List", "CollectionManagement",
                    new { query.InstitutionId, ResourceListType = ResourceListType.FeaturedPublisher }),
                Selected = controller is CollectionManagementController &&
                           collectionManagementQuery.ResourceListType == ResourceListType.FeaturedPublisher &&
                           institutionResources != null
            };

            var pageLinks = new List<PageLink>
            {
                purchaseBooks,
                featuredTitles,
                featuredPublisher
            };

            if (adminBaseModel.Institution != null &&
                adminBaseModel.Institution.AccountStatus.Id == AccountStatus.Active)
            {
                var reserveShelf = new PageLink
                {
                    Text = "Reserve Shelf",
                    Href = urlHelper.Action("List", "ReserveShelfManagement", new { institutionId }),
                    Selected = controller is ReserveShelfManagementController
                };
                pageLinks.Add(reserveShelf);
            }

            var adminInsitution = _adminContext.GetAdminInstitution(institutionId);
            if (adminInsitution.ExpertReviewerUserEnabled)
            {
                var reviewLists = new PageLink
                {
                    Text = "Expert Reviewer Review Lists",
                    Href = urlHelper.Action("List", "Review", new { query.InstitutionId }),
                    Selected = controller is ReviewController
                };


                var facultyRequests = new PageLink
                {
                    Text = "Expert Reviewer Recommendations",
                    Href = urlHelper.Action("List", "CollectionManagement",
                        new { query.InstitutionId, RecommendationsOnly = true }),
                    Selected =
                        controller is CollectionManagementController && institutionResources != null &&
                        collectionManagementQuery.RecommendationsOnly,
                    ChildLinks = new[] { new PageLinkSection { PageLinks = new List<PageLink> { reviewLists } } }
                };

                pageLinks.Add(facultyRequests);
            }

            return pageLinks;
        }


        private IEnumerable<PageLink> GetInstitutionReportsLinks(UrlHelper urlHelper, ControllerBase controller,
            AdminBaseModel adminBaseModel, AuthenticatedInstitution authenticatedInstitution, string actionName)
        {
            var reportModel = adminBaseModel as ReportModel;
            var isMyReportsSelected = adminBaseModel is SavedReports || adminBaseModel is SavedReportDetail;

            var reportQuery = new ReportQuery
                { InstitutionId = adminBaseModel.InstitutionId, Period = ReportPeriod.Last30Days };
            var applicationUsageReport = new PageLink
            {
                Text = "Application Usage Report",
                Href = urlHelper.Action("ApplicationUsage", "Report", reportQuery.ToRouteValues()),
                Selected = controller is ReportController && actionName == "ApplicationUsage"
            };

            var resourceUsageReport = new PageLink
            {
                Text = "Resource Usage Report",
                Href = urlHelper.Action("ResourceUsage", "Report", reportQuery.ToRouteValues()),
                Selected = controller is ReportController && actionName == "ResourceUsage"
            };

            PageLink publisherUsageReport = null;
            if (reportModel != null && reportModel.InstitutionId == 0 && authenticatedInstitution.IsRittenhouseAdmin())
            {
                publisherUsageReport = new PageLink
                {
                    Text = "Publisher Usage Report",
                    Href = urlHelper.Action("PublisherUsage", "Report", reportQuery.ToRouteValues()),
                    Selected = controller is ReportController && actionName == "PublisherUsage"
                };
            }

            PageLink salesReport = null;
            if (reportModel != null && reportModel.InstitutionId == 0 && authenticatedInstitution.IsRittenhouseAdmin())
            {
                salesReport = new PageLink
                {
                    Text = "Sales Report",
                    Href = urlHelper.Action("SalesReport", "Report", reportQuery.ToRouteValues()),
                    Selected = controller is ReportController && actionName == "SalesReport"
                };
            }

            PageLink annualFeeReport = null;
            reportQuery.ReportTypeId = (int)ReportType.AnnualFeeReport;
            if (reportModel != null && reportModel.InstitutionId == 0 && authenticatedInstitution.IsRittenhouseAdmin())
            {
                annualFeeReport = new PageLink
                {
                    Text = "Annual Fee Report",
                    Href = urlHelper.Action("AnnualFeeReport", "Report", reportQuery.ToRouteValues()),
                    Selected = controller is ReportController && actionName == "AnnualFeeReport"
                };
            }

            PageLink pdaCountsReport = null;
            reportQuery.ReportTypeId = (int)ReportType.PdaCountsReport;
            if (reportModel != null && reportModel.InstitutionId == 0 && authenticatedInstitution.IsRittenhouseAdmin())
            {
                pdaCountsReport = new PageLink
                {
                    Text = "PDA Counts Report",
                    Href = urlHelper.Action("PdaCountsReport", "Report", reportQuery.ToRouteValues()),
                    Selected = controller is ReportController && actionName == "PdaCountsReport"
                };
            }

            PageLink myReports;
            if (reportModel != null && reportModel.InstitutionId == 0 && authenticatedInstitution.IsRittenhouseAdmin())
            {
                myReports = new PageLink
                {
                    Text = "My Reports",
                    Href = urlHelper.AdminAction<ReportController>(a => a.SavedReports(0)),
                    Selected = controller is ReportController && isMyReportsSelected
                };
            }
            else
            {
                myReports = new PageLink
                {
                    Text = "My Reports",
                    Href = urlHelper.AdminAction<ReportController>(a => a.SavedReports(adminBaseModel.InstitutionId)),
                    Selected = controller is ReportController && isMyReportsSelected
                };
            }

            var counterReports = new PageLink
            {
                Text = "Counter Reports",
                Href = urlHelper.Action("Index", "CounterReport", reportQuery.ToRouteValues()),
                Selected = controller is CounterReportController
            };

            var pageLinks = new List<PageLink> { applicationUsageReport, resourceUsageReport };

            if (publisherUsageReport != null)
            {
                pageLinks.Add(publisherUsageReport);
                if (salesReport != null)
                {
                    pageLinks.Add(salesReport);
                }

                if (annualFeeReport != null)
                {
                    pageLinks.Add(annualFeeReport);
                }

                if (pdaCountsReport != null)
                {
                    pageLinks.Add(pdaCountsReport);
                }

                pageLinks.Add(myReports);
                return pageLinks;
            }

            pageLinks.Add(myReports);

            if (reportQuery.InstitutionId != 0)
            {
                pageLinks.Add(counterReports);
                return pageLinks;
            }

            if (annualFeeReport != null)
            {
                pageLinks.Add(annualFeeReport);
            }

            if (pdaCountsReport != null)
            {
                pageLinks.Add(pdaCountsReport);
            }

            return pageLinks;
        }

        private IEnumerable<PageLink> GetMarketingLinks(UrlHelper urlHelper, ControllerBase controller,
            string actionName)
        {
            var cartCreate = new PageLink
            {
                Text = "Create Cart",
                Href = urlHelper.Action("AutomatedCartFilter", "Marketing"),
                Selected = controller is MarketingController && (actionName == "AutomatedCartFilter" ||
                                                                 actionName == "AutomatedCartSelected" ||
                                                                 actionName == "AutomatedCartsFinalize")
            };

            var cartHistory = new PageLink
            {
                Text = "Cart History",
                Href = urlHelper.Action("AutomatedCartHistoryList", "Marketing"),
                Selected = controller is MarketingController &&
                           (actionName == "AutomatedCartHistory" || actionName == "AutomatedCartHistoryList")
            };

            return new List<PageLink> { cartCreate, cartHistory };
        }

        private IEnumerable<PageLink> GetCacheLinks(UrlHelper urlHelper, ControllerBase controller, string actionName)
        {
            var cacheLink = new PageLink
            {
                Text = "Cache",
                Href = urlHelper.Action("Cache", "Marketing"),
                Selected = controller is MarketingController && (actionName == "Cache" || actionName == "CacheUpdate" ||
                                                                 actionName == "CacheUpdateItem")
            };

            return new List<PageLink> { cacheLink };
        }

        private IEnumerable<PageLink> GetSubscriptionLinks(UrlHelper urlHelper, ControllerBase controller,
            string actionName)
        {
            var cacheLink = new PageLink
            {
                Text = "Users",
                Href = urlHelper.Action("Users", "SubscriptionManagement"),
                Selected = controller is SubscriptionManagementController &&
                           (actionName == "Users" || actionName == "EditUser")
            };

            return new List<PageLink> { cacheLink };
        }

        private IEnumerable<PageLink> GetReportLinks(UrlHelper urlHelper, ControllerBase controller, string actionName)
        {
            var requestedResources = new PageLink
            {
                Text = "Request Access",
                Href = urlHelper.Action("RequestAccess", "Marketing"),
                Selected = controller is MarketingController && actionName == "RequestAccess"
            };

            return new List<PageLink> { requestedResources };
        }

        private IEnumerable<PageLink> GetDiscountReportsLinks(UrlHelper urlHelper, ControllerBase controller,
            string actionName)
        {
            var specialsReport = new PageLink
            {
                Text = "Specials Report",
                Href = urlHelper.Action("SpecialReport", "Report"),
                Selected = controller is ReportController && actionName == "SpecialReport"
            };


            var pdaPromotionsReport = new PageLink
            {
                Text = "PDA Promotions Report",
                Href = urlHelper.Action("PdaPromotionReport", "Report"),
                Selected = controller is ReportController && actionName == "PdaPromotionReport"
            };

            var promotionsReport = new PageLink
            {
                Text = "Promotions Report",
                Href = urlHelper.Action("PromotionReport", "Report"),
                Selected = controller is ReportController && actionName == "PromotionReport"
            };


            var pageLinks = new List<PageLink> { promotionsReport, pdaPromotionsReport, specialsReport };

            return pageLinks;
        }

        private static IEnumerable<PageLink> GetSystemInformationLinks(UrlHelper urlHelper, ControllerBase controller,
            string actionName, string codeName)
        {
            var howTo = new PageLink
            {
                Text = "R2 How To Videos",
                Href = "/Discover/R2HowTo",
                Selected = controller is CmsController
            };

            var linkName = "R2 Outreach";
            var outreach = new PageLink
            {
                Text = linkName,
                Href = urlHelper.Action("Index", "SystemInformation", new { codeName = linkName }),
                Selected = controller is SystemInformationController && actionName == "Index" && codeName == linkName
            };

            linkName = "Technical Documentation";
            var technicalDocumentation = new PageLink
            {
                Text = linkName,
                Href = urlHelper.Action("Index", "SystemInformation", new { codeName = linkName }),
                Selected = controller is SystemInformationController && actionName == "Index" && codeName == linkName
            };

            return new List<PageLink> { howTo, outreach, technicalDocumentation };
        }

        private IEnumerable<PageLink> GetAccessAndDiscoverabilityLinks(UrlHelper urlHelper, ControllerBase controller,
            AdminBaseModel adminBaseModel, string actionName, string codeName)
        {
            var institutionId = adminBaseModel.InstitutionId;

            var linkName = "Integration and Workflow Support";
            var integration = new PageLink
            {
                Text = linkName,
                Href = urlHelper.Action("Index", "AccessAndDiscoverability",
                    new { institutionId, codeName = linkName }),
                Selected = controller is AccessAndDiscoverabilityController && actionName == "Index" &&
                           codeName == linkName
            };

            linkName = "Access and Authentication";
            var access = new PageLink
            {
                Text = linkName,
                Href = urlHelper.Action("Index", "AccessAndDiscoverability",
                    new { institutionId, codeName = linkName }),
                Selected = true
            };

            var technicalDocumentation = new PageLink
            {
                Text = "Technical Documentation",
                Href = urlHelper.Action("Index", "AccessAndDiscoverability",
                    new { institutionId, codeName = linkName }),
                Selected = controller is AccessAndDiscoverabilityController && actionName == "Index" &&
                           codeName == linkName
            };
            var manageIpRanges = new PageLink
            {
                Text = "Manage Ip Ranges",
                Href = urlHelper.AdminAction<IpAddressRangeController>(a => a.List(institutionId)),
                Selected = controller is IpAddressRangeController
            };
            var manageReferrers = new PageLink
            {
                Text = "Manage Referrers",
                Href = urlHelper.AdminAction<InstitutionReferrerController>(a => a.List(institutionId)),
                Selected = controller is InstitutionReferrerController
            };

            var childPageLinks = new List<PageLink> { technicalDocumentation, manageIpRanges, manageReferrers };
            access.ChildLinks = new[] { new PageLinkSection { PageLinks = childPageLinks } };


            linkName = "Support and Discoverability Tutorials: R2 How-To Videos";
            var support = new PageLink
            {
                Text = linkName,
                Href = urlHelper.Action("Index", "AccessAndDiscoverability",
                    new { codeName = "r2_how_to_videos", isExternalLink = true }),
                Selected = controller is CmsController,
                Target = "_blank"
            };

            return new List<PageLink> { integration, access, support };
        }
    }
}