#region

using System;

#endregion

namespace R2V2.Infrastructure.MessageQueue
{
    public interface IR2V2Message
    {
        Guid MessageId { get; set; }
        string ToJsonString();
    }
}