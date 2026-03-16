#region

using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.Recommendations;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Discipline;

#endregion

namespace R2V2.Core.Reports
{
    public class InstitutionEmailStatistics : InstitutionStatistics
    {
        public IResource MostAccessedResource { get; set; }
        public IResource LeastAccessedResource { get; set; }
        public IResource MostTurnawayAccessResource { get; set; }
        public IResource MostTurnawayConcurrentResource { get; set; }

        public List<InstitutionResourceStatistics> InstitutionResourceStatistics { get; set; }

        public string MostPopularSpecialtyNameOfYear { get; set; }
        public int MostPopularSpecialtyCountOfYear { get; set; }

        public int MostPopularSpecialtyId { get; set; }
        public int MostPopularSpecialtyIdOfYear { get; set; }

        public List<string> QuickNotes { get; set; }

        public InstitutionAccountUsage YearAccountUsage { get; set; }

        public void PopulateResources(List<IResource> resources)
        {
            if (Highlights != null)
            {
                MostAccessedResource = Highlights.MostAccessedResourceId > 0
                    ? resources.FirstOrDefault(x => x.Id == Highlights.MostAccessedResourceId)
                    : null;
                LeastAccessedResource = Highlights.LeastAccessedResourceId > 0
                    ? resources.FirstOrDefault(x => x.Id == Highlights.LeastAccessedResourceId)
                    : null;
                MostTurnawayAccessResource = Highlights.MostTurnawayAccessResourceId > 0
                    ? resources.FirstOrDefault(x => x.Id == Highlights.MostTurnawayAccessResourceId)
                    : null;
                MostTurnawayConcurrentResource = Highlights.MostTurnawayConcurrentResourceId > 0
                    ? resources.FirstOrDefault(x => x.Id == Highlights.MostTurnawayConcurrentResourceId)
                    : null;
            }
        }

        public void PopulateFeaturedTitles(List<IResource> resources, List<IFeaturedTitle> featuredTitles,
            decimal institutionDiscont)
        {
            if (featuredTitles != null && featuredTitles.Any() && resources.Any())
            {
                var featuredTitleResources =
                    resources.Where(x => featuredTitles.Select(ft => ft.ResourceId).Contains(x.Id));
                FeaturedTitleResources = new List<KeyValuePair<IResource, decimal>>();

                foreach (var featuredTitleResource in featuredTitleResources)
                {
                    var discountPrice = featuredTitleResource.ListPrice -
                                        institutionDiscont / 100 * featuredTitleResource.ListPrice;
                    FeaturedTitleResources.Add(
                        new KeyValuePair<IResource, decimal>(featuredTitleResource, discountPrice));
                }
                //FeaturedTitleResources = new List<IResource>();
                //FeaturedTitleResources.AddRange(featuredTitleResources);
            }
        }

        public void PopulateSpecialResources(List<IResource> resources, List<SpecialResource> specialsResources)
        {
            if (specialsResources != null && specialsResources.Any() && resources.Any())
            {
                CurrentSpecialResources = (from r in resources
                    join sr in specialsResources on r.Id equals sr.ResourceId
                    orderby r.PublicationDate descending
                    select new { r, sr }).ToDictionary(t => t.r, t => t.sr.ToDiscount(t.r)).ToList();

                if (CurrentSpecialResources.Count > 4)
                {
                    CurrentSpecialResources =
                        CurrentSpecialResources.Take(4).ToDictionary(x => x.Key, x => x.Value).ToList();
                }
            }
        }

        public void PopulateSpecialtyIds(List<Specialty> specialties)
        {
            if (!string.IsNullOrWhiteSpace(MostPopularSpecialtyNameOfYear))
            {
                var specialty = specialties.FirstOrDefault(x => x.Name == MostPopularSpecialtyNameOfYear);
                if (specialty != null)
                {
                    MostPopularSpecialtyIdOfYear = specialty.Id;
                }
            }

            if (Highlights != null && !string.IsNullOrWhiteSpace(Highlights.MostPopularSpecialtyName))
            {
                var specialty = specialties.FirstOrDefault(x => x.Name == Highlights.MostPopularSpecialtyName);
                if (specialty != null)
                {
                    MostPopularSpecialtyId = specialty.Id;
                }
            }
        }

        public void PopulateRecommendations(List<IResource> resources, List<Recommendation> recommendations)
        {
            if (recommendations != null && recommendations.Any() && resources.Any())
            {
                var expertRecommendedResources =
                    resources.Where(x => recommendations.Select(ft => ft.ResourceId).Contains(x.Id));
                ExpertRecommendedResources = new List<IResource>();
                ExpertRecommendedResources.AddRange(expertRecommendedResources.Count() > 5
                    ? expertRecommendedResources.Take(5)
                    : expertRecommendedResources);
            }
        }

