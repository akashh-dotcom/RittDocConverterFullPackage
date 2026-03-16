#region

using System.Collections.Generic;
using System.IO;
using System.Linq;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public class DocIds
    {
        public string Isbn { get; set; }
        public int MinimumDocId { get; set; }
        public int MaximumDocId { get; set; }
        public List<DocIdFilename> Filenames { get; set; } = new List<DocIdFilename>();

        public IEnumerable<int> GetRange()
        {
            return Enumerable.Range(MinimumDocId, MaximumDocId - MinimumDocId + 1);
        }

        public DocIds GetInvalidDocsInIndex(string rootHtmlPath)
        {
            var docIds = new DocIds
            {
                Isbn = Isbn
            };

            foreach (var filename in Filenames)
            {
                if (filename.IsInvalidPath || !DoesHtmlFileExist(Isbn, filename.Name, rootHtmlPath))
                {
                    docIds.Filenames.Add(filename);
                }
            }

            if (docIds.Filenames.Any())
            {
                MinimumDocId = docIds.Filenames.First().Id;
                MaximumDocId = docIds.Filenames.Last().Id;
            }

            return docIds;
        }

        /// <summary>
        /// </summary>
        private bool DoesHtmlFileExist(string isbn, string filename, string rootHtmlPath)
        {
            var htmlFilePath = Path.Combine(rootHtmlPath, "html", isbn, filename);
            return File.Exists(htmlFilePath);
        }
    }

    public class DocIdFilename
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsInvalidPath { get; set; }
    }
}