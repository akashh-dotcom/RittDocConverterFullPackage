#region

using System.Xml.Xsl;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.Core.Resource.Content
{
    //Used to handle XsltArgumentList.XsltMessageEncountered event
    internal class XsltMessageHandler
    {
        private readonly ILog<XsltMessageHandler> _log;

        public XsltMessageHandler(ILog<XsltMessageHandler> log)
        {
            _log = log;
        }


        internal void OnXsltMessageEncountered(object sender, XsltMessageEncounteredEventArgs e)
        {
            //Intercept this event, do not write message to output stream

            /*var json = foo(e.Message) //attempt to parse json

            switch (json.messageType)
            {
                case "video":
                    //Call method to handle video messages
                    break;
                default:*/
            //Log message
            _log.WarnFormat("Xslt Message Encountered: {0}", e.Message);
            /*break;
    }*/
        }
    }
}