        public string ToDebugString(IUser user)
        {
            var sb = new StringBuilder("InstitutionEmailStatistics = [");
            sb.AppendFormat("InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", UserId: {0}", user.Id);
            sb.AppendFormat(", User Role: {0}", user.Role.Code);

            sb.AppendFormat(", MostAccessedResourceId: {0}", Highlights.MostAccessedResourceId);
            sb.AppendFormat(", MostAccessedCount: {0}", Highlights.MostAccessedCount);
            sb.AppendFormat(", LeastAccessedResourceId: {0}", Highlights.LeastAccessedResourceId);
            sb.AppendFormat(", LeastAccessedCount: {0}", Highlights.LeastAccessedCount);
            sb.AppendFormat(", MostTurnawayAccessResourceId: {0}", Highlights.MostTurnawayAccessResourceId);
            sb.AppendFormat(", MostTurnawayAccessCount: {0}", Highlights.MostTurnawayAccessCount);
            sb.AppendFormat(", MostTurnawayConcurrentResourceId: {0}", Highlights.MostTurnawayConcurrentResourceId);
            sb.AppendFormat(", MostTurnawayConcurrentCount: {0}", Highlights.MostTurnawayConcurrentCount);
            sb.AppendFormat(", MostPopularSpecialtyName: {0}", Highlights.MostPopularSpecialtyName);
            sb.AppendFormat(", MostPopularSpecialtyCount: {0}", Highlights.MostPopularSpecialtyCount);
            sb.AppendFormat(", LeastPopularSpecialtyName: {0}", Highlights.LeastPopularSpecialtyName);
            sb.AppendFormat(", LeastPopularSpecialtyCount: {0}", Highlights.LeastPopularSpecialtyCount);
            sb.AppendFormat(", TotalResourceCount: {0}", Highlights.TotalResourceCount).AppendLine().Append("\t");
            ;
            sb.AppendFormat(", ContentCount: {0}", AccountUsage.ContentCount);
            sb.AppendFormat(", TocCount: {0}", AccountUsage.TocCount);
            sb.AppendFormat(", SessionCount: {0}", AccountUsage.SessionCount);
            sb.AppendFormat(", PrintCount: {0}", AccountUsage.PrintCount);
            sb.AppendFormat(", EmailCount: {0}", AccountUsage.EmailCount);
            sb.AppendFormat(", TurnawayConcurrencyCount: {0}", AccountUsage.TurnawayConcurrencyCount);
            sb.AppendFormat(", TurnawayAccessCount: {0}", AccountUsage.TurnawayAccessCount);

            sb.AppendFormat(", MostAccessedResource: {0}",
                MostAccessedResource != null ? MostAccessedResource.Id.ToString() : "N/A");
            sb.AppendFormat(", LeastAccessedResource: {0}",
                LeastAccessedResource != null ? LeastAccessedResource.Id.ToString() : "N/A");
            sb.AppendFormat(", MostTurnawayAccessResource: {0}",
                MostTurnawayAccessResource != null ? MostTurnawayAccessResource.Id.ToString() : "N/A");
            sb.AppendFormat(", MostTurnawayConcurrentResource: {0}",
                MostTurnawayConcurrentResource != null ? MostTurnawayConcurrentResource.Id.ToString() : "N/A");


            sb.Append(", FeaturedTitleResources = [");
            if (FeaturedTitleResources != null)
            {
                foreach (var resource in FeaturedTitleResources)
                {
                    sb.AppendFormat(", ResourceId: {0} || Price: {1} ", resource.Key.Id, resource.Value);
                }
            }

            sb.Append("] ");

            sb.Append(", CurrentSpecialResources = [");
            if (CurrentSpecialResources != null)
            {
                foreach (var resource in CurrentSpecialResources)
                {
                    sb.AppendFormat(", ResourceId: {0} ", resource.Key.Id);
                }
            }

            sb.Append("] ");

            sb.Append(", ExpertRecommendedResources = [");
            if (ExpertRecommendedResources != null)
            {
                foreach (var resource in ExpertRecommendedResources)
                {
                    sb.AppendFormat(", ResourceId: {0} ", resource.Id);
                }
            }

            sb.Append("]");
            return sb.ToString();
        }

        #region "Always Current"

        public List<KeyValuePair<IResource, decimal>> CurrentSpecialResources { get; set; }

        public List<KeyValuePair<IResource, decimal>> FeaturedTitleResources { get; set; }

        //public List<IResource> FeaturedTitleResources { get; set; }
        public List<IResource> ExpertRecommendedResources { get; set; }

        #endregion
    }

    static class EmailDiscount
    {
        public static decimal ToDiscount(this SpecialResource special, IResource resource)
        {
            //decimal institutionDiscountPrice = resource.ListPrice - ((cart.Discount/100)*resource.ListPrice);                            
            var discountPrice = resource.ListPrice -
                                (decimal)special.Discount.DiscountPercentage / 100 * resource.ListPrice;
            //DiscountPrice = Resource.ListPrice - (discount / 100) * Resource.ListPrice;

            return discountPrice;
        }
    }
}