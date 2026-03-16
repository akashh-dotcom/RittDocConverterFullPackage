namespace R2Utilities.DataAccess.WebActivity
{
    public class TopIpAddress : TopInstitution
    {
        public virtual int OctetA { get; set; }
        public virtual int OctetB { get; set; }
        public virtual int OctetC { get; set; }
        public virtual int OctetD { get; set; }

        public virtual string CountryCode { get; set; }

        public virtual long GetLongValue()
        {
            var ipNumberA = 16777216L * OctetA;
            var ipNumberB = 65536L * OctetB;
            var ipNumberC = 256L * OctetC;
            var ipNumber = ipNumberA + ipNumberB + ipNumberC + OctetD;
            return ipNumber;
        }
    }
}