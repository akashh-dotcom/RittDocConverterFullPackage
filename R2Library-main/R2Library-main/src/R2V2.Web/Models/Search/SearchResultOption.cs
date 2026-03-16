#region

using System;

#endregion

namespace R2V2.Web.Models.Search
{
    public class SearchResultOption : IComparable
    {
        public int Id { get; set; }
        public string Group { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int Count { get; set; }
        public bool Selected { get; set; }

        public string CountText => Count < 1 ? "0" : $"{Count:#,##0}";


        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that indicates whether
        ///     the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared. The return value has these meanings: Value
        ///     Meaning Less than zero This instance is less than <paramref name="obj" />. Zero This instance is equal to
        ///     <paramref name="obj" />. Greater than zero This instance is greater than <paramref name="obj" />.
        /// </returns>
        /// <param name="obj">An object to compare with this instance. </param>
        /// <exception cref="T:System.ArgumentException"><paramref name="obj" /> is not the same type as this instance. </exception>
        /// <filterpriority>2</filterpriority>
        public int CompareTo(object obj)
        {
            // this is not a great solution, but it will work for now - Scott 5/11/2012
            // will need to refactor if this concept still exists after Phil refactors the search UI
            var option = (SearchResultOption)obj;
            if (Group == "year")
            {
                return string.CompareOrdinal(option.Name, Name);
            }

            if (Group == "specialty")
            {
                return option.Count - Count;
            }

            return string.CompareOrdinal(Name, option.Name);
        }

        public string ToDebug()
        {
            return $@"
		Id: {Id},
		Group: {Group},
		Name: {Name},
		Code: {Code},
		Count: {Count},
		Selected: {Selected}
";
        }
    }
}