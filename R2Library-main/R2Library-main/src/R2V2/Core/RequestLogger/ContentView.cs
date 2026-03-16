#region

using System;
using System.Text;

#endregion

namespace R2V2.Core.RequestLogger
{
    [Serializable]
    public class ContentView
    {
        public int ResourceId { get; set; }
        public string ChapterSectionId { get; set; }
        public int ContentTurnawayTypeId { get; set; }

        public int ContentActionTypeId { get; set; }

        public bool FoundFromSearch { get; set; }
        public string SearchTerm { get; set; }

        public int ResourceStatusId { get; set; }
        public int LicenseTypeId { get; set; }

        public string ToDebugString()
        {
            var sb = new StringBuilder("ContentView = [");
            sb.Append($"ResourceId: {ResourceId}");
            sb.Append($", ChapterSectionId: {ChapterSectionId}");
            sb.Append($", ContentTurnawayTypeId: {ContentTurnawayTypeId}");
            sb.Append($", ContentActionTypeId: {ContentActionTypeId}");
            sb.Append($", FoundFromSearch: {FoundFromSearch}");
            sb.Append($", SearchTerm: {SearchTerm}");
            sb.Append($", ResourceStatusId: {ResourceStatusId}");
            sb.Append($", LicenseTypeId: {LicenseTypeId}");
            sb.Append("]");
            return sb.ToString();
        }
    }
}