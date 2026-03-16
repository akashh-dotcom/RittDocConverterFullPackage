#region

using System;

#endregion

namespace R2Library.Data.ADO.R2Utility
{
    public class TransformedResourceError
    {
        /// <param name="filename"> </param>
        public TransformedResourceError(int resourceId, string isbn, string filename, string errorMessage,
            DateTime dateCreated)
        {
            ResourceId = resourceId;
            Isbn = isbn;
            ErrorMessage = errorMessage;
            DateCreated = dateCreated;
            Filename = filename;
        }

        public int Id { get; set; }
        public int ResourceId { get; private set; }
        public string Isbn { get; private set; }
        public DateTime DateCreated { get; private set; }
        public string Filename { get; private set; }
        public string ErrorMessage { get; private set; }
    }
}