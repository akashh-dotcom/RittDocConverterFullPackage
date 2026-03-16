#region

using System;
using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using R2V2.Core;
using R2V2.Extensions;
using R2V2.Web.Helpers;
using log4net;

#endregion

namespace R2V2.Web.Infrastructure
{
    public class DebugInformation
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     The intent of this class is to offer an easy way to log all of the request and session information
        ///     to the log in one call.
        /// </summary>
        /// <param name="exception"> </param>
        public static string GetDebugInformation(Exception exception, HttpRequestBase request, Guid errorId)
        {
            //SetDebugInformation(request);

            var msg = new StringBuilder();
            try
            {
                msg.AppendFormatedLine("///// {0}", exception.Message);
                msg.AppendLine(@"\\\\\");
            }
            catch (Exception ex)
            {
                // swallow exceptions in logging code & return exception information
                msg.AppendLine().AppendLine(">>>>>>>>>> GetDebugInformation() EXCEPTION >>>>>>>>>>")
                    .AppendLine(ex.Message)
                    .AppendLine(ex.StackTrace)
                    .AppendLine("<<<<<<<<<< GetDebugInformation() EXCEPTION <<<<<<<<<<");
            }

            return msg.ToString();
        }

        private static string GetPrimaryRequestData(HttpRequestBase request)
        {
            var msg = new StringBuilder();
            try
            {
                msg.AppendLine("REQUEST DATA:");
                //msg.AppendFormatedLine("URL: {0}", request.RawUrl);
                msg.AppendFormatedLine("URL: {0}", request.Url == null ? request.RawUrl : request.Url.AbsoluteUri);
                msg.AppendFormatedLine("Referrer: {0}", request.HttpReferrer());
                msg.AppendFormatedLine("User Agent: {0}", request.UserAgent ?? "null");
                msg.AppendFormatedLine("IP Address: {0}", request.GetHostIpAddress() ?? "null");
            }
            catch (Exception ex)
            {
                // swallow exceptions in logging code & return exception information
                msg.AppendLine().AppendLine(">>>>>>>>>> GetRequestData() EXCEPTION >>>>>>>>>>")
                    .AppendLine(ex.Message)
                    .AppendLine(ex.StackTrace)
                    .AppendLine("<<<<<<<<<< GetRequestData() EXCEPTION <<<<<<<<<<");
            }

            return msg.ToString();
        }

        private static string GetHttpContentData(HttpContextBase content)
        {
            var msg = new StringBuilder();
            try
            {
                msg.AppendLine("HTTP CONTEXT:");

                foreach (DictionaryEntry entry in content.Items)
                {
                    //msg.AppendFormatedLine("key: {0}, {1}", entry.Key.ToString(), (entry.Value != null) ? entry.Value.ToString() : "null");
                    var o = entry.Value;
                    var debugInfo = o as IDebugInfo;
                    if (debugInfo != null)
                    {
                        msg.AppendFormatedLine("key: {0} -> {1}", entry.Key.ToString(), debugInfo.ToDebugString());
                    }
                    else
                    {
                        msg.AppendFormatedLine("key: {0} -> {1}", entry.Key.ToString(),
                            entry.Value != null ? entry.Value.ToString() : "null");
                    }
                }
            }
            catch (Exception ex)
            {
                // swallow exceptions in logging code & return exception information
                msg.AppendLine().AppendLine(">>>>>>>>>> GetHttpContentData() EXCEPTION >>>>>>>>>>")
                    .AppendLine(ex.Message)
                    .AppendLine(ex.StackTrace)
                    .AppendLine("<<<<<<<<<< GetHttpContentData() EXCEPTION <<<<<<<<<<");
            }

            return msg.ToString();
        }

        private static string GetSecondaryRequestData(HttpRequestBase request)
        {
            var msg = new StringBuilder();
            msg.AppendLine("Additional Request Information:");
            try
            {
                msg.AppendFormatedLine("\tUserAgent: {0}".Args(request.UserAgent));
                msg.AppendFormatedLine("\tContentType: {0}".Args(request.ContentType));
                msg.AppendFormatedLine("\tContentEncoding: {0}".Args(request.ContentEncoding));
                msg.AppendFormatedLine("\tParmas: ");
                foreach (var key in request.Params.Keys)
                {
                    //msg.AppendFormatedLine("\t\t{0}:{1}", key, request.Params[key.ToString()]);
                    msg.AppendFormat("\t\t{0}:", key);
                    msg.AppendLine(request.Params[key.ToString()]);
                }
            }
            catch (Exception ex)
            {
                // swallow exceptions in logging code & return exception information
                msg.AppendLine().AppendLine(">>>>>>>>>> GetSecondaryRequestData() EXCEPTION >>>>>>>>>>")
                    .AppendLine(ex.Message)
                    .AppendLine(ex.StackTrace)
                    .AppendLine("<<<<<<<<<< GetSecondaryRequestData() EXCEPTION <<<<<<<<<<");
            }

            return msg.ToString();
        }


        private static string GetHttpSessionData(HttpSessionStateBase session)
        {
            var msg = new StringBuilder();
            try
            {
                msg.AppendLine("HTTP SESSION:");
                foreach (string key in session.Contents.Keys)
                {
                    var o = session.Contents[key];
                    if (o is IDebugInfo debugInfo)
                    {
                        msg.AppendFormatedLine("key: {0} -> {1}", key, debugInfo.ToDebugString());
                    }
                    else
                    {
                        msg.AppendFormatedLine("key: {0} -> {1}", key, o != null ? o.ToString() : "null");
                    }
                }
            }
            catch (Exception ex)
            {
                // swallow exceptions in logging code & return exception information
                msg.AppendLine().AppendLine(">>>>>>>>>> GetHttpSessionData() EXCEPTION >>>>>>>>>>")
                    .AppendLine(ex.Message)
                    .AppendLine(ex.StackTrace)
                    .AppendLine("<<<<<<<<<< GetHttpSessionData() EXCEPTION <<<<<<<<<<");
            }

            return msg.ToString();
        }

        public static string GetExceptionHash(string stackTrace)
        {
            try
            {
                var bytes = Encoding.Unicode.GetBytes(stackTrace);

                using (var hashAlgorithm = HashAlgorithm.Create("SHA1"))
                {
                    var inArray = hashAlgorithm.ComputeHash(bytes);
                    return Convert.ToBase64String(inArray);
                }
            }
            catch (Exception ex)
            {
                Log.WarnFormat(ex.Message, ex);
                return "ExceptionCreatingHash";
            }
        }
    }
}