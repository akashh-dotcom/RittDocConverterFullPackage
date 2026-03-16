#region

using System.Collections.Generic;
using R2V2.Core.Resource;

#endregion

namespace R2Utilities.DataAccess
{
    public class OnixEisbn
    {
        public string Isbn10 { get; set; }
        public string Isbn13 { get; set; }
        public string EIsbn10 { get; set; }
        public string EIsbn13 { get; set; }
        public string EPubType { get; set; }
        public string EPubTypeDescription { get; set; }
        public string EPubFormat { get; set; }
    }

    public class OnixEisbnDuplidate
    {
        public OnixEisbnDuplidate(OnixEisbn onixEisbn, List<Resource> duplicateResources)
        {
            DuplicateResoruces = duplicateResources;
            OnixEisbn = onixEisbn;
        }

        public List<Resource> DuplicateResoruces { get; set; }
        public OnixEisbn OnixEisbn { get; set; }
    }
}