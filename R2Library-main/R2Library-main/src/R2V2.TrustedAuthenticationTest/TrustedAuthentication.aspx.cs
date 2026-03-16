using System;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using System.Web.UI;

namespace R2V2.TrustedAuthenticationTest
{
    public partial class TrustedAuthentication : Page
    {
        //24 character Trusted Authenication Serivce Key.
        private const string TrustedAuthenticationKey = "BChwmnK0WXYjq9sbvVdxCkGS";
        //Institution account number
        private const string AccountNumber = "005034";

        protected void Page_Load(object sender, EventArgs e)
        {
            string webRequestUrl = string.Format("http://www.r2library.com/TrustedAuthentication/?authenticationKey={0}", TrustedAuthenticationKey);
            
            WebRequest webRequest = WebRequest.Create(webRequestUrl);

            HttpWebResponse response = (HttpWebResponse) webRequest.GetResponse();

            Console.WriteLine(response.StatusDescription);
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content. 
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            Console.WriteLine(responseFromServer);
            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();

            //Deserialize the response from the server
            WebTrustedAuthentication webTrustedAuthentication =
                new JavaScriptSerializer().Deserialize<WebTrustedAuthentication>(responseFromServer);

            //If error message is not emtpy there is a problem with the implementation
            if (!string.IsNullOrWhiteSpace(webTrustedAuthentication.ErrorMessage))
            {
                ErrorMessage.InnerText = string.Format("Error: {0}", webTrustedAuthentication.ErrorMessage);
            }
            else
            {
                //Method to easily create all the query string parameters needed for authentication
                string queryStringParameters = webTrustedAuthentication.GetQueryStringParameters(AccountNumber);

                HomePageLink.HRef = string.Format("http://www.r2library.com/?{0}", queryStringParameters);
                HomePageLink.InnerText = "Home page";

                BookLink.HRef = string.Format("http://www.r2library.com/resource/detail/1449640397/pr0003?{0}", queryStringParameters);
                BookLink.InnerHtml = "Into a Book";
            }
        }
    }

    public class WebTrustedAuthentication
    {
        public string Timestamp { get; set; }
        public string Hash { get; set; }
        public string ErrorMessage { get; set; }

        public string GetQueryStringParameters(string accountNumber)
        {
            return string.Format("acctno={0}&timestamp={1}&hash={2}", accountNumber, Timestamp, Hash);
        }
    }
}