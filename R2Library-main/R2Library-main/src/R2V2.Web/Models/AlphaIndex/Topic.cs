#region

using System;

#endregion

namespace R2V2.Web.Models.AlphaIndex
{
    public class Topic : IComparable
    {
        public string Name { get; set; }
        public string Key => Name.Length < 2 ? Name : Name.ToUpper().Substring(0, 2).Trim();

        public int CompareTo(object obj)
        {
            var topic = (Topic)obj;
            return string.CompareOrdinal(Name.ToLower(), topic.Name.ToLower());
        }
    }
}