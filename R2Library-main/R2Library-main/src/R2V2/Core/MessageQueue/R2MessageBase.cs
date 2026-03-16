#region

using System;

#endregion

namespace R2V2.Core.MessageQueue
{
    public interface IR2MessageBase
    {
        Guid MessageKey { get; set; }
        DateTime MessageTimestamp { get; set; }
    }

    public class R2MessageBase : IR2MessageBase
    {
        public Guid MessageKey { get; set; } = Guid.NewGuid();
        public DateTime MessageTimestamp { get; set; } = DateTime.Now;

        public string ToDebugString()
        {
            return $"R2MessageBase = [MessageKey: {MessageKey}, MessageTimestamp: {MessageTimestamp}]";
        }
    }
}