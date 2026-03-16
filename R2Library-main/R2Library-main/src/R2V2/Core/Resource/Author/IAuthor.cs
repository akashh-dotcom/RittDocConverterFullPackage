namespace R2V2.Core.Resource.Author
{
    public interface IAuthor
    {
        int ResourceId { get; set; }
        short Order { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string MiddleName { get; set; }
        string Lineage { get; set; }
        string Degrees { get; set; }
        int ResourceCount { get; set; }
        int Id { get; set; }

        string GetFullName(bool lastNameFirst);
    }
}