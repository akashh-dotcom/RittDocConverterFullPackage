using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using IXIACOMPONENTSLib;
using TEXTMLPROXYLib;
using TEXTMLCOMMONLib;
using log4net;
using IxiaList = TEXTMLPROXYLib.IxiaList;

namespace R2V2.TextMLExport.Tasks.TextML
{
	public class TextMLService
	{
		private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName);

		private readonly List<string> badFiles = new List<string>();

		public int ExtractResourceDocuments(string isbn)
		{
			DirectoryInfo directoryInfo = null;

			int docCount = 0;
			IxiaResultSpace resultSpace = ExecuteTextmlSearch(GetTextMlQueryXml2(isbn));
			foreach (IxiaDocument2 textmlDoc in resultSpace)
			{
				if (directoryInfo == null)
				{
					directoryInfo = CreateResourceDirectory(isbn);
				}

				docCount++;
				Log.DebugFormat("{1}. textmlDoc.FileName: {0}, Name: {2}", textmlDoc.FileName, docCount, textmlDoc.Name);
				SaveResourceXml(textmlDoc, directoryInfo);
			}
			return docCount;
		}

		public int ExtractAllResourcesDocuments()
		{
			int docCount = 0;
			string query = GetAllDocsTextMlQueryXml();
			Log.DebugFormat("query: {0}", query);
			IxiaResultSpace resultSpace = ExecuteTextmlSearch(query);

			Log.DebugFormat("document count: {0}", resultSpace.Count);
			int totalDocumentCount = resultSpace.Count;
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			decimal percentageComplete;
			decimal percentageErrors;
			long logTime = 0;

			foreach (IxiaDocument2 textmlDoc in resultSpace)
			{
				Log.DebugFormat("Name: {0}", textmlDoc.Name);
				var fileParts = textmlDoc.Name.Split('.');
				string isbn = fileParts[1];
				DirectoryInfo directoryInfo = GetResourceDirectory(isbn);
				docCount++;
				Log.DebugFormat("{1}. textmlDoc.FileName: {0}, Name: {2}", textmlDoc.FileName, docCount, textmlDoc.Name);
				SaveResourceXml(textmlDoc, directoryInfo);

				if (stopwatch.ElapsedMilliseconds > logTime)
				{
					logTime += 15000;
					percentageComplete = ((decimal)docCount / (decimal)totalDocumentCount);
					percentageErrors = ((decimal)badFiles.Count / (decimal)docCount);
					Log.InfoFormat("{0:#0.000%} - {1} of {2}, run time: {3:c}, bad files: {4}, {5:#0.0000%}",
						percentageComplete, docCount, totalDocumentCount, stopwatch.Elapsed,
						badFiles.Count, percentageErrors);
				}
			}
			stopwatch.Stop();
			percentageComplete = ((decimal)docCount / (decimal)totalDocumentCount);
			percentageErrors = ((decimal)badFiles.Count / (decimal)docCount);
			Log.InfoFormat("{0:#0.000%} - {1} of {2}, run time: {3:c}, bad files: {4}, {5:#0.0000%}",
				percentageComplete, docCount, totalDocumentCount, stopwatch.Elapsed,
				badFiles.Count, percentageErrors);
			Log.DebugFormat("badFiles: {0}", string.Join(", ", badFiles));
			return docCount;
		}

		public static IxiaResultSpace ExecuteTextmlSearch(string query)
		{
			IxiaDocBaseServices2 objDocBaseServices = ConnectDocBase();
			Object objResultAsObject;// = new object();

			objDocBaseServices.SearchServices.SearchDocuments(query, out objResultAsObject);
			return (IxiaResultSpace)objResultAsObject;

		}

		private static IxiaDocBaseServices2 ConnectDocBase()
		{
			ClientServices objClientServices = new ClientServices();
			string serverName = Setting.Default.TextmlServer;
			string docBaseName = Setting.Default.TextmlDocBase;

			IxiaServerServices objServerServices = objClientServices.ConnectServer(serverName);
			return (IxiaDocBaseServices2)objServerServices.ConnectDocBase(docBaseName);
		}


