#region

using R2V2.Core.Reports;

#endregion

namespace R2V2.Core.AutomatedCart
{
    public class AutomatedCartReportQuery : BaseReportQuery
    {
        public string[] TerritoryCodes { get; set; }
        public int[] InstitutionTypeIds { get; set; }
        public bool IncludeNewEdition { get; set; }
        public bool IncludeTriggeredPda { get; set; }
        public bool IncludeTurnaway { get; set; }
        public bool IncludeReviewed { get; set; }
        public bool IncludeRequested { get; set; }
        public string AccountNumberBatch { get; set; }

        public bool IsDefaultQuery { get; set; }

        public string[] GetAccountNumberArray()
        {
            if (string.IsNullOrWhiteSpace(AccountNumberBatch))
            {
                return null;
            }

            var cleanNumberBatch = AccountNumberBatch.Replace(" ", "");
            var accountNumberArray = cleanNumberBatch.Split(',');
            return accountNumberArray;
        }
    }
}