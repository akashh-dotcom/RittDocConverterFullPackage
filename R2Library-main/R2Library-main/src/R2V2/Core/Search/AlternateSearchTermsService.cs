#region

using System.Collections.Generic;
using System.Text;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Search
{
    public class AlternateSearchTermsService
    {
        private readonly ILog<AlternateSearchTermsService> _log;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public AlternateSearchTermsService(ILog<AlternateSearchTermsService> log
            , IUnitOfWorkProvider unitOfWorkProvider
        )
        {
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        public IEnumerable<string> GetAlternateTerms(string searchTerm)
        {
            var alternateTerms = new List<string>();

            // abscess, cancer, Acetaminophen, Xylosum
            var sql = new StringBuilder()
                .Append("select distinct dn1.vchDiseaseName ")
                .Append("from   tdiseasename dn1 ")
                .Append("  join dbo.tdiseasename dn2 on dn2.iParentDiseaseNameId = dn1.iParentDiseaseNameId ")
                .Append("where  dn2.vchDiseaseName = :term and dn1.vchDiseaseName <> :term ")
                .Append("union ")
                .Append("select distinct dn2.vchDiseaseName ")
                .Append("from   tdiseasename dn1 ")
                .Append(" join  dbo.tdiseasesynonym ds1 on ds1.iDiseaseNameId = dn1.iDiseaseNameId ")
                .Append(" join  dbo.tdiseasename dn2 on dn2.iParentDiseaseNameId = dn1.iDiseaseNameId ")
                .Append("where  ds1.vchDiseaseSynonym = :term ")
                .Append("union ")
                .Append("select distinct ds.vchDrugSynonymName ")
                .Append("from   tDrugSynonym ds ")
                .Append("  join dbo.tDrugsList dl on dl.iDrugListId = ds.iDrugListId ")
                .Append("where  dl.vchDrugName = :term ")
                .Append("union ")
                .Append("select distinct dl.vchDrugName ")
                .Append("from   tDrugsList dl ")
                .Append(" join  dbo.tDrugSynonym ds on ds.iDrugListId = dl.iDrugListId ")
                .Append("where  ds.vchDrugSynonymName = :term ");

            _log.Debug(sql.ToString());

            using (var uow = _unitOfWorkProvider.Start())
            {
                var query = uow.Session.CreateSQLQuery(sql.ToString());

                query.SetParameter("term", searchTerm);

                var results = query.List<string>();

                alternateTerms.AddRange(results);
                return alternateTerms;
            }
        }
    }
}