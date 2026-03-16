#region

using System;
using System.Text;

#endregion

namespace R2V2.Core.RequestLogger
{
    [Serializable]
    public class MediaView
    {
        public int ResourceId { get; set; }
        public string ChapterSectionId { get; set; }
        public string MediaFileName { get; set; }

        public string ToDebugString()
        {
            var sb = new StringBuilder("MediaView = [");
            sb.AppendFormat("ResourceId: {0}", ResourceId);
            sb.AppendFormat(", ChapterSectionId: {0}", ChapterSectionId);
            sb.AppendFormat(", MediaFileName: {0}", MediaFileName);
            sb.Append("]");
            return sb.ToString();
        }
    }
}