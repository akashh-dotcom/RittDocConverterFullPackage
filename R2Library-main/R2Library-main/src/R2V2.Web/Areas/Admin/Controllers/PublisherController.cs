#region

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using R2V2.Contexts;
using R2V2.Core.Authentication;
using R2V2.Core.Publisher;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Areas.Admin.Controllers.SuperTypes;
using R2V2.Web.Areas.Admin.Models.PublisherConsolidation;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.MvcFramework.Filters.Admin;
using R2V2.Web.Infrastructure.Settings;
using Publisher = R2V2.Web.Areas.Admin.Models.PublisherConsolidation.Publisher;
using PublisherUser = R2V2.Web.Areas.Admin.Models.PublisherConsolidation.PublisherUser;

#endregion

namespace R2V2.Web.Areas.Admin.Controllers
{
    [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN })]
    public class PublisherController : R2AdminBaseController
    {
        private readonly ILog<PublisherController> _log;
        private readonly PublisherService _publisherService;
        private readonly IResourceService _resourceService;
        private readonly IWebImageSettings _webImageSettings;

        public PublisherController(
            IAuthenticationContext authenticationContext
            , ILog<PublisherController> log
            , PublisherService publisherService
            , IWebImageSettings webImageSettings
            , IResourceService resourceService
        ) : base(authenticationContext)
        {
            _log = log;
            _publisherService = publisherService;
            _webImageSettings = webImageSettings;
            _resourceService = resourceService;
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.PUBUSER })]
        public ActionResult List()
        {
            var publishersList = new PublishersList { IsRittenhouseAdmin = IsRittenhouseAdmin() };

            var cachedPublishers = _publisherService.GetAdminPublishers();

            var publishers = ConvertToPublisherModel(cachedPublishers);

            publishersList.SetPublishers(publishers);

            if (CurrentUser != null)
            {
                publishersList.DisplayAddPublisher = CurrentUser.EnablePublisherAdd > 0;
            }

            return View(publishersList);
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.PUBUSER })]
        public ActionResult Detail(int publisherId, int publisherUserId = 0)
        {
            //Need this to display errors from Edit/Add Publisher User. If not they will swallowed and not display
            //ViewDataDictionary tempViewData = TempData.GetItem<ViewDataDictionary>("ViewData");
            var tempErrors = TempData.GetItem<Dictionary<string, string>>("PublisherErrors");
            //TempData.AddItem("PublisherErrors", modelStateErrors);

            if (tempErrors != null)
            {
                foreach (var x in tempErrors)
                {
                    ModelState.AddModelError(x.Key, x.Value);
                }
            }

            var model = GetPublisherDetail(publisherId, publisherUserId);

            if (model == null)
            {
                return RedirectToAction("List");
            }

            return View(model);
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.PUBUSER })]
        [HttpPost]
        public ActionResult Detail(Publisher editPublisher, int consolidatedPublisherId)
        {
            if (consolidatedPublisherId > 0)
            {
                var publisher = _publisherService.GetPublisherForAdmin(editPublisher.Id);
                publisher.ConsolidatedPublisher = new Core.Publisher.Publisher { Id = consolidatedPublisherId };

                _publisherService.SaveUpdatePublisher(publisher);
                var resources = _resourceService.GetAllResources(true);

                var resourcesToTransform =
                    resources.Where(x =>
                        (x.StatusId == (int)ResourceStatus.Active || x.StatusId == (int)ResourceStatus.Archived) &&
                        (x.Publisher.Id == editPublisher.Id ||
                         (x.Publisher.ConsolidatedPublisher != null &&
                          x.Publisher.ConsolidatedPublisher.Id == consolidatedPublisherId)));

                _resourceService.AddToTransformQueue(resourcesToTransform);

                return RedirectToAction("List");
            }

            return Detail(editPublisher.Id);
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.PUBUSER })]
        public ActionResult DeleteConsolidation(int publisherId, int parentPublisherId)
        {
            if (publisherId > 0)
            {
                var publisher = _publisherService.GetPublisherForAdmin(publisherId);
                if (publisher != null)
                {
                    _publisherService.DeletePublisherConsolidation(publisherId);
                    var resources = _resourceService.GetAllResources(true);

                    var resourcesToTransform =
                        resources.Where(x =>
                            (x.StatusId == (int)ResourceStatus.Active || x.StatusId == (int)ResourceStatus.Archived) &&
                            (x.Publisher.Id == publisherId ||
                             (x.Publisher.ConsolidatedPublisher != null &&
                              x.Publisher.ConsolidatedPublisher.Id == parentPublisherId)));

                    _resourceService.AddToTransformQueue(resourcesToTransform);
                }
            }

            return RedirectToAction("Detail", new { publisherId = parentPublisherId });
        }

        public ActionResult Add()
        {
            var user = CurrentUser;
            if (user.EnablePublisherAdd == 0)
            {
                return RedirectToAction("List");
            }

            return View(new Publisher());
        }

        [HttpPost]
        public ActionResult Add(Publisher publisher)
        {
            var user = CurrentUser;
            if (user.EnablePublisherAdd == 0)
            {
                return RedirectToAction("List");
            }

            var newPublisher = new Core.Publisher.Publisher
            {
                Name = publisher.Name,
                City = publisher.City,
                State = publisher.State,
                RecordStatus = true
            };

            var savedPublisher = _publisherService.AddPublisher(newPublisher);

            if (savedPublisher.Id > 0)
            {
                return RedirectToAction("Detail", new { publisherId = savedPublisher.Id });
            }

            ModelState.AddModelError("Name",
                @"Error saving new publisher. Please check to make sure all fields are filled in properly.");

            return View(publisher);
        }

        public ActionResult Delete(int publisherId)
        {
            var user = CurrentUser;
            if (user.EnablePublisherAdd == 0)
            {
                return RedirectToAction("List");
            }

            var publisher = _publisherService.GetPublisherForAdmin(publisherId);
            if (publisher != null)
            {
                var noError = _publisherService.DeletePublisher(publisher);

                if (!noError)
                {
                    var modelStateErrors = new Dictionary<string, string>
                    {
                        {
                            "EditPublisher.Name",
                            "Error deleting publisher. The publisher cannot have any consolidated publishers or resources."
                        }
                    };

                    TempData.AddItem("PublisherErrors", modelStateErrors);
                    return RedirectToAction("Detail", new { publisherId });
                }
            }

            return RedirectToAction("List");
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.PUBUSER })]
        public ActionResult Edit(int publisherId)
        {
            var corePublisher = _publisherService.GetPublisher(publisherId);
            var publisher = ConvertToPublisherModel(corePublisher);

            publisher.ImageUrl = $"{_webImageSettings.PublisherImageUrl}{publisher.ImageFileName}";

            return View(publisher);
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.PUBUSER })]
        [HttpPost]
        public ActionResult Edit(Publisher publisher, HttpPostedFileBase file)
        {
            var corePublisher = _publisherService.GetPublisherForAdmin(publisher.Id);

            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);

                _log.InfoFormat(">>>>> Publisher Image Upload Logging");
                _log.InfoFormat("File Name: '{0}'", fileName);
                var newFileName = $"{publisher.Name.Replace(" ", "")}.jpg";
                _log.InfoFormat("newFileName: '{0}'", newFileName);

                var directoryInfo = new DirectoryInfo(_webImageSettings.PublisherImageDirectory);
                _log.DebugFormat("_webImageSettings.PublisherImageDirectory: {0}",
                    _webImageSettings.PublisherImageDirectory);
                _log.DebugFormat("directoryInfo.Exists: {0}", directoryInfo.Exists);

                if (fileName != null)
                {
                    var path = Path.Combine($"{_webImageSettings.PublisherImageDirectory}", newFileName);
                    _log.DebugFormat("path: {0}", path);
                    if (System.IO.File.Exists(path))
                    {
                        _log.Debug("Path Already Exists Deleting Old");
                        System.IO.File.Delete(path);
                    }

                    file.SaveAs(path);
                    _log.DebugFormat("File Saved, {0}", path);
                    using (var imageFile = Image.FromFile(path))
                    {
                        if (imageFile.Width > 210 || imageFile.Height > 100)
                        {
                            _log.DebugFormat("File's Width: {0} || Height: {1}", imageFile.Width, imageFile.Height);
                            ModelState.AddModelError("ImageUrl",
                                @"Your image file is not within the specific size restrictions. Please resize your image and upload again.");
                            imageFile.Dispose();

                            return View(publisher);
                        }
                    }
                    //imageFile.Dispose();
                }

                corePublisher.ImageFileName = newFileName;
                _log.InfoFormat("<<<<< Institution Branding Logging");
            }

            var publisherNameChanged = corePublisher.Name != publisher.Name;

            corePublisher.Name = publisher.Name;
            corePublisher.City = publisher.City;
            corePublisher.State = publisher.State;
            corePublisher.IsFeaturedPublisher = publisher.IsFeaturedPublisher;
            corePublisher.DisplayName = publisher.DisplayName;
            corePublisher.Description = publisher.Description;
            corePublisher.ProductDescription = publisher.ProductStatement;
            corePublisher.VendorNumber = publisher.VendorNumber;

            //SAVE
            _publisherService.SaveUpdatePublisher(corePublisher);

            //Gets the current Featured Publisher
            var lastFeaturedPublisher = _publisherService.GetFeaturedPublisher();

            //if the corePublisher is featured and is not the current, need to remove featured from the current.
            if (lastFeaturedPublisher != null)
            {
                if (corePublisher.IsFeaturedPublisher && corePublisher.Id != lastFeaturedPublisher.Id)
                {
                    var coreLastFeaturedPublisher = _publisherService.GetPublisherForAdmin(lastFeaturedPublisher.Id);
                    coreLastFeaturedPublisher.IsFeaturedPublisher = false;
                    _publisherService.SaveUpdatePublisher(coreLastFeaturedPublisher);
                }
            }

            if (publisherNameChanged)
            {
                var resources = _resourceService.GetAllResources(true);

                var resourcesToTransform =
                    resources.Where(x =>
                        (x.StatusId == (int)ResourceStatus.Active || x.StatusId == (int)ResourceStatus.Archived) &&
                        (x.Publisher.Id == corePublisher.Id ||
                         (x.Publisher.ConsolidatedPublisher != null &&
                          x.Publisher.ConsolidatedPublisher.Id == corePublisher.Id)));

                _resourceService.AddToTransformQueue(resourcesToTransform);
            }

            return RedirectToAction("Detail", new { publisherId = publisher.Id });
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.PUBUSER })]
        public ActionResult AddPublisherUser(int publisherId)
        {
            return RedirectToAction("Detail", new { publisherId, publisherUserId = -1 });
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.PUBUSER })]
        public ActionResult EditPublisherUser(int publisherId, int publisherUserId)
        {
            return RedirectToAction("Detail", new { publisherId, publisherUserId });
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.PUBUSER })]
        [HttpPost]
        public ActionResult SavePublisherUser(PublisherUser editPublisherUser, int publisherId)
        {
            var error = false;
            editPublisherUser.PublisherId = publisherId;

            var userNameRegex = new Regex(@"[,]");

            if (userNameRegex.IsMatch(editPublisherUser.UserName))
            {
                ModelState.AddModelError("EditPublisherUser.UserName", Resources.UserNameCommas);
                error = true;
            }

            if (((!string.IsNullOrWhiteSpace(editPublisherUser.NewPassword) &&
                  !string.IsNullOrWhiteSpace(editPublisherUser.ConfirmPassword)) || editPublisherUser.Id < 1) && !error)
            {
                if (editPublisherUser.NewPassword == editPublisherUser.ConfirmPassword)
                {
                    editPublisherUser.Password = editPublisherUser.NewPassword;
                }
                else
                {
                    ModelState.AddModelError("EditPublisherUser.NewPassword", Resources.PasswordsDoNotMatch);
                    error = true;
                    //return RedirectToAction("Detail", new { publisherId, publisherUserId = editPublisherUser.Id });
                }

                if (string.IsNullOrWhiteSpace(editPublisherUser.NewPassword) && !error)
                {
                    ModelState.AddModelError("EditPublisherUser.NewPassword", Resources.PleaseFillInPassword);
                    error = true;
                    //return RedirectToAction("Detail", new { publisherId, publisherUserId = editPublisherUser.Id });
                }

                if (!error && (editPublisherUser.NewPassword.Length < 4 || editPublisherUser.NewPassword.Length > 20))
                {
                    ModelState.AddModelError("EditPublisherUser.NewPassword", Resources.PasswordMinMaxLength);
                    error = true;
                }
            }

            if (error)
            {
                var modelStateErrors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(a => a.Key, a => a.Value.Errors[0].ErrorMessage);

                TempData.AddItem("PublisherErrors", modelStateErrors);
                return RedirectToAction("Detail", new { publisherId, publisherUserId = editPublisherUser.Id });
            }

            var publisherUser = ConvertToCorePublisherUser(editPublisherUser);

            _publisherService.SavePublisherUser(publisherUser);

            return RedirectToAction("Detail", new { publisherId });
        }

        [AdminAuthorizationFilter(Roles = new[] { RoleCode.RITADMIN, RoleCode.PUBUSER })]
        public ActionResult DeletePublisherUser(int publisherId, int publisherUserId)
        {
            var publisherUser = _publisherService.GetConsolidatedPublisherUser(publisherUserId);

            _publisherService.DeletePublisherUser(publisherUser);

            return RedirectToAction("Detail", new { publisherId });
        }

        private List<PublisherUser> GetPublisherUsers(int[] ids)
        {
            var corePublisherUsers = _publisherService.GetConsolidatedPublisherUsers(ids);

            return corePublisherUsers.Select(corePublisherUser => new PublisherUser
            {
                UserName = corePublisherUser.UserName,
                Password = corePublisherUser.Password,
                RecordStatus = corePublisherUser.RecordStatus,
                RecordStatusValue = corePublisherUser.RecordStatus ? "Active" : "Inactive",
                Id = corePublisherUser.Id,
                PublisherId = corePublisherUser.Publisher.Id
            }).ToList();
        }

        private IList<Publisher> ConvertToPublisherModel(IEnumerable<IPublisher> publishers)
        {
            return publishers.Select(ConvertToPublisherModel).ToList();
        }


        private Publisher ConvertToPublisherModel(IPublisher publisher)
        {
            if (publisher.Id == 77)
            {
                var test = 1;
            }

            var publisherModel = new Publisher
            {
                Id = publisher.Id,
                Name = publisher.Name,
                City = publisher.City,
                State = publisher.State,
                CityAndState = string.IsNullOrWhiteSpace(publisher.State)
                    ? publisher.City
                    : $"{publisher.City}, {publisher.State}",
                RecordStatus = publisher.RecordStatus ? "Active" : "Inactive",
                IsFeaturedPublisher = publisher.IsFeaturedPublisher,
                ImageFileName = publisher.ImageFileName,
                DisplayName = publisher.DisplayName,
                Description = publisher.Description,
                ResourceCount = publisher.ResourceCount + publisher.ChildrenResourceCount,
                ProductStatement = publisher.ProductDescription,
                NotSaleableDate = publisher.NotSaleableDate,
                VendorNumber = publisher.VendorNumber
            };

            if (publisher.ConsolidatedPublisher != null)
            {
                publisherModel.ConsolidatedPublisher = new Publisher
                {
                    Id = publisher.ConsolidatedPublisher.Id,
                    Name = publisher.ConsolidatedPublisher.Name,
                    City = publisher.ConsolidatedPublisher.City,
                    State = publisher.ConsolidatedPublisher.State,
                    CityAndState = string.IsNullOrWhiteSpace(publisher.ConsolidatedPublisher.State)
                        ? publisher.ConsolidatedPublisher.City
                        : string.Format("{0}, {1}", publisher.ConsolidatedPublisher.City,
                            publisher.ConsolidatedPublisher.State),
                    RecordStatus = publisher.ConsolidatedPublisher.RecordStatus ? "Active" : "Inactive",
                    IsFeaturedPublisher = publisher.IsFeaturedPublisher,
                    ImageFileName = publisher.ImageFileName,
                    DisplayName = publisher.DisplayName,
                    Description = publisher.Description,
                    ProductStatement = publisher.ProductDescription,
                    NotSaleableDate = publisher.NotSaleableDate,
                    VendorNumber = publisher.ConsolidatedPublisher.VendorNumber
                };
            }

            return publisherModel;
        }

        private Core.Authentication.PublisherUser ConvertToCorePublisherUser(PublisherUser publisherUser)
        {
            var pubUser = _publisherService.GetConsolidatedPublisherUser(publisherUser.Id) ??
                          new Core.Authentication.PublisherUser
                          {
                              Publisher = new Core.Publisher.Publisher { Id = publisherUser.PublisherId },
                              Role = new Role { Id = (int)RoleCode.PUBUSER }
                          };

            if (!string.IsNullOrWhiteSpace(publisherUser.Password))
            {
                pubUser.Password = publisherUser.Password;
            }

            pubUser.UserName = publisherUser.UserName;

            pubUser.RecordStatus = publisherUser.RecordStatus;

            return pubUser;
        }

        private void PopulateResourceCount(IEnumerable<Publisher> publishers)
        {
            foreach (var publisher in publishers)
            {
                IPublisher cachedPublisher = _publisherService.GetPublisherForAdmin(publisher.Id);
                if (cachedPublisher != null)
                {
                    publisher.ResourceCount = cachedPublisher.ResourceCount;
                }
            }
        }

        private void PopulateConsolidatedResourceCount(IEnumerable<Publisher> publishers)
        {
            foreach (var publisher in publishers)
            {
                var cachedPublisher = _publisherService.GetPublisher(publisher.Id);
                publisher.ResourceCount = cachedPublisher.ResourceCount + cachedPublisher.ChildrenResourceCount;
            }
        }

        private PublisherDetail GetPublisherDetail(int publisherId, int publisherUserId = 0)
        {
            var nonConsolidatedPublishers = _publisherService.GetNonConsolidatedPublishers();
            var publishers = ConvertToPublisherModel(nonConsolidatedPublishers);

            PopulateResourceCount(publishers);

            var editPublisher = publishers.FirstOrDefault(x => x.Id == publisherId);

            publishers.Remove(editPublisher);

            if (editPublisher == null)
            {
                return null;
            }

            var childPublishersFromCache = _publisherService.GetChildPublishers(publisherId);
            var childPublishers = ConvertToPublisherModel(childPublishersFromCache);

            PopulateResourceCount(childPublishers);

            //Gets all the child PublisherUsers
            var ids = new List<int> { publisherId };
            ids.AddRange(childPublishers.Select(childPublisher => childPublisher.Id));

            var publisherUsers = GetPublisherUsers(ids.ToArray());

            //Used for the dropdownlist
            var activePublishers = publishers.Where(x => x.RecordStatus == "Active").ToList();

            PopulateConsolidatedResourceCount(activePublishers);

            PublisherUser editPubUser = null;
            if (publisherUserId > 0)
            {
                editPubUser = publisherUsers.First(x => x.Id == publisherUserId);
            }
            else if (publisherUserId == -1)
            {
                editPubUser = new PublisherUser { PublisherId = publisherId, Id = -1 };
            }

            var model = new PublisherDetail(editPublisher, activePublishers, childPublishers, publisherUsers,
                editPubUser);

            if (CurrentUser != null)
            {
                model.DisplayPublisherDelete = CurrentUser.EnablePublisherAdd > 0;
            }

            return model;
        }

        public ActionResult NotSaleable(int publisherId)
        {
            var model = GetPublisherDetail(publisherId);
            return View(model);
        }

        [HttpPost]
        public ActionResult NotSaleableConfirm(int publisherId)
        {
            var model = GetPublisherDetail(publisherId);

            var childPublisherIds = model.ChildPublishers.Select(x => x.Id).ToList();
            var publisherIds = new List<int> { publisherId };
            publisherIds.AddRange(childPublisherIds);

            _publisherService.MarkPublisherNotSaleable(publisherIds.Distinct().ToArray(), CurrentUser);

            model = GetPublisherDetail(publisherId);

            return View("NotSaleable", model);
        }
    }
}