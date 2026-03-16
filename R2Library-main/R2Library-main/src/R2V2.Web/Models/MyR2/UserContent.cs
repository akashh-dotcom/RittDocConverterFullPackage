#region

using System;
using System.Collections.Generic;
using System.Linq;
using R2V2.Core.MyR2;

#endregion

namespace R2V2.Web.Models.MyR2
{
    [Serializable]
    public class UserContent : BaseModel
    {
        private IEnumerable<UserContentFolder> _userContentFolders;

        public UserContent(IEnumerable<Core.MyR2.UserContentFolder> userContentFolders
            , UserContentType userContentType
            , IList<SearchItem> searchHistories
            , IList<SearchItem> savedSearches
            , IList<SearchItem> savedSearchResults
        )
        {
            UserContentFolders = userContentFolders.ToUserContentFolders(userContentType);
            UserContentType = userContentType;

            SearchHistories = searchHistories;
            SearchHistoryCount = searchHistories?.Count ?? 0;

            SavedSearchCount = savedSearches?.Count ?? 0;
            ;
            SavedSearches = savedSearches;

            SavedSearchResultsCount = savedSearchResults?.Count ?? 0;
            ;
            SavedSearchResults = savedSearchResults;
        }

        /// <summary>
        ///     Used for gettting User Session Items
        /// </summary>
        /// <param name="savedSearchResults"> </param>
        public UserContent(IEnumerable<UserContentFolder> userContentFolders
            , UserContentType userContentType
            , IList<SearchItem> searchHistories
            , IList<SearchItem> savedSearches
            , IList<SearchItem> savedSearchResults
        )
        {
            //_savedSearchResults = savedSearchResults;
            UserContentFolders = userContentFolders;
            UserContentType = userContentType;

            SearchHistories = searchHistories;
            SearchHistoryCount = searchHistories?.Count ?? 0;
            ;

            SavedSearchCount = savedSearches?.Count ?? 0;
            ;
            SavedSearches = savedSearches;

            SavedSearchResultsCount = savedSearchResults?.Count ?? 0;
            SavedSearchResults = savedSearchResults;
        }

        public int SearchHistoryCount { get; set; }
        public IEnumerable<SearchItem> SearchHistories { get; set; }

        public int SavedSearchResultsCount { get; set; }
        public IEnumerable<SearchItem> SavedSearchResults { get; set; }

        public int SavedSearchCount { get; set; }
        public IEnumerable<SearchItem> SavedSearches { get; set; }

        public UserContentType UserContentType { get; set; }

        public string Title
        {
            get
            {
                switch (UserContentType)
                {
                    case UserContentType.Reference:
                        return "References";

                    case UserContentType.Image:
                        return "Images";

                    case UserContentType.CourseLink:
                        return "Course Links";

                    default:
                        return "Bookmarks";
                }
            }
        }

        public UserContentFolder DefaultFolder
        {
            get
            {
                return _userContentFolders == null ? null : _userContentFolders.FirstOrDefault(x => x.DefaultFolder);
            }
        }

        public IEnumerable<UserContentFolder> UserContentFolders
        {
            get => _userContentFolders; // .Where(x => !x.DefaultFolder); }
            set => _userContentFolders = value;
        }

        public IEnumerable<UserContentItem> UserContentItems
        {
            get { return UserContentFolders.SelectMany(userContentFolder => userContentFolder.UserContentItems); }
        }
    }
}