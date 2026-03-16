#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.Tasks.ContentTasks.AhfsDrugMonograph;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public class AhfsDrugMonographLoaderTask : TaskBase
    {
        private readonly StringBuilder _results = new StringBuilder();

        /// <summary>
        /// </summary>
        public AhfsDrugMonographLoaderTask()
            : base("AhfsDrugMonographLoaderTask", "-AhfsDrugMonographLoaderTask", "x12", TaskGroup.Deprecated,
                "Loads the AHFS Drug Information data", false)
        {
        }

        public override void Run()
        {
            TaskResult.Information = "This task will load the AHFS Drug Information";
            var step = new TaskResultStep { Name = "AhfsDrugMonographLoaderTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            try
            {
                UpdateTaskResult();

                Process();
                step.Results = _results.ToString();
                step.CompletedSuccessfully = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                step.CompletedSuccessfully = false;
                step.Results = ex.Message;
                throw;
            }
            finally
            {
                step.EndTime = DateTime.Now;
                UpdateTaskResult();
            }
        }

        private void Process()
        {
            var path = @"D:\ClientsNoBackup\Rittenhouse\AHFS\ahfs_20130916_xml\";
            var dirInfo = new DirectoryInfo(path);
            Log.DebugFormat("exists: {0}, sourcePath: {1}", dirInfo.Exists, path);

            if (!dirInfo.Exists)
            {
                _results.AppendFormat("source directory does not exist, '{0}'", path).AppendLine();
                return;
            }

            var files = dirInfo.GetFiles("a*.xml");
            if (files.Length == 0)
            {
                Log.WarnFormat("source directory is empty, '{0}'", path);
                _results.AppendFormat("source directory is empty, '{0}'", path).AppendLine();
                return;
            }

            var ahfsDrugDataService = new AhfsDrugDataService();

            var fileCount = 0;
            var drugCount = 0;
            var drugs = new List<AhfsDrug>();
            foreach (var fileInfo in files)
            {
                fileCount++;
                Log.InfoFormat(">>> {0} of {1} - {2}", fileCount, files.Length, fileInfo.Name);
                var xmlDocument = new XmlDocument();
                var xmlPath = fileInfo.FullName;
                xmlDocument.Load(xmlPath);

                var drug = ParseDrug(xmlDocument);
                if (drug != null)
                {
                    drugs.Add(drug);
                    Log.Debug(drug.ToDebugString());
                    drugCount++;
                    Log.InfoFormat("Drug Count: {0} of {1}", drugCount, fileCount);
                    drug.XmlFileName = fileInfo.Name;
                    ahfsDrugDataService.Insert(drug);
                }
            }
        }

        private AhfsDrug ParseDrug(XmlDocument xmlDocument)
        {
            var fullTitle = GetXmlNodeInnerText(xmlDocument, "//dif/ahfs/ahfs-mono/intro-info/full-title");
            Log.Info(fullTitle);

            if (!DoesFileContainDrugInfo(xmlDocument))
            {
                Log.InfoFormat("Not a drug! - {0}", fullTitle);
                return null;
            }

            var drug = new AhfsDrug
            {
                UnitNumber = GetXmlNodeInnerText(xmlDocument, "//dif/ahfs/ahfs-mono/unit-num"),
                FullTitle = fullTitle,
                ShortTitle = GetXmlNodeInnerText(xmlDocument, "//dif/ahfs/ahfs-mono/intro-info/short-title")
                //GenericName = GetXmlNodeInnerText(xmlDocument, "//dif/ahfs/ahfs-mono/intro-info/drug-name-info/gen-name"),
                //ChemicalName = GetXmlNodeInnerText(xmlDocument, "//dif/ahfs/ahfs-mono/intro-info/drug-name-info/chem-name"),
                //Introduction = GetXmlNodeInnerText(xmlDocument, "//dif/ahfs/ahfs-mono/intro-desc/para")
            };

            SetPrintClassData(xmlDocument, drug);
            SetPrintTitles(xmlDocument, drug);
            SetSynonyms(xmlDocument, drug);
            SetGenericNames(xmlDocument, drug);
            SetChecmicalName(xmlDocument, drug);
            SetIntroduction(xmlDocument, drug);

            return drug;
        }

        private bool DoesFileContainDrugInfo(XmlDocument xmlDocument)
        {
            var nodes = xmlDocument.SelectNodes("//dif/ahfs/ahfs-mono/intro-info/drug-name-info");
            if (nodes == null || nodes.Count == 0)
            {
                return false;
            }

            return true;
        }

        private void SetPrintClassData(XmlDocument xmlDocument, AhfsDrug drug)
        {
            var nodes = xmlDocument.SelectNodes("//dif/ahfs/ahfs-mono/intro-info/print-class/class-num");
            if (nodes != null && nodes.Count != 1)
            {
                Log.WarnFormat("Multiple node found, //dif/ahfs/ahfs-mono/intro-info/print-class/class-num, {0}",
                    nodes.Count);
            }


            var classNumMode = xmlDocument.SelectSingleNode("//dif/ahfs/ahfs-mono/intro-info/print-class/class-num");
            if (classNumMode != null)
            {
                if (classNumMode.Attributes == null)
                {
                    Log.WarnFormat("node.Attributes is null - {0}", classNumMode.Name);
                }
                else
                {
                    var classCode = classNumMode.Attributes["class-code-ref"];
                    if (classCode != null)
                    {
                        drug.ClassNumber = classCode.Value;
                    }
                    else
                    {
                        Log.Warn("class-code-ref is null");
                    }

                    var classText = classNumMode.Attributes["class-text"];
                    if (classText != null)
                    {
                        drug.ClassText = classText.Value;
                    }
                    else
                    {
                        Log.Warn("class-text is null");
                    }
                }
            }
            else
            {
                Log.Warn("//dif/ahfs/ahfs-mono/intro-info/print-class/class-num is NULL");
            }
        }

        private void SetSynonyms(XmlDocument xmlDocument, AhfsDrug drug)
        {
            var nodes = xmlDocument.SelectNodes("//dif/ahfs/ahfs-mono/intro-info/synonym");
            if (nodes != null && nodes.Count > 0)
            {
                foreach (XmlNode node in nodes)
                {
                    if (node.Attributes == null)
                    {
                        Log.WarnFormat("node.Attributes is null - {0}", node.InnerText);
                        continue;
                    }

                    var suppress = node.Attributes["suppress-from-index"];
                    if (suppress != null && suppress.Value == "suppress")
                    {
                        Log.InfoFormat("synonym suppressed - {0}", node.InnerText);
                        continue;
                    }

                    drug.AddSynonym(node.InnerText);
                }
            }
        }

        private void SetPrintTitles(XmlDocument xmlDocument, AhfsDrug drug)
        {
            var nodes = xmlDocument.SelectNodes("//dif/ahfs/ahfs-mono/intro-info/print-title");
            if (nodes != null && nodes.Count > 0)
            {
                foreach (XmlNode node in nodes)
                {
                    drug.AddPrintName(node.InnerText);
                }
            }
            else
            {
                Log.Warn("Node not found - //dif/ahfs/ahfs-mono/intro-info/print-title");
            }
        }

        private void SetGenericNames(XmlDocument xmlDocument, AhfsDrug drug)
        {
            var nodes = xmlDocument.SelectNodes("//dif/ahfs/ahfs-mono/intro-info/drug-name-info/gen-name");
            if (nodes != null && nodes.Count > 0)
            {
                foreach (XmlNode node in nodes)
                {
                    drug.AddGenericName(node.InnerText);
                }
            }
            else
            {
                Log.Warn("Node not found - //dif/ahfs/ahfs-mono/intro-info/drug-name-info/gen-name");
            }
        }

        private void SetChecmicalName(XmlDocument xmlDocument, AhfsDrug drug)
        {
            var nodes = xmlDocument.SelectNodes("//dif/ahfs/ahfs-mono/intro-info/drug-name-info/chem-name");
            if (nodes != null && nodes.Count > 0)
            {
                foreach (XmlNode node in nodes)
                {
                    drug.AddChecmicalName(node.InnerText);
                }
            }
        }

        private void SetIntroduction(XmlDocument xmlDocument, AhfsDrug drug)
        {
            var intruduction = new StringBuilder();
            var nodes = xmlDocument.SelectNodes("//dif/ahfs/ahfs-mono/intro-desc/para");
            if (nodes != null && nodes.Count > 0)
            {
                foreach (XmlNode node in nodes)
                {
                    intruduction.Append(node.OuterXml);
                }
            }
            else
            {
                Log.Warn("Node not found - //dif/ahfs/ahfs-mono/intro-desc/para");
            }

            drug.Introduction = intruduction.ToString();
        }


        private string GetXmlNodeInnerText(XmlDocument xmlDocument, string xpath)
        {
            var nodes = xmlDocument.SelectNodes(xpath);
            if (nodes == null || nodes.Count == 0)
            {
                Log.WarnFormat("Node not found - {0}", xpath);
                return null;
            }

            if (nodes.Count == 1)
            {
                return nodes[0].InnerText;
            }

            Log.WarnFormat("Multiple Nodes Found, {0}, for {1}", nodes.Count, xpath);
            foreach (XmlNode node in nodes)
            {
                Log.InfoFormat("{0} = {1}", xpath, node.InnerText);
            }

            return nodes[0].InnerText;
        }
    }
}