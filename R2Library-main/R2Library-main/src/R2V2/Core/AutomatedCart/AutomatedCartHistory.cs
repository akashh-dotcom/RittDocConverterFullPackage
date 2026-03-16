#region

using System;

#endregion

namespace R2V2.Core.AutomatedCart
{
    public class AutomatedCartHistory
    {
        public int AutomatedCartId { get; set; }
        public string CartName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int InstitutionCount { get; set; }
        public int ProcessedCount { get; set; }
        public bool WasProcessed { get; set; }
    }
}