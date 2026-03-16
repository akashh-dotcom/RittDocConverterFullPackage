#region

using System;
using R2V2.DataAccess.DtSearch;

#endregion

namespace R2V2.Web.Models.Ping
{
    public class PingIndexStatus : PingData
    {
        public DateTime CompressedDate { get; private set; }
        public DateTime CreatedDate { get; private set; }
        public uint DocCount { get; private set; }
        public uint Fragmentation { get; private set; }
        public ulong IndexSize { get; private set; }
        public uint LastDocId { get; private set; }
        public uint ObsoleteCount { get; private set; }
        public uint PercentFull { get; private set; }
        public uint StartingDocId { get; private set; }
        public uint StructureVersion { get; private set; }
        public DateTime UpdatedDate { get; private set; }
        public ulong WordCount { get; private set; }

        public void SetIndexStatus(IndexStatus indexInfo)
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
        }
    }
}