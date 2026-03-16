#region

using System.Collections.Generic;

#endregion

namespace R2V2.Core.Resource.Author
{
    public interface IAuthorService
    {
        IEnumerable<Author> GetAuthors(int institutionId, Include include, string practiceArea,
            bool displayAllProducts);

        Author GetAuthor(string lastName);
    }
}