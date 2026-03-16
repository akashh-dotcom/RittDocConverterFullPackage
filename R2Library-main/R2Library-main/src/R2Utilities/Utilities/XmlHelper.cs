#region

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Common.Logging;

#endregion

namespace R2Utilities.Utilities
{
    public class XmlHelper
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        public static string GetXmlNodeValue(XmlDocument xmlDoc, string xpath)
        {
            if (null != xmlDoc.DocumentElement)
            {
                var xmlNodeList = xmlDoc.DocumentElement.SelectNodes(xpath);

                if (null == xmlNodeList)
                {
                    return string.Empty;
                }

                if (xmlNodeList.Count == 0)
                {
                    //Log.WarnFormat("NODE NOT FOUND, xpath: {0}, count:{1}", xpath, xmlNodeList.Count);
                    return string.Empty;
                }

                if (xmlNodeList.Count > 1)
                {
                    //Log.WarnFormat("MULTIPLE NODES FOUND, xpath: {0}, count:{1}", xpath, xmlNodeList.Count);
                    return string.Empty;
                }

                var value = xmlNodeList[0].InnerText;
                return value.Replace("\r", "").Replace("\n", "").Replace("\t", "");
            }

            return "";
        }

        public static string GetXmlNodeValue(XmlNode xmlNode)
        {
            if (null == xmlNode)
            {
                return "";
            }

            var value = xmlNode.InnerText;
            return value.Replace("\r", "").Replace("\n", "").Replace("\t", "");
        }

        public static XmlNode GetXmlNode(XmlDocument xmlDoc, string xpath)
        {
            if (null != xmlDoc.DocumentElement)
            {
                var xmlNodeList = xmlDoc.DocumentElement.SelectNodes(xpath);

                if (null == xmlNodeList)
                {
                    return null;
                }

                if (xmlNodeList.Count == 0)
                {
                    //Log.WarnFormat("NODE NOT FOUND, xpath: {0}, count:{1}", xpath, xmlNodeList.Count);
                    return null;
                }

                if (xmlNodeList.Count > 1)
                {
                    //Log.WarnFormat("MULTIPLE NODES FOUND, xpath: {0}, count:{1}", xpath, xmlNodeList.Count);
                    return null;
                }

                return xmlNodeList[0];
            }

            return null;
        }


        public static List<XmlNode> GetXmlNodes(XmlDocument xmlDoc, string xpath)
        {
            var nodes = new List<XmlNode>();
            if (null != xmlDoc.DocumentElement)
            {
                var xmlNodeList = xmlDoc.DocumentElement.SelectNodes(xpath);
                if (xmlNodeList != null)
                {
                    nodes.AddRange(xmlNodeList.Cast<XmlNode>());
                }
            }

            return nodes;
        }

        public static void AppendXmlNode(XmlDocument xmlDoc, XmlNode parentNode, string nodeName, string nodeValue)
        {
            if (!string.IsNullOrEmpty(nodeValue))
            {
                var childNode = xmlDoc.CreateNode(XmlNodeType.Element, nodeName, null);
                childNode.InnerText = nodeValue;
                //childNode.InnerXml =  nodeValue;
                parentNode.AppendChild(childNode);
            }
        }

        public static string GetAttributeValue(XmlNode node, string name)
        {
            if (node.Attributes == null)
            {
                return string.Empty;
            }

            var attribute = node.Attributes[name];

            if (attribute == null)
            {
                return string.Empty;
            }

            return attribute.Value;
        }

        public static XmlDocument StripTags(XmlDocument xmlDoc, string xpathToStrip)
        {
            var xmlNodeList = xmlDoc.SelectNodes(xpathToStrip);

            if (xmlNodeList == null) return xmlDoc;

            var listXmlNode = xmlNodeList.Cast<XmlNode>().Where(n => !n.IsReadOnly).OrderBy(n => n.InnerXml.Length)
                .ToList();
            foreach (var xmlNode in listXmlNode)
            {
                var xmlFragment = xmlDoc.CreateDocumentFragment();
                xmlFragment.InnerXml = xmlNode.InnerXml;

                if (xmlNode.ParentNode == null)
                {
                    xmlDoc.ReplaceChild(xmlFragment, xmlNode);
                    continue;
                }

                xmlNode.ParentNode.ReplaceChild(xmlFragment, xmlNode);
            }

            return xmlDoc;
        }

        public static XmlDocument RemoveComments(XmlDocument xmlDoc)
        {
            var xmlNodeList = xmlDoc.SelectNodes("//comment()");

            if (xmlNodeList == null) return xmlDoc;

            foreach (XmlNode xmlNode in xmlNodeList)
            {
                if (xmlNode.ParentNode == null)
                {
                    xmlDoc.RemoveChild(xmlNode);
                    continue;
                }

                xmlNode.ParentNode.RemoveChild(xmlNode);
            }

            return xmlDoc;
        }
    }
}