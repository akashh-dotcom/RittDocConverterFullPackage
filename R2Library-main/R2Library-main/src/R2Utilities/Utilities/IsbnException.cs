#region

using System;

#endregion

namespace R2Utilities.Utilities
{
    public class IsbnException : Exception
    {
        public IsbnException(string message, Exception innerException, string errorCode)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public IsbnException(string message, string errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public IsbnException(string message)
            : base(message)
        {
            ErrorCode = null;
        }

        public string ErrorCode { get; set; }
    }
}