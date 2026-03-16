#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R2V2.Core.Institution;
using R2V2.Core.Publisher;
using R2V2.Core.Resource.Author;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource
{
    public class Resource : AuditableEntity, ISoftDeletable, IResource
    {
        public Resource()
        {
            ResourceSpecialties = new List<ResourceSpecialty>();
            ResourcePracticeAreas = new List<ResourcePracticeArea>();
            ResourceCollections = new List<ResourceCollection>();
        }

        public virtual IEnumerable<InstitutionResourceLicense> InstitutionResourceLicenses { get; set; }

        public virtual IList<ResourceSpecialty> ResourceSpecialties { get; set; }
        public virtual IList<ResourcePracticeArea> ResourcePracticeAreas { get; set; }

        public virtual IList<ResourceCollection> ResourceCollections { get; set; }
        // -- Table: tResource
        // -- Fields: iResourceId, vchResourceDesc, vchResourceTitle, vchResourceSubTitle, vchResourceAuthors, vchResourceAdditionalContributors,
        // vchResourcePublisher, dtRISReleaseDate, dtResourcePublicationDate, tiBrandonHillStatus, vchResourceISBN, vchResourceEdition,
        // decResourcePrice, decPayPerView, decSubScriptionPrice, vchResourceImageName, vchCopyRight, tiResourceReady, tiAllowSubscriptions,
        // iPublisherId, iResourceStatusId, tiGloballyAccessible, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate,
        // vchMARCRecord, vchResourceNLMCall, tiDrugMonograph, iDCTStatusId, tiDoodyReview, vchDoodyReviewURL, iPrevEditResourceID, vchAuthorXML, vchForthcomingDate, NotSaleable,
        // tiExcludeFromAutoArchive

        public virtual string Description { get; set; }
        public virtual string Title { get; set; }
        public virtual string SubTitle { get; set; }
        public virtual string Authors { get; set; }
        public virtual IEnumerable<IAuthor> AuthorList { get; set; }
        public virtual string AdditionalContributors { get; set; }
        public virtual IPublisher Publisher { get; set; }
        public virtual DateTime? ReleaseDate { get; set; }
        public virtual DateTime? PublicationDate { get; set; }
        public virtual string Isbn { get; set; }
        public virtual string Edition { get; set; }
        public virtual decimal ListPrice { get; set; }
        public virtual decimal? BundlePrice3 { get; set; }
        public virtual string ImageFileName { get; set; }
        public virtual string ImageUrl { get; set; }
        public virtual string Copyright { get; set; }

        public virtual string Affiliation { get; set; }
        public virtual bool AffiliationUpdatedByPrelude { get; set; }

        public virtual int PublisherId { get; set; }
        public virtual int StatusId { get; set; }
        public virtual string NlmCall { get; set; }
        public virtual short DrugMonograph { get; set; }
        public virtual short DoodyReview { get; set; }
        public virtual int? LatestEditResourceId { get; set; }
        public virtual string ForthcomingDate { get; set; }

        public virtual bool TabersStatus { get; set; }
        public virtual bool NotSaleable { get; set; }
        public virtual bool ExcludeFromAutoArchive { get; set; }

        public virtual byte ContainsVideo { get; set; }

        public virtual bool IsFreeResource { get; set; }

        public virtual string SortTitle { get; set; }
        public virtual string SortAuthor { get; set; }
        public virtual string AlphaKey { get; set; }

        public virtual string Isbn10 { get; set; }
        public virtual string Isbn13 { get; set; }
        public virtual string EIsbn { get; set; }
        public virtual string NewEditionResourceIsbn { get; set; }

        public virtual DateTime? QaApprovalDate { get; set; }
        public virtual DateTime? LastPromotionDate { get; set; }
        public virtual DateTime? NotSaleableDate { get; set; }
        public virtual bool IsForSale { get; set; }
        public virtual short DoodyRating { get; set; }
        public virtual DateTime? ArchiveDate { get; set; }

        public virtual IEnumerable<ISpecialty> Specialties
        {
            get
            {
                return ResourceSpecialties.Select(x => new CachedSpecialty(x.Specialty)).Cast<ISpecialty>().ToList();
            }
        }

        public virtual IEnumerable<IPracticeArea> PracticeAreas
        {
            get
            {
                return ResourcePracticeAreas.Select(x => new CachedPracticeArea(x.PracticeArea)).Cast<IPracticeArea>()
                    .ToList();
            }
        }

        public virtual IEnumerable<ICollection> Collections
        {
            get
            {
                return ResourceCollections.Select(x => new CachedCollection(x.Collection)).Cast<ICollection>().ToList();
            }
        }

        public virtual string PageCount { get; set; }

        public virtual int DocumentIdMin { get; set; }
        public virtual int DocumentIdMax { get; set; }

        public virtual string GetGist(int maxLength)
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

        public virtual bool IsActive()
        {
            return StatusId == 6;
        }

        public virtual bool IsArchive()
        {
            return StatusId == 7;
        }

        public virtual bool IsDisabled()
        {
            return StatusId == 72;
        }

        public virtual bool IsForthcoming()
        {
            return StatusId == 8;
        }

        public virtual string ToDebugInfo()
        {
            var sb = new StringBuilder("Resource = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", Isbn: {0}", Isbn);
            sb.AppendFormat(", Title: {0}", Title);
            sb.AppendFormat(", StatusId: {0}", StatusId);
            sb.Append("]");
            return sb.ToString();
        }

        public virtual string SpecialtiesToString()
        {
            var specialties = new List<string>();

            foreach (var specialty in Specialties.Where(specialty => !specialties.Contains(specialty.Name)))
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

        public virtual string PracticeAreasToString()
        {
            var practiceAreas = new List<string>();

            foreach (var practiceArea in
                     PracticeAreas.Where(practiceArea => !practiceAreas.Contains(practiceArea.Name)))
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

        public virtual int[] CollectionIdsToArray()
        {
            var collections = new List<int>();

            foreach (var collection in Collections.Where(collection => !collections.Contains(collection.Id)))
            {
                collections.Add(collection.Id);
            }

            return collections.ToArray();
        }

        public virtual bool RecordStatus { get; set; }
    }
}