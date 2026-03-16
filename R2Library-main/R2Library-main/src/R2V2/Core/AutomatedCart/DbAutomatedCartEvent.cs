#region

using System;

#endregion

namespace R2V2.Core.AutomatedCart
{
    public class DbAutomatedCartEvent
    {
        public virtual Guid Id { get; set; }
        public virtual int InstitutionId { get; set; }
        public virtual int ResourceId { get; set; }
        public virtual int TerritoryId { get; set; }
        public virtual DateTime EventDate { get; set; }
        public virtual int NewEdition { get; set; }
        public virtual int TriggeredPda { get; set; }
        public virtual int Turnaway { get; set; }
        public virtual int Reviewed { get; set; }
        public virtual int Requested { get; set; }
    }
}