#region

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Xml;
using System.Xml.Serialization;
using R2V2.Core.Api;

#endregion

namespace R2V2.Web.Controllers.SuperTypes
{
    public class BaseApiController : ApiController
    {
        public FormattedContentResult<string> SendXmlResponse<T>(object data)
        {
            try
            {
                if (data is string)
                {
                    var errorMessage = new ApiError
                    {
                        Success = false,
                        Message = $"{data}",
                        Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    var xmlContent = SerializeXmlData<T>(errorMessage);
                    return Content(HttpStatusCode.BadRequest, xmlContent,
                        new StringContentTypeFormatter("application/xml"));
                }
                else
                {
                    var xmlContent = SerializeXmlData<T>(data);
                    return Content(HttpStatusCode.OK, xmlContent, new StringContentTypeFormatter("application/xml"));
                }
            }
            catch (Exception ex)
            {
                var errorMessage = new ApiError
                {
                    Success = false,
                    Message = "Unknown Error",
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                var xmlContent = SerializeXmlData<ApiError>(errorMessage);
                return Content(HttpStatusCode.BadRequest, xmlContent,
                    new StringContentTypeFormatter("application/xml"));
            }
        }

        private string SerializeXmlData<T>(object data)
        {
            var poSerializer = new XmlSerializer(typeof(T));
            var xmlNamespaces = new XmlSerializerNamespaces();
            xmlNamespaces.Add("", ""); // Empty namespace to avoid xmlns attributes
            using (var stringWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
                       {
                           Indent = true, // Optional: for readable XML
                           OmitXmlDeclaration = false // Ensure XML declaration is included
                       }))
                {
                    poSerializer.Serialize(xmlWriter, data, xmlNamespaces);
                    return stringWriter.ToString();
                }
            }
        }
    }

    public class StringContentTypeFormatter : MediaTypeFormatter
    {
        private readonly string _mediaType;

        public StringContentTypeFormatter(string mediaType)
        {
            _mediaType = mediaType;
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaType));
        }

        public override bool CanReadType(Type type)
        {
            return false; // Not used for reading
        }

        public override bool CanWriteType(Type type)
        {
            return type == typeof(string); // Only supports writing strings
        }

        public override async Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
            TransportContext transportContext)
        {
            if (value is string stringValue)
            {
                var bytes = Encoding.UTF8.GetBytes(stringValue);
                await writeStream.WriteAsync(bytes, 0, bytes.Length);
            }
        }
    }
}