#region

using System;
using System.Collections.Generic;

#endregion

namespace R2V2.Core.MyR2
{
    [Serializable]
    public class UserSavedFolder : UserContentFolder
    {
        private readonly IList<UserSavedSearch> _savedSearches = new List<UserSavedSearch>();

        public virtual IEnumerable<UserSavedSearch> SavedSearches => _savedSearches;

        public virtual void Add(UserSavedSearch savedSearch)
        {
            _savedSearches.Add(savedSearch);
            savedSearch.Folder = this;
        }
    }
}