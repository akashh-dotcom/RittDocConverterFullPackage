#region

using R2V2.Core.Publisher;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Author;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public sealed class ResourceMap : BaseMap<Resource>
    {
        public ResourceMap()
        {
            // -- Table: tResource
            // -- Fields: iResourceId, vchResourceDesc, vchResourceTitle, vchResourceSubTitle, vchResourceAuthors, vchResourceAdditionalContributors, 
            // vchResourcePublisher, dtRISReleaseDate, dtResourcePublicationDate, tiBrandonHillStatus, vchResourceISBN, vchResourceEdition, 
            // decResourcePrice, decPayPerView, decSubScriptionPrice, vchResourceImageName, vchCopyRight, tiResourceReady, tiAllowSubscriptions, 
            // iPublisherId, iResourceStatusId, tiGloballyAccessible, vchCreatorId, dtCreationDate, vchUpdaterId, dtLastUpdate,
            // vchMARCRecord, vchResourceNLMCall, tiDrugMonograph, iDCTStatusId, tiDoodyReview, vchDoodyReviewURL, iPrevEditResourceID, vchAuthorXML, vchForthcomingDate

            Table("tResource");

            // -- Fields: iResourceId, vchResourceDesc, vchResourceTitle, vchResourceSubTitle, vchResourceAuthors, vchResourceAdditionalContributors, 
            Id(x => x.Id).Column("iResourceId").GeneratedBy.Identity();

            //Map(x => x.Description).Column("vchResourceDesc");
            Map(x => x.Description).Column("vchResourceDesc").CustomType("StringClob").CustomSqlType("nvarchar(max)");

            Map(x => x.Title).Column("vchResourceTitle");
            Map(x => x.SubTitle).Column("vchResourceSubTitle");
            Map(x => x.Authors).Column("vchResourceAuthors");
            Map(x => x.AdditionalContributors).Column("vchResourceAdditionalContributors");

            // vchResourcePublisher, dtRISReleaseDate, dtResourcePublicationDate, tiBrandonHillStatus, vchResourceISBN, vchResourceEdition, 
            References<Publisher>(x => x.Publisher).Column("iPublisherId").ReadOnly();
            Map(x => x.PublisherId).Column("iPublisherId");
            Map(x => x.ReleaseDate).Column("dtRISReleaseDate");
            Map(x => x.PublicationDate).Column("dtResourcePublicationDate");
            //Map(x => x.BrandonHillStatus).Column("tiBrandonHillStatus");
            Map(x => x.Isbn).Column("vchResourceISBN");
            Map(x => x.Edition).Column("vchResourceEdition");

            //// decResourcePrice, decPayPerView, decSubScriptionPrice, vchResourceImageName, vchCopyRight, tiResourceReady, tiAllowSubscriptions, 
            Map(x => x.ListPrice).Column("decResourcePrice");
            Map(x => x.BundlePrice3).Column("dec3BundlePrice");

            Map(x => x.ImageFileName).Column("vchResourceImageName");
            Map(x => x.Copyright).Column("vchCopyRight");

            //// iPublisherId, iResourceStatusId, tiGloballyAccessible,
            Map(x => x.StatusId).Column("iResourceStatusId");

            //// vchMARCRecord, vchResourceNLMCall, tiDrugMonograph, iDCTStatusId, tiDoodyReview, vchDoodyReviewURL, iPrevEditResourceID, vchAuthorXML, vchForthcomingDate
            Map(x => x.NlmCall).Column("vchResourceNLMCall");
            Map(x => x.DrugMonograph).Column("tiDrugMonograph");
            //Map(x => x.DctStatusId).Column("iDCTStatusId");
            Map(x => x.DoodyReview).Column("tiDoodyReview");
            //Map(x => x.PreviousEditResourceId).Column("iPrevEditResourceID");
            Map(x => x.LatestEditResourceId).Column("iLatestEditResourceId");
            Map(x => x.ForthcomingDate).Column("vchForthcomingDate");
            Map(x => x.NotSaleable).Column("NotSaleable");
            Map(x => x.ExcludeFromAutoArchive).Column("tiExcludeFromAutoArchive");

            Map(x => x.TabersStatus).Column("tiTabersStatus");

            Map(x => x.ContainsVideo).Column("tiContainsVideo");

            Map(x => x.SortTitle).Column("vchResourceSortTitle");
            Map(x => x.SortAuthor).Column("vchResourceSortAuthor");
            Map(x => x.AlphaKey).Column("chrAlphaKey");

            Map(x => x.Isbn10).Column("vchIsbn10");
            Map(x => x.Isbn13).Column("vchIsbn13");
            Map(x => x.EIsbn).Column("vchEIsbn");

            Map(x => x.QaApprovalDate).Column("dtQaApprovalDate");
            Map(x => x.LastPromotionDate).Column("dtLastPromotionDate");

            Map(x => x.PageCount).Column("vchPageCount");

            Map(x => x.Affiliation).Column("vchAffiliation");
            Map(x => x.AffiliationUpdatedByPrelude).Column("tiAffiliationUpdatedByPrelude");

            Map(x => x.NotSaleableDate).Column("dtNotSaleableDate");
            Map(x => x.IsFreeResource).Column("tiFreeResource");
            Map(x => x.DoodyRating).Column("siDoodyRating");
            Map(x => x.ArchiveDate).Column("dtArchiveDate");

            HasMany(x => x.ResourceSpecialties).KeyColumn("iResourceId").AsBag().Cascade.AllDeleteOrphan()
                .ApplyFilter<SoftDeleteFilter>();
            HasMany(x => x.ResourcePracticeAreas).KeyColumn("iResourceId").AsBag().Cascade.AllDeleteOrphan()
                .ApplyFilter<SoftDeleteFilter>();

            HasMany(x => x.InstitutionResourceLicenses).KeyColumn("iResourceId");
            HasMany<Author>(x => x.AuthorList).KeyColumn("iResourceId");

            HasMany(x => x.ResourceCollections).KeyColumn("iResourceId").AsBag().Cascade.AllDeleteOrphan()
                .ApplyFilter<SoftDeleteFilter>();
        }
    }
}