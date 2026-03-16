#region

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using R2Library.Data.ADO.Core.SqlCommandParameters;

#endregion

namespace R2Utilities.DataAccess.Terms
{
    public class DiseaseResource : TermResource
    {
        #region Methods

        public override void Populate(SqlDataReader reader)
        {
            base.Populate(reader);

            try
            {
                Id = GetInt32Value(reader, "iDiseaseResourceId", -1);
                TermId = GetInt32Value(reader, "iDiseaseNameId", -1);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat(ex.Message, ex);
                throw;
            }
        }

        public override IEnumerable<ISqlCommandParameter> ToParameters(int x)
        {
            var result = base.ToParameters(x).ToList();

            result.Add(new Int32Parameter($"DiseaseResourceId_{x}", Id));
            result.Add(new Int32Parameter($"DiseaseNameId_{x}", TermId));

            return result.ToArray();
        }

        #endregion Methods

        #region Properties

        public override string SqlInsert =>
            new StringBuilder()
                .Append(
                    "insert into tdiseaseresource (iDiseaseNameId, vchResourceISBN, vchChapterId, vchSectionId, vchCreatorId, dtCreationDate) ")
                .Append(
                    "values(@DiseaseNameId_{0}, @ResourceISBN_{0}, @ChapterId_{0}, @SectionId_{0}, @CreatorId_{0}, getdate())  ")
                .ToString();

        public override string SqlInactivate =>
            new StringBuilder()
                .Append("update tdiseaseresource set tiRecordStatus = 0 where vchResourceISBN = @ResourceISBN")
                .ToString();

        #endregion Properties
    }
}