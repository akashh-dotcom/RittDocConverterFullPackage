#region

using System;
using System.Web;

#endregion

namespace R2V2.Web.Exceptions
{
    [Serializable]
    public class HttpForbiddenException : HttpException, IExpectedException
    {
        private readonly int _errorCode;

        public HttpForbiddenException(string message, Exception innerException, int errorCode)
            : base(403, message, innerException)
        {
            _errorCode = errorCode;
        }

        public HttpForbiddenException(string message)
            : this(message, null, 0)
        {
        }

        public HttpForbiddenException()
            : this("The request was a legal request, but the server is refusing to respond to it")
        {
        }

        public override int ErrorCode => _errorCode;

        #region IExceptedException Members

        public int HttpStatusCode => GetHttpCode();

        public int HttpSubStatusCode => ErrorCode;

        public string UserFriendlyMessage => Message;

        #endregion
    }
}