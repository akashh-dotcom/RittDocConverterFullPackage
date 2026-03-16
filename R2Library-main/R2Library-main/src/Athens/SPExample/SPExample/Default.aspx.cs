using System;
using System.Diagnostics;
using System.Text;
using System.Web.UI;
using Eduserv.OpenAthens;
using log4net;

namespace SPExample
{
    public partial class _Default : Page
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected void Page_Load(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            try
            {
                FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(@"C:\Program Files (x86)\Eduserv\OpenAthens.Net\Bin\OpenAthens.Net.dll");

                sb = new StringBuilder()
                    .AppendFormat("Status OK!!! \n")
                    .AppendFormat("File: {0} \nVersion number: {1}", myFileVersionInfo.FileDescription,
                        myFileVersionInfo.FileVersion);
                Log.Info(sb);
                textLit.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                sb.Append("Status FAIL!!! \n")
                    .Append("Failed to get access to File: OpenAthens.Net.dll \n")
                    .AppendFormat("{0}\n{1}", ex.Message, ex);
                textLit.Text = sb.ToString();
            }
            
        }
    }
}
