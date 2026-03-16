#region

using System.Xml;
using System.Xml.Xsl;
using R2V2.Infrastructure.DependencyInjection;
using R2V2.Core.Resource.Content;

#endregion

namespace R2V2.Extensions
{
    public static class XmlExtensions
    {
        public static void Transform(this XslCompiledTransform transform, XmlReader input, XsltArgumentList arguments,
            XmlWriter results, bool handleXslMessages)
        {
            transform.Transform(input, arguments, results, null, handleXslMessages);
        }

        public static void Transform(this XslCompiledTransform transform, XmlReader input, XsltArgumentList arguments,
            XmlWriter results, XmlResolver documentResolver, bool handleXslMessages)
        {
            XsltMessageHandler xsltMessageHandler = null;

            if (handleXslMessages)
            {
                xsltMessageHandler = ServiceLocator.Current.GetInstance<XsltMessageHandler>();
                arguments.XsltMessageEncountered += xsltMessageHandler.OnXsltMessageEncountered;
            }

            if (documentResolver != null) transform.Transform(input, arguments, results, documentResolver);
            else transform.Transform(input, arguments, results);

            if (handleXslMessages) arguments.XsltMessageEncountered -= xsltMessageHandler.OnXsltMessageEncountered;
        }
    }
}