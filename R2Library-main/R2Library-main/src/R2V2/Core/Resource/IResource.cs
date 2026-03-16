#region

using System;
using System.Collections.Generic;
using R2V2.Core.Publisher;
using R2V2.Core.Resource.Author;
using R2V2.Core.Resource.Collection;
using R2V2.Core.Resource.Discipline;
using R2V2.Core.Resource.PracticeArea;

#endregion

namespace R2V2.Core.Resource
{
    public interface IResource
    {
        string Description { get; set; }
        string Title { get; set; }
        string SubTitle { get; set; }
        string Authors { get; set; }
        IEnumerable<IAuthor> AuthorList { get; set; }
        string AdditionalContributors { get; set; }
        IPublisher Publisher { get; set; }
        DateTime? ReleaseDate { get; set; }

        DateTime? PublicationDate { get; set; }

        //short BrandonHillStatus { get; set; }
        string Isbn { get; set; }
        string Edition { get; set; }
        decimal ListPrice { get; set; }
        decimal? BundlePrice3 { get; set; }
        string ImageFileName { get; set; }
        string ImageUrl { get; set; }
        string Copyright { get; set; }
        int PublisherId { get; set; }
        int StatusId { get; set; }
        bool RecordStatus { get; set; }
        string NlmCall { get; set; }

        short DrugMonograph { get; set; }

        //int DctStatusId { get; set; }
        short DoodyReview { get; set; }

        //int? PreviousEditResourceId { get; set; }
        int? LatestEditResourceId { get; set; }
        string ForthcomingDate { get; set; }

        bool TabersStatus { get; set; }

        string CreatedBy { get; set; }
        DateTime CreationDate { get; set; }
        string UpdatedBy { get; set; }
        DateTime? LastUpdated { get; set; }
        int Id { get; set; }
        bool NotSaleable { get; set; }
        bool ExcludeFromAutoArchive { get; set; }

        string SortTitle { get; set; }
        string SortAuthor { get; set; }
        string AlphaKey { get; set; }

        string Isbn10 { get; set; }
        string Isbn13 { get; set; }
        string EIsbn { get; set; }

        string NewEditionResourceIsbn { get; set; }

        string Affiliation { get; set; }
        bool AffiliationUpdatedByPrelude { get; set; }
        DateTime? QaApprovalDate { get; set; }
        DateTime? LastPromotionDate { get; set; }

        IEnumerable<ISpecialty> Specialties { get; }
        IEnumerable<IPracticeArea> PracticeAreas { get; }

        IEnumerable<ICollection> Collections { get; }
        //IEnumerable<int> CollectionIds { get; }

        string PageCount { get; set; }

        int DocumentIdMin { get; set; }
        int DocumentIdMax { get; set; }

        byte ContainsVideo { get; set; }

        bool IsFreeResource { get; set; }
        DateTime? NotSaleableDate { get; set; }

        bool IsForSale { get; set; }

        short DoodyRating { get; set; }

        DateTime? ArchiveDate { get; set; }

        string GetGist(int maxLength);

        bool IsActive();
        bool IsArchive();
        bool IsDisabled();
        bool IsForthcoming();

        string ToDebugInfo();
        string SpecialtiesToString();
        string PracticeAreasToString();
        string CollectionsToString();

        int[] CollectionIdsToArray();
    }
}