		public static string GetTextMlQueryXml2(string isbn)
		{
			var sb = new StringBuilder();
			sb.Append("<?xml version='1.0' encoding='UTF-16'?>");
			sb.Append("<query VERSION='3.6' RESULTSPACE='ALL_INDEX_RS'>");
			sb.Append("<key NAME='isbn'>");
			sb.AppendFormat("<elem>{0}</elem>", isbn);
			sb.Append("</key></query>");

			return sb.ToString();
		}

		public static string GetAllDocsTextMlQueryXml()
		{
			var sb = new StringBuilder();
			sb.Append("<?xml version='1.0' encoding='UTF-16'?>");
			sb.Append("<query VERSION=\"3.6\" RESULTSPACE=\"R1\">");
			sb.Append("<property NAME=\"DocType\"><elem> TEXTML_DOCUMENT </elem></property></query>");
			return sb.ToString();
		}

		public static DirectoryInfo CreateResourceDirectory(string isbn)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(string.Format("{0}/{1}", Setting.Default.OutputDirectory, isbn));
			if (!directoryInfo.Exists)
			{
				directoryInfo.Create();
			}
			return directoryInfo;
		}

		public static DirectoryInfo GetResourceDirectory(string isbn)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(string.Format("{0}/{1}", Setting.Default.OutputDirectory, isbn));
			if (!directoryInfo.Exists)
			{
				directoryInfo.Create();
			}
			return directoryInfo;
		}


		public void SaveResourceXml(IxiaDocument2 textmlDoc, DirectoryInfo directoryInfo)
		{
			string xml = GetDocumentXmlFromTextML(textmlDoc);
			string filename = string.Format("{0}\\{1}", directoryInfo.FullName, textmlDoc.Name);
			var msg = new StringBuilder();
			msg.AppendFormat("filename: {0}", filename);

			// write XML to file on failure
			try
			{
				XmlReaderSettings readerSettings = new XmlReaderSettings
				                                   	{
				                                   		IgnoreWhitespace = true,
				                                   		IgnoreComments = true,
				                                   		CheckCharacters = true,
				                                   		CloseInput = true,
				                                   		IgnoreProcessingInstructions = false,
				                                   		ValidationFlags = System.Xml.Schema.XmlSchemaValidationFlags.None,
				                                   		ValidationType = ValidationType.None,
				                                   		XmlResolver = null,
				                                   		DtdProcessing = DtdProcessing.Ignore,
				                                   	};

				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();
				byte[] bytes = Encoding.UTF8.GetBytes(xml);
				MemoryStream memoryStream = new MemoryStream(bytes);
				XmlReader xmlReader = XmlReader.Create(memoryStream, readerSettings);
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(xmlReader);
				stopwatch.Stop();
				msg.AppendFormat("XML Load Time: {0} ms", stopwatch.ElapsedMilliseconds);

				xmlDoc.PreserveWhitespace = false;
				stopwatch.Restart();

				string newXml = xmlDoc.InnerXml;
				File.WriteAllText(filename, newXml);

				stopwatch.Stop();
				//Log.DebugFormat("XML Save Time: {0} ms", stopwatch.ElapsedMilliseconds);
				msg.AppendFormat("XML Save Time: {0} ms", stopwatch.ElapsedMilliseconds);
				return;
			}
			catch (Exception ex)
			{
				//Log.WarnFormat("ERROR PARSING XML: {0}", xml);
				Log.WarnFormat("BAD XML: {0}", textmlDoc.Name);
				Log.Warn(ex.Message, ex);
				badFiles.Add(textmlDoc.Name);
			}

			// write XML to file on failure
			File.WriteAllText(filename, xml);
		}

		private string GetDocumentXmlFromTextML(IxiaDocument2 textmlDoc)//, XmlDocument oXmlDoc)
		{
			string xml = string.Empty;
			try
			{
				Log.DebugFormat("textmlDoc.Content: {0}", textmlDoc.Content.ToString());
				
				if (textmlDoc.Content is Byte[])
				{
					MemoryStream memoryStream = new MemoryStream((Byte[])textmlDoc.Content);

					//Log.Debug(memoryStream.ToString());

					TextReader textReader = new StreamReader(memoryStream);

					xml = textReader.ReadToEnd();
				}
				else
				{
					Log.Warn("TEXTML CONTENT NOT BYTE ARRAY!");
				}
				return xml;
			}
			catch (Exception ex)
			{
				Log.InfoFormat("xml: {0}", xml);
				Log.ErrorFormat(ex.Message, ex);
				throw;
			}
		}


		public bool ExtractBookXmlFileFromTextML(string isbn)
		{
			DirectoryInfo directoryInfo = CreateResourceDirectory(isbn);

			var directoryNames = { "Medicine", "Nursing", "Allied Health", "Drug Monograph" };

			foreach (var directoryName in directoryNames)
			{
				try
				{
					string docName = string.Format("/{0}/book.{1}.xml", directoryName, isbn);

					Log.DebugFormat("docName: {0}", docName);
					IxiaDocBaseServices2 docBaseServices = ConnectDocBase();

					DynamicList docInList = new DynamicList {docName};

					object docOutList;
					docBaseServices.DocumentServices.GetDocuments((IxiaList)docInList, out docOutList, 11, TEXTML_DOCUMENT_TYPE.TEXTML_DOCUMENT);

					IxiaList docList = (IxiaList)docOutList;
					IxiaDocument2 textmlDoc = (IxiaDocument2)docList.Item(0);

					SaveResourceXml(textmlDoc, directoryInfo);
					return true;
				}
				catch (Exception ex)
				{
					Log.Debug(ex.Message);
				}
			}
			return false;
		}


		//Public Function GetDocument(ByVal strDocName As String, _
		//                ByRef xmlDocument As XmlDocument) As Integer

		//    Dim oDocList As Object = Nothing
		//    Dim oIxiaDoc As IxiaDocument2 = Nothing
		//    Dim lstDocumentName As New DynamicListClass
		//    Dim lstDocument As TEXTMLPROXYLib.IxiaList = Nothing
		//    Dim iReturnCode As Integer
		//    Dim oDocBaseSrv As IxiaDocBaseServices2
		//    Dim sLogString As String

		//    If MyBase._ApplicationDebug Then
		//        Me._MethodName = "GetDocument"
		//        sLogString = MyBase._BeginMethod & MyBase._ClassName & "." & Me._MethodName
		//        Me.DebugLog(sLogString)
		//    End If

		//    Try
		//        'Retrieve the document from document base
		//        iReturnCode = ConnectDocBase(oDocBaseSrv)
		//        If iReturnCode < 0 Then
		//            Return iReturnCode
		//        End If
		//        lstDocumentName.Add(strDocName)

		//        oDocBaseSrv.DocumentServices.GetDocuments(CType(lstDocumentName, TEXTMLPROXYLib.IxiaList), _
		//                oDocList, TEXTMLPROXYLib._ENUM_TEXTML_GETDOCUMENTS.TEXTML_DOCUMENT_CONTENT + TEXTMLPROXYLib._ENUM_TEXTML_GETDOCUMENTS.TEXTML_DOCUMENT_ERRORS + TEXTMLPROXYLib._ENUM_TEXTML_GETDOCUMENTS.TEXTML_DOCUMENT_PROPERTIES, TEXTMLPROXYLib.TEXTML_DOCUMENT_TYPE.TEXTML_DOCUMENT)
		//        lstDocument = CType(oDocList, TEXTMLPROXYLib.IxiaList)
		//        oIxiaDoc = CType(lstDocument.Item(0), IxiaDocument2)

		//        GetDocument = GetXmlDocFromIxiaDoc(oIxiaDoc, xmlDocument)
		//    Catch ex As Exception
		//        MyBase.HandleException("Error Getting Document " + strDocName, ex)
		//        GetDocument = Me.m_ERRORGETTINGXMLDOC
		//    End Try
		//    If MyBase._ApplicationDebug Then
		//        Me._MethodName = "GetDocument"
		//        sLogString = MyBase._EndMethod & MyBase._ClassName & "." & Me._MethodName
		//        Me.DebugLog(sLogString)
		//    End If
		//End Function

	}
}
