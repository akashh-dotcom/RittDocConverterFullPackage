namespace R2V2.XslTest
{
    internal static class Extensions
    {
        internal static string TrimEnd(this string value, string trimString)
        {
            return value.TrimEnd(trimString.ToCharArray());
        }
    }
}