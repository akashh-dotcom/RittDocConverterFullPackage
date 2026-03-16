#region

using System.Xml;

#endregion

namespace R2V2.Core.Resource.Content
{
    public class XmlTransformResult : ITransformResult
    {
        public XmlDocument Result { get; set; }

        public string Isbn { get; set; }
        public string Section { get; set; }
        public string OutputFilename { get; set; }
    }
}