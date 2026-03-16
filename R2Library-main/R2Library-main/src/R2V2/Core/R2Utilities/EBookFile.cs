#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

#endregion

namespace R2V2.Core.R2Utilities
{
    public class EBookReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int PublisherCount { get; set; }
        public int TitleCount { get; set; }

        public List<EBookPublisher> PublisherFiles { get; set; }
    }

    public class EBookPublisher
    {
        public string Publisher { get; set; }
        public int FileCount { get; set; }
        public List<EBookFile> Files { get; set; }
    }

    public class EBookFile
    {
        public string FileName { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public List<string> Paths { get; set; }
        public string Folder { get; set; }
        public DateTime CreateTime { get; set; }
        public string Link { get; set; }
        public List<string> Extensions { get; set; }

        public bool NameAsIsbn { get; set; }

        public EBookDetails Details { get; set; }

        public string GetExtensionString()
        {
            var t = Extensions.Distinct();
            return string.Join(", ", t);
        }

        public string GetPathString()
        {
            var t = Paths.Distinct();
            return string.Join(", ", t);
        }

        public string GoogleBooksUrl()
        {
            return NameAsIsbn ? $"https://books.google.com/books?vid={Name}" : "";
        }

        public string GoogleSearchUrl()
        {
            return NameAsIsbn ? $"https://www.google.com/search?q={Name}" : "";
        }

        public string AmazonSearchUrl()
        {
            return NameAsIsbn ? $"https://www.amazon.com/s?k={Name}" : "";
        }
        //public List<EBookFileLink> EBookLinks { get; set; }

        public string ToDebug()
        {
            return new StringBuilder()
                .Append($"-> Folder: {Folder} ")
                .Append($"-- Name: {FileName} ")
                .Append($"--  CreateTime: {CreateTime:yyyy-MM-dd hh:mm:ss} ")
                .Append($"--  Path: {Path} ")
                .ToString();
        }
    }

    public class EBookDetailsRoot
    {
        [JsonProperty("book")] public EBookDetails Book { get; set; }
    }


    public class EBookDetails
    {
        [JsonProperty("publisher")] public string Publisher { get; set; }
        [JsonProperty("synopsis")] public string Synopsis { get; set; }
        [JsonProperty("language")] public string Language { get; set; }
        [JsonProperty("image")] public string Image { get; set; }
        [JsonProperty("edition")] public string Edition { get; set; }
        [JsonProperty("pages")] public string Pages { get; set; }
        [JsonProperty("date_published")] public string DatePublished { get; set; }
        [JsonProperty("subjects")] public List<string> Subjects { get; set; }
        [JsonProperty("authors")] public List<string> Authors { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("isbn10")] public string Isbn10 { get; set; }
        [JsonProperty("isbn13")] public string Isbn13 { get; set; }

        [JsonProperty("msrp")] public decimal Msrp { get; set; }

        //public decimal? Msrp { get; set; }
        [JsonProperty("other_isbns")] public List<EBookDetailsOtherIsbn> OtherIsbns { get; set; }
    }

    public class EBookDetailsOtherIsbn
    {
        [JsonProperty("isbn")] public string Isbn { get; set; }
        [JsonProperty("binding")] public string Binding { get; set; }
    }
}