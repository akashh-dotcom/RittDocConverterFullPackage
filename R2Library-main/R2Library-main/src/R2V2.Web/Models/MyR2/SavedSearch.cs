#region

using System;
using R2V2.Web.Models.Search;

#endregion

namespace R2V2.Web.Models.MyR2
{
    [Serializable]
    public class SavedSearch : SearchQuery
    {
        public string Name { get; set; }
        public int Total { get; set; }

        //Only Used for Session Saved Searches
        public int Id { get; set; }
        public DateTime SearchDate { get; set; }
    }
}