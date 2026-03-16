#region

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.R2Utilities;
using R2V2.Core.Resource;
using R2V2.Core.Resource.Collection;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models.SpecialCollectionManagement;
using R2V2.Web.Infrastructure.Settings;
using Resource = R2V2.Web.Areas.Admin.Models.Resource.Resource;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    public class SpecialCollectionManagementController : R2AdminBaseController
    {
        private readonly ICollectionService _collectionService;
        private readonly ResourceService _resourceService;
        private readonly IWebImageSettings _webImageSettings;

        public SpecialCollectionManagementController(
            IAuthenticationContext authenticationContext
            , ICollectionService collectionService
            , ResourceService resourceService
            , IWebImageSettings webImageSettings
        ) : base(authenticationContext)
        {
            _collectionService = collectionService;
            _resourceService = resourceService;
            _webImageSettings = webImageSettings;
        }

        public ActionResult List()
        {
            var model = GetCollectionListModel(false);

            return View(model);
        }

        private ListModel GetCollectionListModel(bool isEditMode)
        {
            var specialCollectionLists = _collectionService.GetCollectionLists();

            var model = new ListModel { SpecialCollectionLists = new List<SpecialCollectionList>() };

            foreach (var specialCollectionList in specialCollectionLists)
            {
                var resourceCount = _resourceService.GetAllResources()
                    .Count(x => x.CollectionIdsToArray().Contains(specialCollectionList.Id));
                model.SpecialCollectionLists.Add(new SpecialCollectionList
                {
                    Id = specialCollectionList.Id, Name = specialCollectionList.Name,
                    Sequence = specialCollectionList.SpecialCollectionSequence, ResourceCount = resourceCount,
                    IsPublic = specialCollectionList.IsPublic
                });
            }

            model.IsEditMode = isEditMode;
            return model;
        }

        public ActionResult Add()
        {
            return View(new EditModel());
            var t = _collectionService.GetPublicCollection();
            return View(new EditModel(t == null));
        }

        [HttpPost]
        public ActionResult Add(EditModel model)
        {
            if (ModelState.IsValid)
            {
                var collectionId = _collectionService.AddCollection(model.Name);
                return RedirectToAction("Edit", new { collectionId });
            }

            return View(model);
        }

        public ActionResult Edit(int collectionId)
        {
            if (collectionId > 0)
            {
                var collection = _collectionService.GetCollectionById(collectionId);
                var publicCollection = _collectionService.GetPublicCollection();
                var resources = _resourceService.GetAllResources()
                    .Where(x => x.CollectionIdsToArray().Contains(collectionId));
                var model = new EditModel
                {
                    CollectionId = collection.Id,
                    Name = collection.Name,
                    Description = collection.Description,
                    ResourceModels = resources.Select(x => new Resource(x)).ToList(),
                    SpecialBaseIconUrl = _webImageSettings.SpecialIconBaseUrl,
                    CanBeMadePublic = publicCollection == null,
                    IsPublic = collection.IsPublic
                };
                return View(model);
            }

            return RedirectToAction("List");
        }

        [HttpPost]
        public ActionResult Edit(EditModel editModel)
        {
            if (editModel.CollectionId > 0)
            {
                ICollection collection = _collectionService.GetCollection(editModel.CollectionId);
                collection.Name = editModel.Name;
                collection.Description = editModel.Description;
                collection.IsPublic = editModel.IsPublic;

                _collectionService.UpdateCollection(collection);
            }

            return RedirectToAction("List");
        }

        public ActionResult Delete(int collectionId)
        {
            if (collectionId > 0)
            {
                var collection = _collectionService.GetCollection(collectionId);
                var resources = _resourceService.GetAllResources()
                    .Where(x => x.CollectionIdsToArray().Contains(collectionId));
                var model = new DeleteModel();
                model.Name = collection.Name;
                model.ResourceCount = resources.Count();
                model.CollectionId = collection.Id;
                return View(model);
            }

            return RedirectToAction("List");
        }

        [HttpPost]
        public ActionResult Delete(DeleteModel model)
        {
            if (model.CollectionId > 0)
            {
                var collection = _collectionService.GetCollection(model.CollectionId);
                _collectionService.DeleteSpecialCollection(model.CollectionId);
                model.Name = collection.Name;
                model.CollectionId = 0;
                return View(model);
            }

            return RedirectToAction("List");
        }

        private string GetSequenceString(IEnumerable<SpecialCollectionList> specialCollectionLists)
        {
            var sequences = specialCollectionLists.Select(x => x.Id).ToList();
            var sb = new StringBuilder();
            for (var i = 0; i < sequences.Count(); i++)
            {
                if (i == 0)
                {
                    sb.Append(sequences[i]);
                }
                else
                {
                    sb.AppendFormat(",{0}", sequences[i]);
                }
            }

            return sb.ToString();
        }

        private void OrderCollectionListBySequence(List<SpecialCollectionList> specialCollectionLists,
            string sequenceString)
        {
            var sequence = sequenceString.Split(',').Select(int.Parse).ToList();

            for (var i = 0; i < sequence.Count(); i++)
            {
                var specialCollectionList = specialCollectionLists.FirstOrDefault(x => x.Id == sequence[i]);
                if (specialCollectionList != null)
                {
                    specialCollectionList.Sequence = i + 1;
                }
            }
        }

        public ActionResult EditSequence()
        {
            var model = GetCollectionListModel(true);
            model.SequenceString = GetSequenceString(model.SpecialCollectionLists);
            return View("List", model);
        }

        public ActionResult SequenceMoveUp(int collectionId, string sequenceString)
        {
            var model = GetCollectionListModel(true);

            OrderCollectionListBySequence(model.SpecialCollectionLists, sequenceString);

            var collectionToMoveUp = model.SpecialCollectionLists.FirstOrDefault(x => x.Id == collectionId);

            var collectionSequence = collectionToMoveUp != null ? collectionToMoveUp.Sequence : 0;

            foreach (var specialCollectionList in model.SpecialCollectionLists)
            {
                if (specialCollectionList.Sequence == collectionSequence - 1 &&
                    specialCollectionList.Id != collectionId)
                {
                    specialCollectionList.Sequence++;
                }

                if (specialCollectionList.Id == collectionId)
                {
                    specialCollectionList.Sequence--;
                }
            }

            model.SpecialCollectionLists = model.SpecialCollectionLists.OrderBy(x => x.Sequence).ToList();

            model.SequenceString = GetSequenceString(model.SpecialCollectionLists);

            return View("List", model);
        }

        public ActionResult SequenceMoveDown(int collectionId, string sequenceString)
        {
            var model = GetCollectionListModel(true);

            OrderCollectionListBySequence(model.SpecialCollectionLists, sequenceString);

            var collectionToMoveDown = model.SpecialCollectionLists.FirstOrDefault(x => x.Id == collectionId);

            var collectionSequence = collectionToMoveDown != null ? collectionToMoveDown.Sequence : 0;

            foreach (var specialCollectionList in model.SpecialCollectionLists)
            {
                if (specialCollectionList.Sequence == collectionSequence + 1 &&
                    specialCollectionList.Id != collectionId)
                {
                    specialCollectionList.Sequence--;
                }

                if (specialCollectionList.Id == collectionId)
                {
                    specialCollectionList.Sequence++;
                }
            }

            model.SpecialCollectionLists = model.SpecialCollectionLists.OrderBy(x => x.Sequence).ToList();

            model.SequenceString = GetSequenceString(model.SpecialCollectionLists);

            return View("List", model);
        }


        public ActionResult SaveSequence(string sequenceString)
        {
            if (!string.IsNullOrWhiteSpace(sequenceString))
            {
                var sequence = sequenceString.Split(',').Select(int.Parse).ToArray();
                _collectionService.SaveCollectionListSequence(sequence);
            }

            return RedirectToAction("List");
        }

        public ActionResult RemoveResource(int collectionId, int resourceId)
        {
            if (collectionId > 0 && resourceId > 0)
            {
                _collectionService.RemoveResourceFromCollection(collectionId, resourceId);
            }

            return RedirectToAction("Edit", new { collectionId });
        }

        public ActionResult BulkAddResources(int collectionId)
        {
            return View(new EditModel { CollectionId = collectionId });
        }

        [HttpPost]
        public ActionResult BulkAddResources(EditModel model)
        {
            var resourceIds = model.GetResourceIds();

            _collectionService.BulkAddResourcesToSpecialCollection(model.CollectionId, resourceIds);

            foreach (var resourceId in resourceIds)
            {
                var resource = _resourceService.GetResource(resourceId);
                model.AddResource(resource);
            }

            return View(model);
        }

        public ActionResult BulkAddResourcesVerify(EditModel model)
        {
            var lastIsbn = "";
            foreach (var isbn in IsbnUtilities.GetDelimitedIsbns(model.Isbns).Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var resource = _resourceService.GetResource(isbn);
                if (resource == null)
                {
                    model.AddResourceNotFound(isbn);
                }
                else if (resource.NotSaleable ||
                         (resource.StatusId != (int)ResourceStatus.Forthcoming &&
                          resource.StatusId != (int)ResourceStatus.Active))
                {
                    model.AddExcludedResource(resource);
                }
                else
                {
                    model.AddResource(resource);
                }
            }

            if (model.Resources != null && model.Resources.Any())
            {
                var sb = new StringBuilder();
                foreach (var resource in model.Resources)
                {
                    sb.AppendFormat("{0},", resource.Id);
                }

                model.ResourceString = sb.ToString(0, sb.Length - 1);
            }

            return View(model);
        }
    }
}