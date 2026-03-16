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
    public class ResourcePrintLockStatus : IDebugInfo
    {
        public ResourcePrintLockStatus(int institutionId, IResource resource, IWebSettings webSettings)
        {
            InstitutionId = institutionId;
            Resource = resource;

            MinTimestamp = DateTime.Now.AddHours(webSettings.ResourcePrintCheckPeriodInHours * -1);

            PrintLimitPercentage = webSettings.ResourcePrintLimitPercentage;
            PrintLimitCountMax = webSettings.ResourcePrintLimitMax;
            PrintLimitCountMin = webSettings.ResourcePrintLimitMin;
            WarningThresholdPercentage = webSettings.ResourcePrintWarningPercentage;

            ResourceSectionCount = resource.DocumentIdMax - resource.DocumentIdMin;
            PrintLimitPercentageCount = ResourceSectionCount * (PrintLimitPercentage * 0.01);
            PrintLimitCount = PrintLimitPercentageCount > PrintLimitCountMax
                ? PrintLimitCountMax
                : PrintLimitPercentageCount < PrintLimitCountMin
                    ? PrintLimitCountMin
                    : (int)Math.Round(PrintLimitPercentageCount, MidpointRounding.AwayFromZero);
            WarningThresholdCount = (int)Math.Round(PrintLimitCount * WarningThresholdPercentage * 0.01,
                MidpointRounding.AwayFromZero);
        }

        public int InstitutionId { get; }
        public IResource Resource { get; }
        public int ResourceSectionCount { get; }

        public bool PrintLimitReached { get; private set; }
        public int PrintLimitPercentage { get; }
        public double PrintLimitPercentageCount { get; }
        public int PrintLimitCountMax { get; }
        public int PrintLimitCountMin { get; }
        public int PrintLimitCount { get; }

        public bool WarningThresholdReached { get; private set; }
        public int WarningThresholdPercentage { get; }
        public int WarningThresholdCount { get; }

        public DateTime MinTimestamp { get; }
        public int PrintRequestCount { get; private set; }

        public InstitutionResourceLock ResourceLock { get; private set; }


        public string ToDebugString()
        {
            var sb = new StringBuilder("ResourcePrintLockStatus = [");
            sb.AppendFormat("Resource: {0}", Resource.ToDebugInfo());

            sb.AppendLine().Append("\t");
            sb.AppendFormat(", InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", PrintLimitReached: {0}", PrintLimitReached);
            sb.AppendFormat(", WarningThresholdReached: {0}", WarningThresholdReached);
            sb.AppendFormat(", PrintRequestCount: {0}", PrintRequestCount);
            sb.AppendFormat(", PrintLimitCount: {0}", PrintLimitCount);
            sb.AppendFormat(", ResourceSectionCount: {0}", ResourceSectionCount);
            sb.AppendFormat(", PrintLimitPercentage: {0}", PrintLimitPercentage);
            sb.AppendFormat(", PrintLimitPercentageCount: {0}", PrintLimitPercentageCount);
            sb.AppendFormat(", PrintLimitCountMax: {0}", PrintLimitCountMax);
            sb.AppendFormat(", PrintLimitCountMin: {0}", PrintLimitCountMin);
            sb.AppendFormat(", WarningThresholdPercentage: {0}", WarningThresholdPercentage);
            sb.AppendFormat(", WarningThresholdCount: {0}", WarningThresholdCount);
            sb.AppendFormat(", MinTimestamp: {0}", MinTimestamp);

            sb.AppendLine().Append("\t");
            sb.AppendFormat(", ResourceLock: {0}", ResourceLock == null ? "[null]" : ResourceLock.ToDebugString());
            sb.Append("]");
            return sb.ToString();
        }

        public void CalculateStatus(int printRequestCount)
        {
            PrintRequestCount = printRequestCount;
            PrintLimitReached = PrintRequestCount >= PrintLimitCount;
            WarningThresholdReached = PrintRequestCount >= WarningThresholdCount;
        }

        public void SetStatus(InstitutionResourceLock resourceLock)
        {
            ResourceLock = resourceLock;
            PrintRequestCount = PrintLimitCount;
            PrintLimitReached = true;
            WarningThresholdReached = true;
        }
    }
}