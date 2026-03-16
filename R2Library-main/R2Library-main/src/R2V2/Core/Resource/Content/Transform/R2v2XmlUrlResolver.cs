#region

using System;
using System.IO;
using System.Xml;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.Core.Resource.Content.Transform
{
    public class R2V2XmlUrlResolver : XmlUrlResolver
    {
        private readonly IContentSettings _contentSettings;

        public R2V2XmlUrlResolver(IContentSettings contentSettings)
        {
            _contentSettings = contentSettings;
        }

        public override Uri ResolveUri(Uri baseUri, string relativeUri)
        {
            if (IsIgnoredEntityUri(baseUri, relativeUri, _contentSettings))
            {
                return IgnoredExternalEntityUri(_contentSettings);
            }

            if (relativeUri.StartsWith(".."))
            {
                return base.ResolveUri(baseUri,
                    Path.GetFullPath(_contentSettings.XslLocation + relativeUri.TrimStart('.')));
            }

            var segment = "";

            // This is a little ugly.  Refactor and/or comment.
            var segments = (baseUri ?? new Uri(relativeUri)).Segments;
            for (var i = segments.Length - 1; i > 0; i--)
            {
                segment = Path.Combine(segments[i], segment);
                var path = Path.Combine(_contentSettings.XslLocation, segment);
                if (File.Exists(path))
                {
                    return base.ResolveUri(baseUri, path);
                }
            }

            return base.ResolveUri(baseUri, relativeUri);
        }

        public static bool IsIgnoredEntityUri(Uri baseUri, string relativeUri, IContentSettings contentSettings)
        {
            if (contentSettings.IgnoreExternalEntities)
            {
                if (baseUri != null && baseUri.ToString().Contains("book.") && baseUri.ToString().EndsWith(".xml"))
                {
                    if (relativeUri != null && relativeUri.ToLower().EndsWith(".xml") &&
                        !relativeUri.ToLower().StartsWith("appendix.") && !relativeUri.ToLower().Contains(".biblios"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static Uri IgnoredExternalEntityUri(IContentSettings contentSettings)
        {
            return new Uri(contentSettings.IgnoredExternalEntityUriPath);
        }
    }

    public class R2V2XmlEntityResolver : XmlUrlResolver
    {
        private readonly IContentSettings _contentSettings;

        public R2V2XmlEntityResolver(IContentSettings contentSettings)
        {
            _contentSettings = contentSettings;
        }

        public override Uri ResolveUri(Uri baseUri, string relativeUri)
        {
            return R2V2XmlUrlResolver.IsIgnoredEntityUri(baseUri, relativeUri, _contentSettings)
                ? R2V2XmlUrlResolver.IgnoredExternalEntityUri(_contentSettings)
                : base.ResolveUri(baseUri, relativeUri);
        }
    }
}