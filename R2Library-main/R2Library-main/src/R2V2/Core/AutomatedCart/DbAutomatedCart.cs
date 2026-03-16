#region

using System;
using System.Text;
using R2V2.Core.Reports;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.AutomatedCart
{
    public class DbAutomatedCart : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual ReportPeriod Period { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }
        public virtual bool NewEdition { get; set; }
        public virtual bool TriggeredPda { get; set; }
        public virtual bool Reviewed { get; set; }
        public virtual bool Requested { get; set; }
        public virtual bool Turnaway { get; set; }
        public virtual decimal Discount { get; set; }
        public virtual string AccountNumbers { get; set; }
        public virtual string CartName { get; set; }
        public virtual string EmailSubject { get; set; }
        public virtual string EmailTitle { get; set; }
        public virtual string EmailText { get; set; }
        public virtual string TerritoryIds { get; set; }
        public virtual string InstitutionTypeIds { get; set; }


        public virtual string ToDebugString()
        {
            return new StringBuilder()
                .Append("AutomatedCart=[")
                .Append($"Period: {Period},")
                .Append($"StartDate: {StartDate},")
                .Append($"EndDate: {EndDate},")
                .Append($"TerritoryIds: {TerritoryIds},")
                .Append($"InstitutionTypeIds: {InstitutionTypeIds},")
                .Append($"NewEdition: {NewEdition},")
                .Append($"TriggeredPda: {TriggeredPda},")
                .Append($"Reviewed: {Reviewed},")
                .Append($"Requested: {Requested},")
                .Append($"Turnaway: {Turnaway},")
                .Append($"Discount: {Discount},")
                .Append($"AccountNumbers: {AccountNumbers},")
                .Append($"EmailSubject: {EmailSubject},")
                .Append($"EmailTitle: {EmailTitle},")
                .Append($"EmailText: {EmailText},")
                .Append($"CartName: {CartName},")
                .Append($"RecordStatus: {RecordStatus}]")
                .ToString();
        }

        public virtual bool RecordStatus { get; set; }
    }
}