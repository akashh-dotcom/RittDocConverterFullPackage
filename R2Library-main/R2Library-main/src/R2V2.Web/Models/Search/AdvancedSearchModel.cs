#region

using System;
using System.Collections.Generic;
using R2V2.Core.Resource.PracticeArea;

#endregion

namespace R2V2.Web.Models.Search
{
    [Serializable]
    public class AdvancedSearchModel
    {
        public string Author { get; set; } // advanced search
        public string Title { get; set; } // advanced search
        public string Publisher { get; set; } // advanced search
        public string Editor { get; set; } // advanced search
        public string Isbn { get; set; } // advanced search
        public string PracticeArea { get; set; } // advanced search
        public int YearMin { get; set; } // advanced search
        public int YearMax { get; set; } // advanced search

        public bool DisplayTocAvailable { get; set; } // advanced search

        public int[] PublicationYears { get; set; } // advanced search
        public IEnumerable<PracticeArea> PracticeAreas { get; set; } // advanced search

        public bool IncludeArchiveTitles { get; set; }
    }
}