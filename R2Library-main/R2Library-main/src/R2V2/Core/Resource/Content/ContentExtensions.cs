namespace R2V2.Core.Resource.Content
{
    public static class ContentExtensions
    {
        public static string CleanAndTrim(this string s)
        {
            return s.Replace("\r\n", " ").Trim();
        }
    }
}