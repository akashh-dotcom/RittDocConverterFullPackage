#region

using System;

#endregion

namespace R2V2.Core.Authentication
{
    [Serializable]
    public class Address : IDebugInfo
    {
        public virtual string Address1 { get; set; }
        public virtual string Address2 { get; set; }
        public virtual string City { get; set; }
        public virtual string State { get; set; }
        public virtual string Zip { get; set; }

        public string ToDebugString()
        {
            return $"Address = [Address1: {Address1}, Address2: {Address2}, City: {City}, State: {State}, Zip: {Zip}]";
        }
    }
}