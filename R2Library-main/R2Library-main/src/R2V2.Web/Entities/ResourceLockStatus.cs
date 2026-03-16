#region

using System;
using System.Text;
using R2V2.Core;
using R2V2.Core.Institution;
using R2V2.Core.Resource;
using R2V2.Web.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Entities
{
    public class ResourceLockStatus : IDebugInfo
    {
        public ResourceLockStatus(LockType lockType, int institutionId, int? userId, IResource resource,
            IWebSettings webSettings)
        {
            LockType = lockType;
            InstitutionId = institutionId;
            UserId = userId;
            Resource = resource;

            MinTimestamp = DateTime.Now.AddHours(webSettings.ResourcePrintCheckPeriodInHours * -1);

            LimitPercentage = webSettings.ResourcePrintLimitPercentage;
            LimitCountMax = webSettings.ResourcePrintLimitMax;
            LimitCountMin = webSettings.ResourcePrintLimitMin;
            WarningThresholdPercentage = webSettings.ResourcePrintWarningPercentage;

            ResourceSectionCount = resource.DocumentIdMax - resource.DocumentIdMin;
            LimitPercentageCount = ResourceSectionCount * (LimitPercentage * 0.01);
            LimitCount = LimitPercentageCount > LimitCountMax
                ? LimitCountMax
                : LimitPercentageCount < LimitCountMin
                    ? LimitCountMin
                    : (int)Math.Round(LimitPercentageCount, MidpointRounding.AwayFromZero);
            WarningThresholdCount = (int)Math.Round(LimitCount * WarningThresholdPercentage * 0.01,
                MidpointRounding.AwayFromZero);
        }

        public LockType LockType { get; }
        public int InstitutionId { get; }
        public int? UserId { get; }
        public IResource Resource { get; }
        public int ResourceSectionCount { get; }

        public bool LimitReached { get; private set; }
        public int LimitPercentage { get; }
        public double LimitPercentageCount { get; }
        public int LimitCountMax { get; }
        public int LimitCountMin { get; }
        public int LimitCount { get; }

        public bool WarningThresholdReached { get; private set; }
        public int WarningThresholdPercentage { get; }
        public int WarningThresholdCount { get; }

        public DateTime MinTimestamp { get; }
        public int RequestCount { get; private set; }

        public InstitutionResourceLock ResourceLock { get; private set; }

        public string ToDebugString()
        {
            var sb = new StringBuilder("ResourceLockStatus = [");
            sb.AppendFormat("Resource: {0}", Resource.ToDebugInfo());

            sb.AppendFormat(", LockType: {0}", LockType);
            sb.AppendFormat(", InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", UserId: {0}", UserId);
            sb.AppendFormat(", LimitReached: {0}", LimitReached);
            sb.AppendFormat(", WarningThresholdReached: {0}", WarningThresholdReached);
            sb.AppendFormat(", RequestCount: {0}", RequestCount);
            sb.AppendFormat(", LimitCount: {0}", LimitCount);
            sb.AppendFormat(", ResourceSectionCount: {0}", ResourceSectionCount);
            sb.AppendFormat(", LimitPercentage: {0}", LimitPercentage);
            sb.AppendFormat(", LimitPercentageCount: {0}", LimitPercentageCount);
            sb.AppendFormat(", LimitCountMax: {0}", LimitCountMax);
            sb.AppendFormat(", LimitCountMin: {0}", LimitCountMin);
            sb.AppendFormat(", WarningThresholdPercentage: {0}", WarningThresholdPercentage);
            sb.AppendFormat(", WarningThresholdCount: {0}", WarningThresholdCount);
            sb.AppendFormat(", MinTimestamp: {0}", MinTimestamp);

            sb.AppendFormat(", ResourceLock: {0}", ResourceLock == null ? "[null]" : ResourceLock.ToDebugString());
            sb.Append("]");
            return sb.ToString();
        }

        public void CalculateStatus(int requestCount)
        {
            RequestCount = requestCount;
            LimitReached = RequestCount >= LimitCount;
            WarningThresholdReached = RequestCount >= WarningThresholdCount;
        }

        public void SetStatus(InstitutionResourceLock resourceLock)
        {
            ResourceLock = resourceLock;
            RequestCount = LimitCount;
            LimitReached = true;
            WarningThresholdReached = true;
        }
    }
}