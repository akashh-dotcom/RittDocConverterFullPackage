#region

using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Transform;
using R2V2.Core.CollectionManagement;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Reports
{
    public class DiscountReportService
    {
        private readonly ILog<DiscountReportService> _log;
        private readonly IQueryable<PdaPromotion> _pdaPromotions;
        private readonly IQueryable<CollectionManagement.Promotion> _promotions;
        private readonly IQueryable<Special> _specials;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public DiscountReportService(
            ILog<DiscountReportService> log
            , IUnitOfWorkProvider unitOfWorkProvider
            , IQueryable<Special> specials
            , IQueryable<PdaPromotion> pdaPromotions
            , IQueryable<CollectionManagement.Promotion> promotions
        )
        {
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
            _specials = specials;
            _pdaPromotions = pdaPromotions;
            _promotions = promotions;
        }

        public List<Special> GetSpecials()
        {
            var sql = new StringBuilder()
                .Append("select s.iSpecialId ")
                .Append("from tCartItem ci ")
                .Append("join tCart c on ci.iCartId = c.iCartId ")
                .Append("join tResource r on ci.iResourceId = r.iResourceId ")
                .Append("join tPublisher p on r.iPublisherId = p.iPublisherId ")
                .Append("join tInstitution i on c.iInstitutionId = i.iInstitutionId ")
                .Append("join tSpecial s on ci.vchSpecialText like s.vchName + '%' ")
                .Append("join tSpecialDiscount sd on s.iSpecialId = sd.iSpecialId ")
                .Append(
                    "where c.tiProcessed = 1 and ci.iNumberOfLicenses > 0 and vchSpecialText is not null and ci.tiRecordStatus = 1 ")
                .Append("and s.tiRecordStatus = 1 and sd.tiRecordStatus = 1 ")
                .Append("group by s.iSpecialId ")
                .ToString();
            var specialIds = new List<int>();
            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);
                var results = query.List();

                foreach (var result in results)
                {
                    specialIds.Add(ConvertObjectToInt(result));
                }
            }

            return specialIds.Any() ? _specials.Where(x => specialIds.Contains(x.Id)).ToList() : null;
        }

        public Special GetSpecial(int id)
        {
            return _specials.FirstOrDefault(x => x.Id == id);
        }


        public List<DiscountResource> GetSpecialsReport(int specialId)
        {
            var sql = new StringBuilder()
                .Append("select sd.iDiscountPercentage as 'DiscountPercentage', ")
                .Append(
                    "i.vchInstitutionAcctNum as 'AccountNumber', i.iInstitutionId as 'InstitutionId', r.vchIsbn10 as 'Isbn10', r.vchIsbn13 as 'Isbn13', ")
                .Append(
                    "r.vchResourceTitle as 'Title', r.tiFreeResource as 'FreeResource', p.vchPublisherName as 'Publisher', ci.decDiscountPrice as 'DiscountPrice', ")
                .Append(
                    "sum(ci.iNumberOfLicenses) as 'Licenses', sum(ci.iNumberOfLicenses) * ci.decDiscountPrice as 'Total', ci.decListPrice as 'ListPrice', ")
                .Append("c.iCartId as 'CartId', c.vchOrderNumber as 'OrderNumber', r.iResourceId as 'ResourceId' ")
                .Append(", c.dtPurchaseDate as 'PurchaseDate',  i.vchInstitutionName as 'InstitutionName' ")
                .Append("from tCartItem ci ")
                .Append("join tCart c on ci.iCartId = c.iCartId ")
                .Append("join tResource r on ci.iResourceId = r.iResourceId ")
                .Append("join tPublisher p on r.iPublisherId = p.iPublisherId ")
                .Append("join tInstitution i on c.iInstitutionId = i.iInstitutionId ")
                .Append("join tSpecial s on ci.vchSpecialText like s.vchName + '%' ")
                .Append("join tSpecialDiscount sd on s.iSpecialId = sd.iSpecialId ")
                .Append(
                    "join tSpecialResource sr on sr.iResourceId = r.iResourceId and sd.iSpecialDiscountId = sr.iSpecialDiscountId ")
                .Append("where c.tiProcessed = 1 and ci.iNumberOfLicenses > 0 and vchSpecialText is not null ")
                .Append(
                    "and s.iSpecialId = :SpecialId and s.tiRecordStatus = 1 and sd.tiRecordStatus = 1 and ci.tiRecordStatus = 1 and c.tiRecordStatus = 1 ")
                .Append(
                    "group by sd.iDiscountPercentage, i.vchInstitutionAcctNum, i.iInstitutionId, r.vchIsbn10, r.vchIsbn13 ")
                .Append(
                    ", r.vchResourceTitle, r.tiFreeResource, p.vchPublisherName, ci.decDiscountPrice, ci.decListPrice, c.iCartId, r.iResourceId, c.vchOrderNumber ")
                .Append(", c.dtPurchaseDate, i.vchInstitutionName ")
                .Append("order by i.iInstitutionId, c.vchOrderNumber, r.vchResourceTitle")
                .ToString();
            IList<DiscountResource> discountResources;
            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);
                query.SetParameter("SpecialId", specialId);
                discountResources = query.SetResultTransformer(Transformers.AliasToBean(typeof(DiscountResource)))
                    .List<DiscountResource>();
            }

            return discountResources.Any() ? discountResources.ToList() : null;
        }

        public List<PdaPromotion> GetPdaPromotions()
        {
            var sql = new StringBuilder()
                .Append("select pp.iPdaPromotionId ")
                .Append("from tCartItem ci ")
                .Append("join tCart c on ci.iCartId = c.iCartId ")
                .Append("join tResource r on ci.iResourceId = r.iResourceId ")
                .Append("join tPublisher p on r.iPublisherId = p.iPublisherId ")
                .Append("join tInstitution i on c.iInstitutionId = i.iInstitutionId ")
                .Append("join tPdaPromotion pp on   ci.vchSpecialText like '%' + pp.vchPdaPromotionName + '%' ")
                .Append("where ci.tiLicenseOriginalSourceId = 2 ")
                .Append("and c.tiProcessed = 1 and ci.iNumberOfLicenses > 0 and ci.vchSpecialText is not null ")
                .Append("and pp.tiRecordStatus = 1 and ci.tiRecordStatus = 1 and c.tiRecordStatus = 1 ")
                .Append("group by pp.iPdaPromotionId ")
                .ToString();
            var pdaPromotionIds = new List<int>();
            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);
                var results = query.List();

                foreach (var result in results)
                {
                    pdaPromotionIds.Add(ConvertObjectToInt(result));
                }
            }

            return pdaPromotionIds.Any() ? _pdaPromotions.Where(x => pdaPromotionIds.Contains(x.Id)).ToList() : null;
        }

        public PdaPromotion GetPdaPromotion(int id)
        {
            return _pdaPromotions.FirstOrDefault(x => x.Id == id);
        }

        public List<DiscountResource> GetPdaPromotionsReport(int pdaPromotionId)
        {
            var sql = new StringBuilder()
                .Append("select pp.iDiscountPercentage as 'DiscountPercentage', ")
                .Append(
                    "i.vchInstitutionAcctNum as 'AccountNumber', i.iInstitutionId as 'InstitutionId', r.vchIsbn10 as 'Isbn10', r.vchIsbn13 as 'Isbn13', ")
                .Append(
                    "r.vchResourceTitle as 'Title', p.vchPublisherName as 'Publisher', ci.decDiscountPrice as 'DiscountPrice', ")
                .Append(
                    "sum(ci.iNumberOfLicenses) as 'Licenses', sum(ci.iNumberOfLicenses) * ci.decDiscountPrice as 'Total', ci.decListPrice as 'ListPrice', ")
                .Append(" c.iCartId as 'CartId', c.vchOrderNumber as 'OrderNumber', r.iResourceId as 'ResourceId' ")
                .Append(", c.dtPurchaseDate as 'PurchaseDate',  i.vchInstitutionName as 'InstitutionName' ")
                .Append("from tCartItem ci ")
                .Append("join tCart c on ci.iCartId = c.iCartId ")
                .Append("join tResource r on ci.iResourceId = r.iResourceId ")
                .Append("join tPublisher p on r.iPublisherId = p.iPublisherId ")
                .Append("join tInstitution i on c.iInstitutionId = i.iInstitutionId ")
                .Append("join tPdaPromotion pp on   ci.vchSpecialText like '%' + pp.vchPdaPromotionName + '%' ")
                .Append("where ci.tiLicenseOriginalSourceId = 2 ")
                .Append("and c.tiProcessed = 1 and ci.iNumberOfLicenses > 0 and ci.vchSpecialText is not null ")
                .Append("and pp.tiRecordStatus = 1 and ci.tiRecordStatus = 1 and c.tiRecordStatus = 1 ")
                .Append("and pp.iPdaPromotionId = :PdaPromotionId ")
                .Append(
                    "group by pp.iDiscountPercentage, i.vchInstitutionAcctNum, i.iInstitutionId, r.vchIsbn10, r.vchIsbn13, ")
                .Append("r.vchResourceTitle, p.vchPublisherName, ci.decDiscountPrice, ")
                .Append("ci.iNumberOfLicenses, ci.decListPrice, c.iCartId, r.iResourceId, c.vchOrderNumber ")
                .Append(", c.dtPurchaseDate, i.vchInstitutionName ")
                .Append("order by i.iInstitutionId, c.vchOrderNumber, r.vchResourceTitle")
                .ToString();
            IList<DiscountResource> discountResources;
            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);
                query.SetParameter("PdaPromotionId", pdaPromotionId);
                discountResources = query.SetResultTransformer(Transformers.AliasToBean(typeof(DiscountResource)))
                    .List<DiscountResource>();
            }

            return discountResources.Any() ? discountResources.ToList() : null;
        }

        public List<CollectionManagement.Promotion> GetPromotions()
        {
            var sql = new StringBuilder()
                .Append("select pro.iPromotionId ")
                .Append("from tCartItem ci ")
                .Append("join tCart c on ci.iCartId = c.iCartId ")
                .Append("join tResource r on ci.iResourceId = r.iResourceId ")
                .Append("join tPublisher p on r.iPublisherId = p.iPublisherId ")
                .Append("join tInstitution i on c.iInstitutionId = i.iInstitutionId ")
                .Append("join tPromotion pro on c.vchPromotionCode = pro.vchPromotionCode ")
                .Append("where c.tiProcessed = 1 and ci.iNumberOfLicenses > 0 ")
                .Append("and pro.tiRecordStatus = 1 and ci.tiRecordStatus = 1 and c.tiRecordStatus = 1 ")
                .Append("group by pro.iPromotionId ")
                .ToString();
            var promotionIds = new List<int>();
            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);
                var results = query.List();

                foreach (var result in results)
                {
                    promotionIds.Add(ConvertObjectToInt(result));
                }
            }

            return promotionIds.Any() ? _promotions.Where(x => promotionIds.Contains(x.Id)).ToList() : null;
        }

        public CollectionManagement.Promotion GetPromotion(int id)
        {
            return _promotions.FirstOrDefault(x => x.Id == id);
        }

        public List<DiscountResource> GetPromotionsReport(int promotionId)
        {
            var sql = new StringBuilder()
                .Append("select pro.iDiscountPercentage as 'DiscountPercentage', ")
                .Append(
                    "i.vchInstitutionAcctNum as 'AccountNumber', i.iInstitutionId as 'InstitutionId', r.vchIsbn10 as 'Isbn10', r.vchIsbn13 as 'Isbn13', ")
                .Append(
                    "r.vchResourceTitle as 'Title', p.vchPublisherName as 'Publisher', ci.decDiscountPrice as 'DiscountPrice', ")
                .Append(
                    "sum(ci.iNumberOfLicenses) as 'Licenses', sum(ci.iNumberOfLicenses) * ci.decDiscountPrice as 'Total', ci.decListPrice as 'ListPrice', ")
                .Append(" c.iCartId as 'CartId', c.vchOrderNumber as 'OrderNumber', r.iResourceId as 'ResourceId' ")
                .Append(", c.dtPurchaseDate as 'PurchaseDate',  i.vchInstitutionName as 'InstitutionName' ")
                .Append("from tCartItem ci ")
                .Append("join tCart c on ci.iCartId = c.iCartId ")
                .Append("join tResource r on ci.iResourceId = r.iResourceId ")
                .Append("join tPublisher p on r.iPublisherId = p.iPublisherId ")
                .Append("join tInstitution i on c.iInstitutionId = i.iInstitutionId ")
                .Append("join tPromotion pro on c.vchPromotionCode = pro.vchPromotionCode ")
                .Append("where c.tiProcessed = 1 and ci.iNumberOfLicenses > 0 ")
                .Append("and pro.tiRecordStatus = 1 and ci.tiRecordStatus = 1 and c.tiRecordStatus = 1 ")
                .Append("and pro.iPromotionId = :PromotionId ")
                .Append(
                    "group by pro.iDiscountPercentage, i.vchInstitutionAcctNum, i.iInstitutionId, r.vchIsbn10, r.vchIsbn13, ")
                .Append("r.vchResourceTitle, p.vchPublisherName, ci.decDiscountPrice, ")
                .Append("ci.iNumberOfLicenses, ci.decListPrice, c.iCartId, r.iResourceId, c.vchOrderNumber ")
                .Append(", c.dtPurchaseDate, i.vchInstitutionName ")
                .Append("order by i.iInstitutionId, c.vchOrderNumber, r.vchResourceTitle")
                .ToString();
            IList<DiscountResource> discountResources;
            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql);
                query.SetParameter("PromotionId", promotionId);
                discountResources = query.SetResultTransformer(Transformers.AliasToBean(typeof(DiscountResource)))
                    .List<DiscountResource>();
            }

            return discountResources.Any() ? discountResources.ToList() : null;
        }


        private int ConvertObjectToInt(object value)
        {
            if (value == null)
            {
                return 0;
            }

            return (int)value;
        }
    }
}