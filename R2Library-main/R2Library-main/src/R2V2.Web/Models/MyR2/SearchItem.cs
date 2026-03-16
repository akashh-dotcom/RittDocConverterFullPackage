#region

using System;
using System.ComponentModel.DataAnnotations;

#endregion

namespace R2V2.Web.Models.MyR2
{
    [Serializable]
    public class SearchItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Href { get; set; }
        public int ResultsCount { get; set; }
        public int TotalResultsCount { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ConvertEmptyStringToNull = true, NullDisplayText = "",
            ApplyFormatInEditMode = true)]
        public DateTime SearchDate { get; set; }
    }
}