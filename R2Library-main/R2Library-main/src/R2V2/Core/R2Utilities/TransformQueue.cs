#region

using System;

#endregion

namespace R2V2.Core.R2Utilities
{
    public class TransformQueue
    {
        public virtual int Id { get; set; }
        public virtual int ResourceId { get; set; }
        public virtual string Isbn { get; set; }

        public virtual string Status { get; set; }

        public virtual DateTime DateAdded { get; set; }
        public virtual DateTime DateStarted { get; set; }
        public virtual DateTime DateFinished { get; set; }

        public virtual string StatusMessage { get; set; }
    }
}