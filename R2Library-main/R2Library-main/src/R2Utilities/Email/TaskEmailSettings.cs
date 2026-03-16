#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using Common.Logging;

#endregion

namespace R2Utilities.Email
{
    public class TaskEmailSettings
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);
        private readonly string _emailConfigDirectory;

        /// <summary>
        /// </summary>
        /// <param name="emailConfigDirectory"> </param>
        public TaskEmailSettings(string taskKey, string emailConfigDirectory)
        {
            _emailConfigDirectory = emailConfigDirectory;
            TaskKey = taskKey;

            // verify the email config directry exists
            if (!Directory.Exists(_emailConfigDirectory))
            {
                Log.ErrorFormat("Email Config directory NOT found, _emailConfigDirectory: {0}", _emailConfigDirectory);
            }

            SuccessEmailConfig = new EmailConfiguration { Type = "Success" };
            ErrorEmailConfig = new EmailConfiguration { Type = "Error" };
            TaskEmailConfig = new EmailConfiguration { Type = "Task" };

            PopulateEmailConfigurations("Default", SuccessEmailConfig);
            PopulateEmailConfigurations("Default", ErrorEmailConfig);
            PopulateEmailConfigurations("Default", TaskEmailConfig);
            PopulateEmailConfigurations(taskKey, SuccessEmailConfig);
            PopulateEmailConfigurations(taskKey, ErrorEmailConfig);
            PopulateEmailConfigurations(taskKey, TaskEmailConfig);
        }

        public string TaskKey { get; set; }

        /// <summary>
        ///     Email config used to send success message at task completion.
        /// </summary>
        public EmailConfiguration SuccessEmailConfig { get; }

        /// <summary>
        ///     Email config used to send error message at task completion.
        /// </summary>
        public EmailConfiguration ErrorEmailConfig { get; }

        /// <summary>
        ///     Email config used by the task to send messages during the task, example: reports.
        /// </summary>
        public EmailConfiguration TaskEmailConfig { get; }

        private void PopulateEmailConfigurations(string taskKey, EmailConfiguration emailConfiguration)
        {
            Log.DebugFormat("EmailConfigDirectory: {0}", _emailConfigDirectory);
            var xmlFilename = $@"{_emailConfigDirectory}\{taskKey}.xml";
            Log.DebugFormat("xmlFilename: {0}", xmlFilename);

            if (File.Exists(xmlFilename))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFilename);

                if (null != xmlDoc.DocumentElement)
                {
                    var xmlNodes =
                        xmlDoc.DocumentElement.SelectNodes(
                            "/RittenhouseWebLoader/EmailConfigurations/EmailConfiguration");
                    if (xmlNodes != null)
                    {
                        foreach (XmlNode xmlNode in xmlNodes)
                        {
                            Log.DebugFormat("xmlNode: {0}", xmlNode.Name);
                            if (xmlNode.Attributes == null)
                            {
                                throw new Exception($"Invalid Email Config XML in {xmlFilename}");
                            }

                            var type = xmlNode.Attributes["type"];
                            var send = xmlNode.Attributes["send"];
                            if (type.Value == emailConfiguration.Type)
                            {
                                PopulateEmailAddresses(emailConfiguration, xmlNode);
                                emailConfiguration.Send = send.Value.ToLower() == "true";
                            }
                        }
                    }
                }
            }
        }

        private static void PopulateEmailAddresses(EmailConfiguration emailConfiguration, XmlNode emailConfigNode)
        {
            var toEmailAddresses = new List<string>();
            var ccEmailAddresses = new List<string>();
            var bccEmailAddresses = new List<string>();

            //XmlNodeList emailAddressNodes = emailConfigNode.SelectNodes("/RittenhouseWebLoader/EmailConfigurations/EmailConfiguration/ToAddresses/EmailAddress");
            //XmlNodeList emailAddressNodes = emailConfigNode.SelectNodes("/RittenhouseWebLoader/EmailConfigurations/EmailConfiguration/ToAddresses/EmailAddress");

            var childNodes = emailConfigNode.ChildNodes;
            foreach (XmlNode childNode in childNodes)
            {
                if (childNode.Name == "ToAddresses")
                {
                    var emailAddressNodes = childNode.ChildNodes;

                    foreach (XmlNode emailAddressNode in emailAddressNodes)
                    {
                        if (emailAddressNode.Name == "EmailAddress")
                        {
                            Log.DebugFormat("emailAddressNode: {0}", emailAddressNode.InnerText);
                            toEmailAddresses.Add(emailAddressNode.InnerText);
                        }
                    }
                }

                if (childNode.Name == "CcAddresses")
                {
                    var emailAddressNodes = childNode.ChildNodes;

                    foreach (XmlNode emailAddressNode in emailAddressNodes)
                    {
                        if (emailAddressNode.Name == "EmailAddress")
                        {
                            Log.DebugFormat("emailAddressNode: {0}", emailAddressNode.InnerText);
                            ccEmailAddresses.Add(emailAddressNode.InnerText);
                        }
                    }
                }

                if (childNode.Name == "BccAddresses")
                {
                    var emailAddressNodes = childNode.ChildNodes;

                    foreach (XmlNode emailAddressNode in emailAddressNodes)
                    {
                        if (emailAddressNode.Name == "EmailAddress")
                        {
                            Log.DebugFormat("emailAddressNode: {0}", emailAddressNode.InnerText);
                            bccEmailAddresses.Add(emailAddressNode.InnerText);
                        }
                    }
                }
            }

            emailConfiguration.ToAddresses.Clear();
            if (toEmailAddresses.Count > 0)
            {
                emailConfiguration.ToAddresses.AddRange(toEmailAddresses);
            }

            emailConfiguration.CcAddresses.Clear();
            if (ccEmailAddresses.Count > 0)
            {
                emailConfiguration.CcAddresses.AddRange(ccEmailAddresses);
            }

            emailConfiguration.BccAddresses.Clear();
            if (bccEmailAddresses.Count > 0)
            {
                emailConfiguration.BccAddresses.AddRange(bccEmailAddresses);
            }
        }
    }
}