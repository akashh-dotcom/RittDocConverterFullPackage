#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using R2Library.Data.ADO.R2Utility;
using R2Utilities.DataAccess.Tabers;
using R2Utilities.Infrastructure.Settings;
using R2Utilities.Utilities;
using R2V2.Extensions;

#endregion

namespace R2Utilities.Tasks.ContentTasks
{
    public partial class LoadTabersDictionaryTask : TaskBase
    {
        private readonly IR2UtilitiesSettings _r2UtilitiesSettings;
        private readonly TabersDataService _tabersDataService;

        public LoadTabersDictionaryTask(IR2UtilitiesSettings r2UtilitiesSettings, TabersDataService tabersDataService)
            : base("LoadTabersDictionaryTask", "-LoadTabersDictionaryTask", "07", TaskGroup.ContentLoading,
                "Task to load Tabers Dictionary terms", true)
        {
            _r2UtilitiesSettings = r2UtilitiesSettings;
            _tabersDataService = tabersDataService;
        }

        public override void Run()
        {
            TaskResult.Information = "This task will load the Taber's Dictionary database from the Taber's Source XML.";
            var step = new TaskResultStep { Name = "LoadTabersDictionaryTask", StartTime = DateTime.Now };
            TaskResult.AddStep(step);
            UpdateTaskResult();

            try
            {
                LoadTabersContent();
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

        private IEnumerable<FileInfo> GetXmlFileInfo()
        {
            var tabersXmlPath = _r2UtilitiesSettings.TabersXmlPath;
            var directoryInfo = new DirectoryInfo(tabersXmlPath);

            return directoryInfo.EnumerateFiles();
        }

        private void LoadTabers()
        {
            var xmlFileInfo = GetXmlFileInfo();

            foreach (var fileInfo in xmlFileInfo)
            {
                var xmlDocument = new XmlDocument();

                var xmlPath = fileInfo.FullName;
                xmlDocument.Load(xmlPath);

                LoadTerms(xmlDocument);
            }
        }

        private void LoadTerms(XmlNode document)
        {
            var mainEntries = document.SelectNodes("//mainentry");
            if (mainEntries == null) return;

            foreach (XmlNode mainEntry in mainEntries)
            {
                LoadTerm(mainEntry);
            }
        }

        private void LoadTerm(XmlNode mainEntry)
        {
            var mainEntryKey = LoadMainEntry(mainEntry);

            LoadSpecialty(mainEntry, mainEntryKey);
            LoadSenses(mainEntry, mainEntryKey);
            LoadVariants(mainEntry, mainEntryKey);
            LoadPronounces(mainEntry, mainEntryKey);
            LoadPlurals(mainEntry, mainEntryKey);
            LoadEtymologies(mainEntry, mainEntryKey);
            LoadSubentries(mainEntry, mainEntryKey);
            LoadAbbrevXentries(mainEntry, mainEntryKey, null);
            LoadXentries(mainEntry, mainEntryKey, null);
        }

        private int LoadMainEntry(XmlNode mainEntry)
        {
            var currency = XmlHelper.GetAttributeValue(mainEntry, "currency");
            var dateRevised = XmlHelper.GetAttributeValue(mainEntry, "date-revised")
                .TryParseExact<DateTime>("ddd MMM dd HH:mm:ss yyyy");
            var name = mainEntry.Attributes["dbname"].IfNotNull(a => a.InnerXml);
            var editionAdded = XmlHelper.GetAttributeValue(mainEntry, "edition-added").TryParse<int>();
            var letter = XmlHelper.GetAttributeValue(mainEntry, "letter");
            var output = XmlHelper.GetAttributeValue(mainEntry, "output");
            var sortOrder = XmlHelper.GetAttributeValue(mainEntry, "sortorder").TryParse<int>();
            var spaceSaver = XmlHelper.GetAttributeValue(mainEntry, "spacesaver");
            var xlinkType = XmlHelper.GetAttributeValue(mainEntry, "xlink:type");
            var biography = mainEntry.SelectSingleNode("biography").IfNotNull(node => node.InnerXml);
            var abbrev = mainEntry.SelectSingleNode("abbrev").IfNotNull(node => node.InnerXml);
            var symb = mainEntry.SelectSingleNode("symb").IfNotNull(node => node.InnerXml);

            var orthoDisp = mainEntry.SelectSingleNode("ortho-disp");
            var orthoDispKey = LoadOrthoDisp(orthoDisp, null);

            return _tabersDataService.InsertMainEntry(currency, dateRevised, name, editionAdded, letter, output,
                sortOrder, spaceSaver, xlinkType, orthoDispKey, biography, abbrev, symb);
        }

        private int LoadOrthoDisp(XmlNode orthoDisp, int? pluralKey)
        {
            var orthoDispId = XmlHelper.GetAttributeValue(orthoDisp, "id");
            var orthoDispText = orthoDisp.InnerXml;

            return _tabersDataService.InsertOrthoDisp(orthoDispId, orthoDispText, pluralKey);
        }

        private void LoadSpecialty(XmlNode currentNode, int mainEntryKey)
        {
            var specialty = currentNode.SelectSingleNode("specialty");

            if (specialty == null) return;

            var primary1 = specialty.SelectSingleNode("primary1");

            var primary1Code = XmlHelper.GetAttributeValue(primary1, "code");

            _tabersDataService.InsertSpecialty(mainEntryKey, primary1Code);
        }

        private void LoadSenses(XmlNode currentNode, int mainEntryKey)
        {
            var senses = currentNode.SelectNodes("sense");

            if (senses == null) return;

            foreach (XmlNode sense in senses)
            {
                var definition = sense.SelectSingleNode("definition").IfNotNull(node => node.InnerXml);

                var senseKey = _tabersDataService.InsertSense(mainEntryKey, definition);

                LoadDefExp(sense, senseKey);
            }
        }

        private void LoadVariants(XmlNode currentNode, int mainEntryKey)
        {
            var variants = currentNode.SelectNodes("variant");

            if (variants == null) return;

            foreach (XmlNode variant in variants)
            {
                var orthoDisp = variant.SelectSingleNode("ortho-disp");

                var orthoDispKey = LoadOrthoDisp(orthoDisp, null);

                _tabersDataService.InsertVariant(mainEntryKey, orthoDispKey);
            }
        }

        private void LoadPronounces(XmlNode currentNode, int mainEntryKey)
        {
            var pronounces = currentNode.SelectNodes("pronounce");

            if (pronounces == null) return;

            foreach (XmlNode pronounce in pronounces)
            {
                var audio = pronounce.SelectSingleNode("audio");
                if (audio != null) pronounce.RemoveChild(audio);

                var pronounceText = pronounce.InnerXml;
                var audioFile = audio.IfNotNull(a => a.Attributes["file"].Value);

                _tabersDataService.InsertPronounce(mainEntryKey, pronounceText, audioFile);
            }
        }

        private void LoadPlurals(XmlNode currentNode, int mainEntryKey)
        {
            var plurals = currentNode.SelectNodes("plural");

            if (plurals == null) return;

            foreach (XmlNode plural in plurals)
            {
                var orthoDisps = plural.SelectNodes("ortho-disp");

                var pluralKey = _tabersDataService.InsertPlural(mainEntryKey);

                if (orthoDisps == null) return;

                foreach (XmlNode orthoDisp in orthoDisps)
                {
                    LoadOrthoDisp(orthoDisp, pluralKey);
                }
            }
        }

        private void LoadEtymologies(XmlNode currentNode, int mainEntryKey)
        {
            var etymologies = currentNode.SelectNodes("etymology");

            if (etymologies == null) return;

            foreach (XmlNode etymology in etymologies)
            {
                var etymologyText = etymology.InnerXml;

                _tabersDataService.InsertEtymology(mainEntryKey, etymologyText);
            }
        }

        private void LoadSubentries(XmlNode currentNode, int mainEntryKey)
        {
            var subentries = currentNode.SelectNodes("subentry");

            if (subentries == null) return;

            foreach (XmlNode subentry in subentries)
            {
                var currency = XmlHelper.GetAttributeValue(subentry, "currency");
                var dateRevised = XmlHelper.GetAttributeValue(subentry, "date-revised")
                    .TryParseExact<DateTime>("ddd MMM dd HH:mm:ss yyyy");
                var name = subentry.Attributes["dbname"].IfNotNull(a => a.InnerXml);
                var editionAdded = XmlHelper.GetAttributeValue(subentry, "edition-added").TryParse<int>();
                var output = XmlHelper.GetAttributeValue(subentry, "output");
                var spaceSaver = XmlHelper.GetAttributeValue(subentry, "spacesaver");
                var xlinkType = XmlHelper.GetAttributeValue(subentry, "xlink:type");

                var subentryKey = _tabersDataService.InsertSubentry(mainEntryKey, currency, dateRevised, name,
                    editionAdded, output, spaceSaver, xlinkType);

                LoadAbbrevXentries(subentry, null, subentryKey);
                LoadXentries(subentry, null, subentryKey);
            }
        }

        private void LoadAbbrevXentries(XmlNode currentNode, int? mainEntryKey, int? subentryKey)
        {
            var abbrevXentries = currentNode.SelectNodes("abbrev-xentry");

            if (abbrevXentries == null) return;

            foreach (XmlNode abbrevXentry in abbrevXentries)
            {
                var xlinkHref = XmlHelper.GetAttributeValue(abbrevXentry, "xlink:href");
                var abbrevXentryText = abbrevXentry.InnerXml;

                _tabersDataService.InsertAbbrevXentry(mainEntryKey, subentryKey, xlinkHref, abbrevXentryText);
            }
        }

        private void LoadXentries(XmlNode currentNode, int? mainEntryKey, int? subentryKey)
        {
            var xentries = currentNode.SelectNodes("xentry");

            if (xentries == null) return;

            foreach (XmlNode xentry in xentries)
            {
                var xlinkHref = XmlHelper.GetAttributeValue(xentry, "xlink:href");
                var xentryText = xentry.InnerXml;

                _tabersDataService.InsertXentry(mainEntryKey, subentryKey, xlinkHref, xentryText);
            }
        }

        private void LoadDefExp(XmlNode currentNode, int senseKey)
        {
            var defExp = currentNode.SelectSingleNode("defexp");

            if (defExp == null) return;

            var output = XmlHelper.GetAttributeValue(defExp, "output");
            var defExpText = defExp.InnerXml;

            _tabersDataService.InsertDefExp(senseKey, output, defExpText);
        }
    }
}