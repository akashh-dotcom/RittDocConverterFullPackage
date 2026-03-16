#region

using System;
using System.Text;
using dtSearch.Engine;

#endregion

namespace R2V2.DataAccess.DtSearch
{
    public class IndexStatus
    {
        public IndexStatus(IndexInfo indexInfo, string indexLocation)
        {
            CompressedDate = indexInfo.CompressedDate;
            CreatedDate = indexInfo.CompressedDate;
            DocCount = indexInfo.DocCount;
            Fragmentation = indexInfo.Fragmentation;
            IndexSize = indexInfo.IndexSize;
            LastDocId = indexInfo.LastDocId;
            ObsoleteCount = indexInfo.ObsoleteCount;
            PercentFull = indexInfo.PercentFull;
            StartingDocId = indexInfo.StartingDocId;
            StructureVersion = indexInfo.StructureVersion;
            UpdatedDate = indexInfo.UpdatedDate;
            WordCount = indexInfo.WordCount;
            Location = indexLocation;
        }

        public DateTime CompressedDate { get; }
        public DateTime CreatedDate { get; }
        public uint DocCount { get; }
        public uint Fragmentation { get; }
        public ulong IndexSize { get; }
        public uint LastDocId { get; }
        public uint ObsoleteCount { get; }
        public uint PercentFull { get; }
        public uint StartingDocId { get; }
        public uint StructureVersion { get; }
        public DateTime UpdatedDate { get; }
        public ulong WordCount { get; }
        public string Location { get; }

        public string ToDebugString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Index Status = [ Location: {0}", Location);
            sb.AppendFormat(", CompressedDate: {0}", CompressedDate);
            sb.AppendFormat(", CreatedDate: {0}", CreatedDate);
            sb.AppendFormat(", DocCount: {0}", DocCount);
            sb.AppendFormat(", Fragmentation: {0}", Fragmentation);
            sb.AppendFormat(", IndexSize: {0}", IndexSize);
            sb.AppendFormat(", Location: {0}", LastDocId);
            sb.AppendFormat(", ObsoleteCount: {0}", ObsoleteCount);
            sb.AppendFormat(", PercentFull: {0}", PercentFull);
            sb.AppendFormat(", StartingDocId: {0}", StartingDocId);
            sb.AppendFormat(", StructureVersion: {0}", StructureVersion);
            sb.AppendFormat(", UpdatedDate: {0}", UpdatedDate);
            sb.AppendFormat(", WordCount: {0}", WordCount);
            sb.AppendFormat("]");
            return sb.ToString();
        }
    }
}