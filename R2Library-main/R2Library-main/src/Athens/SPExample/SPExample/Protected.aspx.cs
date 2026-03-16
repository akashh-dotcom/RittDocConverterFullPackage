using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using log4net;

namespace SPExample
{
    public partial class Protected : Page
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected void Page_Load(object sender, EventArgs e)
        {
            Log.Info(">>> Attempting Athens Login");
            if (Session["oa_idp_entity"] != null)
            {
                //The objects you get back from Athens HAVE to be casted to a List of Strings. 
                //This is from Athens not my doing. 
                List<string> athensOrgIdList = Session["oa_urn:mace:eduserv.org.uk:athens:attribute-def:organisation:1.0:identifier"] as List<string>; //Old will be deprecated
                List<string> athensUserNameList = Session["oa_urn:mace:eduserv.org.uk:athens:attribute-def:person:1.0:username"] as List<string>; //Old will be deprecated
                List<string> athensPersistantUidList = Session["oa_urn:mace:eduserv.org.uk:athens:attribute-def:person:1.0:persistentUID"] as List<string>; //Old will be deprecated
                List<string> scopedAffiliationList = Session["oa_urn:oid:1.3.6.1.4.1.5923.1.1.1.9"] as List<string>;//scopedAffiliation e.g. member@rittenhouse.com
                List<string> targetedIdList = Session["oa_urn:oid:1.3.6.1.4.1.5923.1.1.1.10"] as List<string>;//targetedId - replaces persistantId

                string athensOrgId = athensOrgIdList != null ? athensOrgIdList.First() : "";
                string athensUserName = athensUserNameList != null ? athensUserNameList.First() : "";
                string athensPersistantUid = athensPersistantUidList != null ? athensPersistantUidList.First() : "";
                string scopedAffiliation = scopedAffiliationList != null ? scopedAffiliationList.First() : "";
                string targetedId = targetedIdList != null ? targetedIdList.First() : "";
                
                var timeStamp = DateTime.Now;
                var year = timeStamp.Year;
                var month = timeStamp.Month;
                var day = timeStamp.Day;
                var hour = timeStamp.Hour;
                var minute = timeStamp.Minute;
                var second = timeStamp.Second;

                var formatedTimeStamp = $"{month}/{day}/{year} {hour}:{minute}:{second}";


                HttpCookie athensCookie = new HttpCookie("athensAuthentication")
                {
                    Value = $"organizationId={athensOrgId}|username={athensUserName}|persistentUid={athensPersistantUid}|scopedAffiliation={scopedAffiliation}|targetedId={targetedId}|formatedDate={formatedTimeStamp}",
                    Expires = DateTime.MinValue,
                    Secure = true
                };
                Log.Info("Cookie set");
                Log.Info(athensCookie.Value);
                Response.Cookies.Add(athensCookie);
            }
            var redirectUrl = "/Authentication/Athens";
            Log.Info("<<<< Athens Login Complete");

            Response.Redirect(redirectUrl);
        }
    }
}