#region

using System;

#endregion

namespace R2Library.Data.ADO.R2Utility
{
    public class ExtractedResourceError
    {
        public ExtractedResourceError(int resourceId, string isbn, string errorMessage)
        {
            ResourceId = resourceId;
            Isbn = isbn;
            ErrorMessage = errorMessage;
            DateCreated = DateTime.Now;
        }

        public int Id { get; set; }
        public int ResourceId { get; private set; }
        public string Isbn { get; private set; }
        public DateTime DateCreated { get; private set; }
        public string ErrorMessage { get; private set; }
    }
}