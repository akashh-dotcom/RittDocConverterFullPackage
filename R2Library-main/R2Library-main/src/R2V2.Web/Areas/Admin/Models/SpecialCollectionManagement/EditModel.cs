#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using R2V2.Core.Resource;

#endregion

namespace R2V2.Web.Areas.Admin.Models.SpecialCollectionManagement
{
    public class EditModel : AdminBaseModel
    {
        private readonly IList<IResource> _excludedResources = new List<IResource>();
        private readonly IList<IResource> _resources = new List<IResource>();
        private readonly IList<string> _resourcesNotFound = new List<string>();

        public EditModel()
        {
        }

        public EditModel(bool canBePublic)
        {
            CanBeMadePublic = canBePublic;
        }

        public int CollectionId { get; set; }

        [Display(Name = @"Name:")] public string Name { get; set; }

        [AllowHtml]
        [Display(Name = @"Description:")]
        public string Description { get; set; }

        public List<Resource.Resource> ResourceModels { get; set; }
        public string SpecialBaseIconUrl { get; set; }

        [Display(Name = @"Is Public:")] public bool IsPublic { get; set; }
        public bool CanBeMadePublic { get; set; }

        public string Isbns { get; set; }
        public IEnumerable<IResource> Resources => _resources;

        public IEnumerable<IResource> ExcludedResources => _excludedResources;

        public IEnumerable<string> ResourcesNotFound => _resourcesNotFound;

        public string ResourceString { get; set; }

        public void AddResource(IResource resource)
        {
            _resources.Add(resource);
        }

        public void AddExcludedResource(IResource resource)
        {
            _excludedResources.Add(resource);
        }

        public void AddResourceNotFound(string isbn)
        {
            _resourcesNotFound.Add(isbn);
        }

        public int[] GetResourceIds()
        {
            var resourceIds = string.IsNullOrWhiteSpace(ResourceString)
                ? new string[0]
                : ResourceString.Split(',');

            var ids = new List<int>();
            foreach (var id in resourceIds)
            {
                int resourceId;
                int.TryParse(id, out resourceId);
                ids.Add(resourceId);
            }

            return ids.ToArray();
        }
    }
}