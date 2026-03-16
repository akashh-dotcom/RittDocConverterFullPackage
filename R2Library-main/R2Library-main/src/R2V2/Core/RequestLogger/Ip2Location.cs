namespace R2V2.Core.RequestLogger
{
    public class Ip2Location : IDebugInfo
    {
        public virtual int Id { get; set; }
        public virtual long IpFrom { get; set; }
        public virtual long IpTo { get; set; }
        public virtual string CountryCode { get; set; }
        public virtual string CountryName { get; set; }

        public virtual string ToDebugString()
        {
            return
                $"Ip2Location = [Id = {Id}, CountryCode: {CountryCode}, CountryName: {CountryName}, IpTo: {IpTo}, IpFrom: {IpFrom}]";
        }
    }
}