#region

using System;
using System.Web;

#endregion

namespace R2V2.Web.Exceptions
{
    [Serializable]
    public class NotFoundHttpException : HttpException, IExpectedException
    {
        public NotFoundHttpException(string message, Exception innerException)
            : base(404, message, innerException)
        {
        }

        public NotFoundHttpException(string message) : this(message, null)
        {
        }

        public NotFoundHttpException() : this(
            "Sorry, you tried to access a page on this website that could not be found.")
        {
        }


        public int HttpStatusCode => GetHttpCode();

        public string UserFriendlyMessage => Message;

        public int HttpSubStatusCode => ErrorCode;
    }
}