#region

using System;
using System.Collections.Generic;

#endregion

namespace R2V2.Core.MyR2
{
    [Serializable]
    public class UserSavedSearchResultFolder : UserContentFolder
    {
        private readonly IList<UserSavedSearchResult> _savedSearchResults = new List<UserSavedSearchResult>();

        public virtual IEnumerable<UserSavedSearchResult> SavedSearchResults => _savedSearchResults;

        public virtual void Add(UserSavedSearchResult savedSearchResult)
        {
            _savedSearchResults.Add(savedSearchResult);
            savedSearchResult.Folder = this;
        }
    }
}