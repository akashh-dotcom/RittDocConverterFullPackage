#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace R2V2.Core.Reports.Counter
{
    public class CounterResource
    {
        [Display(Name = "Title:")] public string Title { get; set; }

        [Display(Name = "Publisher:")] public string Publisher { get; set; }

        [Display(Name = "Publisher ID:")] public string PublisherId { get; set; }

        [Display(Name = "Proprietary ID:")] public string ProprietaryId { get; set; }

        [Display(Name = "Isbn 10:")] public string Isbn10 { get; set; }

        [Display(Name = "Isbn 13:")] public string Isbn13 { get; set; }

        [Display(Name = "Year of Publication:")]
        public int YearOfPublication { get; set; }
    }
}