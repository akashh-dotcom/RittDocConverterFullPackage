#region

using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource.Author
{
    public class Author : Entity, IAuthor
    {
        // -- Table: tAuthor
        // -- Fields: a.iAuthorId, a.iResourceId, a.vchFirstName, a.vchLastName, a.vchMiddleName, a.vchLineage, a.vchDegree, a.tiAuthorOrder

        public virtual int ResourceId { get; set; }
        public virtual short Order { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string MiddleName { get; set; }
        public virtual string Lineage { get; set; }
        public virtual string Degrees { get; set; }

        public virtual int ResourceCount { get; set; }

        public virtual string GetFullName(bool lastNameFirst)
        {
            var fullname = !lastNameFirst
                ? $"{FirstName} {MiddleName} {LastName} {Lineage} {Degrees}"
                : $"{LastName} {FirstName} {MiddleName} {Lineage} {Degrees}";
            return fullname.Trim().Replace("  ", " ").Replace(" ,", ",");
        }
    }
}