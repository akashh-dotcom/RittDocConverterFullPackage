#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Core.Publisher;
using R2V2.Core.Resource.Author;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;

#endregion

namespace R2V2.Core.Resource
{
    [Serializable]
    public class CachedResource : IResource
    {
        private readonly List<ICollection> _collections = new List<ICollection>();
        private readonly List<IPracticeArea> _practiceAreas = new List<IPracticeArea>();
        private readonly List<ISpecialty> _specialties = new List<ISpecialty>();

        public CachedResource(IResource resource
            , IDictionary<int, Resource> allResources
            , IDictionary<int, IPublisher> publishers
            , ILookup<int, IAuthor> authors
            , ILookup<int?, ResourcePracticeArea> resourcePracticeAreas
            , ILookup<int?, ResourceSpecialty> resourceSpecialties
            , Dictionary<int, ResourceFileDocIds> resourceFileDocIds
            , string imageBaseUrl
            //, ILookup<int, Resource> newEditionResources
            , decimal resourceMinimumListPrice
            , ILookup<int?, ResourceCollection> resourceCollections
        )
        {
            Id = resource.Id;
            Title = resource.Title;

            SubTitle = resource.SubTitle;
            Authors = resource.Authors;
            Description = resource.Description;
            AdditionalContributors = resource.AdditionalContributors;
            ReleaseDate = resource.ReleaseDate;
            PublicationDate = resource.PublicationDate;
            Isbn = resource.Isbn;
            Edition = resource.Edition;
            ListPrice = resource.ListPrice;
            BundlePrice3 = resource.BundlePrice3;
            ImageFileName = resource.ImageFileName;
            ImageUrl = resource.ImageUrl;
            Copyright = resource.Copyright;

            PublisherId = resource.PublisherId;
            StatusId = resource.StatusId;

            RecordStatus = resource.RecordStatus;
            NlmCall = resource.NlmCall;
            DrugMonograph = resource.DrugMonograph;
            DoodyReview = resource.DoodyReview;
            ForthcomingDate = resource.ForthcomingDate;

            TabersStatus = resource.TabersStatus;
            NotSaleable = resource.NotSaleable;
            ExcludeFromAutoArchive = resource.ExcludeFromAutoArchive;

            SortTitle = resource.SortTitle;
            SortAuthor = resource.SortAuthor;
            AlphaKey = resource.AlphaKey;

            Isbn10 = resource.Isbn10;
            Isbn13 = resource.Isbn13;
            EIsbn = resource.EIsbn;

            QaApprovalDate = resource.QaApprovalDate;
            LastPromotionDate = resource.LastPromotionDate;

            PageCount = resource.PageCount;

            Affiliation = resource.Affiliation;
            AffiliationUpdatedByPrelude = resource.AffiliationUpdatedByPrelude;
            CreatedBy = resource.CreatedBy;
            CreationDate = resource.CreationDate;
            UpdatedBy = resource.UpdatedBy;
            LastUpdated = resource.LastUpdated;

            DocumentIdMin = resource.DocumentIdMin;
            DocumentIdMax = resource.DocumentIdMax;

            Publisher = publishers.ContainsKey(resource.PublisherId)
                ? new CachedPublisher(publishers[resource.PublisherId])
                : null;
            LatestEditResourceId = resource.LatestEditResourceId;

            ContainsVideo = resource.ContainsVideo;

            DoodyRating = resource.DoodyRating;
            ArchiveDate = resource.ArchiveDate;

            var authorList = authors[Id];
            IList<IAuthor> resourceAuthors =
                authorList.Select(author => new CachedAuthor(author)).Cast<IAuthor>().ToList();
            AuthorList = resourceAuthors;

            ImageUrl = string.IsNullOrWhiteSpace(ImageFileName) ? "" : $"{imageBaseUrl}/{ImageFileName}";
            if (LatestEditResourceId.HasValue && LatestEditResourceId.Value > 0 &&
                allResources.ContainsKey(LatestEditResourceId.Value))
            {
                NewEditionResourceIsbn = allResources[LatestEditResourceId.Value].Isbn;
            }

            IsFreeResource = resource.IsFreeResource;
            NotSaleableDate = resource.NotSaleableDate;

            IsForSale =
                (StatusId == (int)ResourceStatus.Active || resource.StatusId == (int)ResourceStatus.Forthcoming) &&
                !resource.NotSaleable;

            if (resource.ListPrice < resourceMinimumListPrice && !IsFreeResource)
            {
                IsForSale = false;
            }

            var specialties = resourceSpecialties[Id];
            foreach (var resourceSpecialty in specialties)
            {
                _specialties.Add(new CachedSpecialty(resourceSpecialty.Specialty));
            }

            var practiceAreas = resourcePracticeAreas[Id];
            foreach (var resourcePracticeArea in practiceAreas)
            {
                _practiceAreas.Add(new CachedPracticeArea(resourcePracticeArea.PracticeArea));
            }

            var collections = resourceCollections[Id];
            foreach (var resourceCollection in collections)
            {
                _collections.Add(new CachedCollection(resourceCollection.Collection));
            }

            if (resourceFileDocIds.ContainsKey(resource.Id))
            {
                var fileIds = resourceFileDocIds[resource.Id];
                DocumentIdMax = fileIds.MaxDocumentId;
                DocumentIdMin = fileIds.MinDocumentId;
            }
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Authors { get; set; }
        public string Description { get; set; }
        public IEnumerable<IAuthor> AuthorList { get; set; }
        public string AdditionalContributors { get; set; }
        public IPublisher Publisher { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public DateTime? PublicationDate { get; set; }
        public string Isbn { get; set; }
        public string Edition { get; set; }
        public decimal ListPrice { get; set; }
        public decimal? BundlePrice3 { get; set; }
        public string ImageFileName { get; set; }
        public string ImageUrl { get; set; }
        public string Copyright { get; set; }

        public int PublisherId { get; set; }
        public int StatusId { get; set; }

        public bool RecordStatus { get; set; }
        public string NlmCall { get; set; }
        public short DrugMonograph { get; set; }
        public short DoodyReview { get; set; }
        public int? LatestEditResourceId { get; set; }
        public string ForthcomingDate { get; set; }

        public bool TabersStatus { get; set; }
        public bool NotSaleable { get; set; }
        public bool ExcludeFromAutoArchive { get; set; }

        public string SortTitle { get; set; }
        public string SortAuthor { get; set; }
        public string AlphaKey { get; set; }

        public string Isbn10 { get; set; }
        public string Isbn13 { get; set; }
        public string EIsbn { get; set; }

        public string Affiliation { get; set; }
        public bool AffiliationUpdatedByPrelude { get; set; }
        public DateTime? QaApprovalDate { get; set; }
        public DateTime? LastPromotionDate { get; set; }

        public byte ContainsVideo { get; set; }
        public bool IsFreeResource { get; set; }
        public DateTime? NotSaleableDate { get; set; }
        public bool IsForSale { get; set; }

        public IEnumerable<ISpecialty> Specialties => _specialties;

        public IEnumerable<IPracticeArea> PracticeAreas => _practiceAreas;

        public IEnumerable<ICollection> Collections => _collections;

        public string PageCount { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }

        public int DocumentIdMin { get; set; }
        public int DocumentIdMax { get; set; }

        public string NewEditionResourceIsbn { get; set; }

        public short DoodyRating { get; set; }
        public DateTime? ArchiveDate { get; set; }

        public string GetGist(int maxLength)
        {
            if (string.IsNullOrEmpty(Description))
            {
                return string.Empty;
            }

            if (Description.Length < maxLength)
            {
                return Description;
            }

            var gist = Description.Substring(0, maxLength);
            for (var i = maxLength - 1 - 1; i >= 0; i--)
            {
                if (gist[i] == ' ')
                {
                    return $"{gist.Substring(0, i)}...";
                }
            }

            return gist;
        }

        public bool IsActive()
        {
            return StatusId == 6;
        }

        public bool IsArchive()
        {
            return StatusId == 7;
        }

        public bool IsDisabled()
        {
            return StatusId == 72;
        }

        public bool IsForthcoming()
        {
            return StatusId == 8;
        }


        public string ToDebugInfo()
        {
            var sb = new StringBuilder("CachedResource = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", Isbn: {0}", Isbn);
            sb.AppendFormat(", Title: {0}", Title);
            sb.AppendFormat(", StatusId: {0}", StatusId);
            sb.Append("]");
            return sb.ToString();
        }

        public string SpecialtiesToString()
        {
            var specialties = new List<string>();

            foreach (var specialty in _specialties.Where(specialty => !specialties.Contains(specialty.Name)))
            {
                specialties.Add(specialty.Name);
            }

            var sb = new StringBuilder();
            foreach (var specialty in specialties)
            {
                sb.AppendFormat("{0}{1}", sb.Length > 0 ? ", " : string.Empty, specialty);
            }

            return sb.ToString();
        }

        public string PracticeAreasToString()
        {
            var practiceAreas = new List<string>();

            foreach (var practiceArea in
                     _practiceAreas.Where(practiceArea => !practiceAreas.Contains(practiceArea.Name)))
            {
                practiceAreas.Add(practiceArea.Name);
            }

            var sb = new StringBuilder();
            foreach (var practiceArea in practiceAreas)
            {
                sb.AppendFormat("{0}{1}", sb.Length > 0 ? ", " : string.Empty, practiceArea);
            }

            return sb.ToString();
        }

        public virtual string CollectionsToString()
        {
            var collections = new List<string>();

            foreach (var collection in Collections.Where(collection => !collections.Contains(collection.Name)))
            {
                collections.Add(collection.Name);
            }

            var sb = new StringBuilder();
            foreach (var collection in collections)
            {
                sb.AppendFormat("{0}{1}", sb.Length > 0 ? ", " : string.Empty, collection);
            }

            return sb.ToString();
        }

        public int[] CollectionIdsToArray()
        {
            var collections = new List<int>();

            foreach (var collection in Collections.Where(collection => !collections.Contains(collection.Id)))
            {
                collections.Add(collection.Id);
            }

            return collections.ToArray();
        }

        /// <summary>
        ///     This method was added to see if this had any affect on memory usage. It does not appear to but I'm leaving it here
        ///     for now.
        ///     SJS - 10/10/2013
        /// </summary>
        public void ClearLists()
        {
            _collections.Clear();
            _practiceAreas.Clear();
            _specialties.Clear();
        }
    }
}