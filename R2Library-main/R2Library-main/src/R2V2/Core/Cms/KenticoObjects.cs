#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace R2V2.Core.Cms
{
    class KenticoObjects
    {
    }

    public class CmsData
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string FormHeader { get; set; }
        public string FormRecipients { get; set; }
    }


    [Serializable]
    public class KenticoItems
    {
        [JsonProperty(PropertyName = "items")] public KenticoItem[] Items { get; set; }


        public KenticoItem[] CollectionList { get; set; }

        [JsonProperty(PropertyName = "modular_content")]
        public object CollectionObject
        {
            //get { return _collectionList; }
            set
            {
                var kenticoItems = new List<KenticoItem>();
                var test = value as IEnumerable;
                foreach (var x in test)
                {
                    var kenticoItem = ((JProperty)x).Value.ToObject<KenticoItem>();
                    kenticoItems.Add(kenticoItem);
                }

                CollectionList = kenticoItems.ToArray();
            }
        }
    }

    [Serializable]
    public class KenticoCollectionList
    {
        public IEnumerable<object> Items { get; set; }
    }

    [Serializable]
    public class KenticoItem
    {
        [JsonProperty(PropertyName = "system")]
        public KenticoName ItemName { get; set; }

        [JsonProperty(PropertyName = "elements")]
        public KenticoElement ItemValues { get; set; }


        public CmsData ToData()
        {
            var dataToReturn = new CmsData
            {
                Name = ItemName.ElementName,
                Type = $"{ItemValues.ContentType.Value?.FirstOrDefault()?.CodeName}",
                Value = ItemValues.ContentText?.Value
            };

            if (dataToReturn.Value == null)
            {
                dataToReturn.FormRecipients = ItemValues.FormRecipients.Value;
                dataToReturn.FormHeader = ItemValues.FormHeader.Value;
                if (ItemValues?.HtmlAssets?.Values != null)
                {
                    foreach (var asset in ItemValues.HtmlAssets.Values)
                    {
                        var assetName = asset.Name;
                        var assetUrl = asset.Url;
                        dataToReturn.FormHeader = dataToReturn.FormHeader.Replace(assetName, assetUrl);
                    }
                }
            }
            else
            {
                if (ItemValues?.HtmlAssets?.Values != null)
                {
                    foreach (var asset in ItemValues.HtmlAssets.Values)
                    {
                        var assetName = asset.Name;
                        var assetUrl = asset.Url;
                        dataToReturn.Value = dataToReturn.Value.Replace(assetName, assetUrl);
                    }
                }
            }

            return dataToReturn;
        }
    }

    [Serializable]
    public class KenticoAsset
    {
        [JsonProperty(PropertyName = "name")] public string Name { get; set; }
        [JsonProperty(PropertyName = "value")] public KenticoAssetItem[] Values { get; set; }
    }

    [Serializable]
    public class KenticoAssetItem
    {
        [JsonProperty(PropertyName = "name")] public string Name { get; set; }
        [JsonProperty(PropertyName = "type")] public string Type { get; set; }
        [JsonProperty(PropertyName = "size")] public int Size { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "url")] public string Url { get; set; }
    }


    [Serializable]
    public class KenticoName
    {
        [JsonProperty(PropertyName = "name")] public string ElementName { get; set; }

        [JsonProperty(PropertyName = "codename")]
        public string CodeName { get; set; }

        [JsonProperty(PropertyName = "type")] public string Type { get; set; }

        [JsonProperty(PropertyName = "id")] public string Id { get; set; }

        [JsonProperty(PropertyName = "last_modified")]
        public DateTime LastModified { get; set; }
    }

    [Serializable]
    public class KenticoElement
    {
        [JsonProperty(PropertyName = "content_requirement__active_date")]
        public KenticoDateField ActiveDate { get; set; }

        [JsonProperty(PropertyName = "content_requirement__expiration_date")]
        public KenticoDateField ExpirationDate { get; set; }

        [JsonProperty(PropertyName = "content_requirement__folder")]
        public KenticoTaxonomy ContentType { get; set; }

        [JsonProperty(PropertyName = "content_requirement__environment")]
        public KenticoTaxonomy Environment { get; set; }

        [JsonProperty(PropertyName = "content")]
        public KenticoTextField ContentText { get; set; }

        [JsonProperty(PropertyName = "form_header")]
        public KenticoTextField FormHeader { get; set; }

        [JsonProperty(PropertyName = "form_recipients")]
        public KenticoTextField FormRecipients { get; set; }

        [JsonProperty(PropertyName = "collection")]
        public KenticoCollection CollectionNames { get; set; }

        [JsonProperty(PropertyName = "image_upload")]
        public KenticoImageField ImageFile { get; set; }

        [JsonProperty(PropertyName = "image_destination")]
        public KenticoTextField ImageDestination { get; set; }

        [JsonProperty(PropertyName = "sort_order")]
        public KenticoIntField SortOrder { get; set; }

        [JsonProperty(PropertyName = "navigation_text")]
        public KenticoTextField NavigationText { get; set; }

        [JsonProperty(PropertyName = "navigation_hover_text")]
        public KenticoTextField NavigationHoverText { get; set; }

        [JsonProperty(PropertyName = "Assets")]
        public KenticoAsset HtmlAssets { get; set; }
    }

    public class KenticoCollection
    {
        [JsonProperty(PropertyName = "type")] public string Type { get; set; }

        [JsonProperty(PropertyName = "name")] public string Name { get; set; }

        [JsonProperty(PropertyName = "value")] public string[] Value { get; set; }
    }

    public class KenticoTaxonomy
    {
        [JsonProperty(PropertyName = "name")] public string Name { get; set; }

        [JsonProperty(PropertyName = "taxonomy_group")]
        public string CodeName { get; set; }

        public KenticoTaxonomyValue[] Value { get; set; }
    }

    public class KenticoTaxonomyValue
    {
        [JsonProperty(PropertyName = "name")] public string Name { get; set; }

        [JsonProperty(PropertyName = "codename")]
        public string CodeName { get; set; }
    }


    [Serializable]
    public class KenticoTextField
    {
        [JsonProperty(PropertyName = "type")] public string Type { get; set; }

        [JsonProperty(PropertyName = "name")] public string Name { get; set; }

        [JsonProperty(PropertyName = "value")] public string Value { get; set; }
    }

    [Serializable]
    public class KenticoIntField
    {
        [JsonProperty(PropertyName = "type")] public string Type { get; set; }

        [JsonProperty(PropertyName = "name")] public string Name { get; set; }

        [JsonProperty(PropertyName = "value")] public int Value { get; set; }
    }

    [Serializable]
    public class KenticoDateField
    {
        [JsonProperty(PropertyName = "type")] public string Type { get; set; }

        [JsonProperty(PropertyName = "name")] public string Name { get; set; }

        [JsonProperty(PropertyName = "value")] public DateTime? Value { get; set; }
    }


    [Serializable]
    public class KenticoImageField
    {
        [JsonProperty(PropertyName = "type")] public string Type { get; set; }

        [JsonProperty(PropertyName = "name")] public string Name { get; set; }

        [JsonProperty(PropertyName = "value")] public KenticoImageFieldValue[] Value { get; set; }
    }

    public class KenticoImageFieldValue
    {
        [JsonProperty(PropertyName = "type")] public string Type { get; set; }

        [JsonProperty(PropertyName = "name")] public string Name { get; set; }

        [JsonProperty(PropertyName = "url")] public string ImageUrl { get; set; }
    }
}