#region

using System;

#endregion

namespace R2V2.Core.Resource.Author
{
    [Serializable]
    public class CachedAuthor : IAuthor
    {
        public CachedAuthor(IAuthor author)
        {
            Id = author.Id;
            ResourceCount = author.ResourceCount;
            ResourceId = author.ResourceId;
            Order = author.Order;
            FirstName = author.FirstName;
            LastName = author.LastName;
            MiddleName = author.MiddleName;
            Lineage = author.Lineage;
            Degrees = author.Degrees;
        }

        public int ResourceId { get; set; }
        public short Order { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Lineage { get; set; }
        public string Degrees { get; set; }

        public int ResourceCount { get; set; }
        public int Id { get; set; }

        public string GetFullName(bool lastNameFirst)
        {
            var fullname = !lastNameFirst
                ? $"{FirstName} {MiddleName} {LastName} {Lineage} {Degrees}"
                : $"{LastName} {FirstName} {MiddleName} {Lineage} {Degrees}";
            return fullname.Trim().Replace("  ", " ").Replace(" ,", ",");
        }
    }
}