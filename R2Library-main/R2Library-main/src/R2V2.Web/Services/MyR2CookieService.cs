#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using R2V2.Core.MyR2;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;
using R2V2.Web.Models.MyR2;
using UserContentFolder = R2V2.Web.Models.MyR2.UserContentFolder;
using UserContentItem = R2V2.Web.Models.MyR2.UserContentItem;

#endregion

namespace R2V2.Web.Services
{
    public class MyR2CookieService
    {
        private const string MyR2DataCookieName = "R2V2.MyR2.Data";
        private readonly ILog<MyR2CookieService> _log;
        private readonly IQueryable<MyR2Data> _myR2Data;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public MyR2CookieService(
            ILog<MyR2CookieService> log
            , IQueryable<MyR2Data> myR2Data
            , IUnitOfWorkProvider unitOfWorkProvider
        )
        {
            _log = log;
            _myR2Data = myR2Data;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        public void CreateUserContentFolder(UserContentFolder folder, UserContentType type, int institutionId)
        {
            try
            {
                switch (type)
                {
                    case UserContentType.Bookmark:
                        SetupMyR2Folder(folder, MyR2Type.Bookmark, institutionId, false);
                        break;
                    case UserContentType.CourseLink:
                        SetupMyR2Folder(folder, MyR2Type.CourseLink, institutionId, false);
                        break;
                    case UserContentType.Image:
                        SetupMyR2Folder(folder, MyR2Type.Image, institutionId, false);
                        break;
                    case UserContentType.Reference:
                        SetupMyR2Folder(folder, MyR2Type.Reference, institutionId, false);
                        break;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        public void RenameUserContentFolder(UserContentType type, string oldName, string newName, int institutionId)
        {
            //AddMyR2Data
            try
            {
                switch (type)
                {
                    case UserContentType.Bookmark:
                        RenameMyR2Folder(MyR2Type.Bookmark, oldName, newName, institutionId);
                        break;
                    case UserContentType.CourseLink:
                        RenameMyR2Folder(MyR2Type.CourseLink, oldName, newName, institutionId);
                        break;
                    case UserContentType.Image:
                        RenameMyR2Folder(MyR2Type.Image, oldName, newName, institutionId);
                        break;
                    case UserContentType.Reference:
                        RenameMyR2Folder(MyR2Type.Reference, oldName, newName, institutionId);
                        break;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        public void DeleteUserContentFolder(UserContentFolder folder, UserContentType type, int institutionId)
        {
            try
            {
                switch (type)
                {
                    case UserContentType.Bookmark:
                        SetupMyR2Folder(folder, MyR2Type.Bookmark, institutionId, true);
                        break;
                    case UserContentType.CourseLink:
                        SetupMyR2Folder(folder, MyR2Type.CourseLink, institutionId, true);
                        break;
                    case UserContentType.Image:
                        SetupMyR2Folder(folder, MyR2Type.Image, institutionId, true);
                        break;
                    case UserContentType.Reference:
                        SetupMyR2Folder(folder, MyR2Type.Reference, institutionId, true);
                        break;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        public void SaveUserContentItem(UserContentType type, UserContentItem userContentItem, UserContentFolder folder,
            int institutionId)
        {
            try
            {
                switch (type)
                {
                    case UserContentType.Bookmark:
                        SaveMyR2Data(MyR2Type.Bookmark, userContentItem, folder.FolderName, folder.DefaultFolder,
                            institutionId);
                        break;
                    case UserContentType.CourseLink:
                        SaveMyR2Data(MyR2Type.CourseLink, userContentItem, folder.FolderName, folder.DefaultFolder,
                            institutionId);
                        break;
                    case UserContentType.Image:
                        SaveMyR2Data(MyR2Type.Image, userContentItem, folder.FolderName, folder.DefaultFolder,
                            institutionId);
                        break;
                    case UserContentType.Reference:
                        SaveMyR2Data(MyR2Type.Reference, userContentItem, folder.FolderName, folder.DefaultFolder,
                            institutionId);
                        break;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        public void DeleteUserContentItem(UserContentType type, int id, int institutionId)
        {
            try
            {
                switch (type)
                {
                    case UserContentType.Bookmark:
                        DeleteMyR2Data(MyR2Type.Bookmark, id, institutionId);
                        break;
                    case UserContentType.CourseLink:
                        DeleteMyR2Data(MyR2Type.CourseLink, id, institutionId);
                        break;
                    case UserContentType.Image:
                        DeleteMyR2Data(MyR2Type.Image, id, institutionId);
                        break;
                    case UserContentType.Reference:
                        DeleteMyR2Data(MyR2Type.Reference, id, institutionId);
                        break;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        public List<UserContentFolder> GetUserContentFolders(UserContentType type, int institutionId)
        {
            try
            {
                switch (type)
                {
                    case UserContentType.Bookmark:
                        return GetMyR2Folders(MyR2Type.Bookmark, institutionId);
                    case UserContentType.CourseLink:
                        return GetMyR2Folders(MyR2Type.CourseLink, institutionId);
                    case UserContentType.Image:
                        return GetMyR2Folders(MyR2Type.Image, institutionId);
                    default:
                    case UserContentType.Reference:
                        return GetMyR2Folders(MyR2Type.Reference, institutionId);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return new List<UserContentFolder>();
        }

        public List<SavedSearch> GetMyR2Searches(bool isSearchHistory, int institutionId)
        {
            try
            {
                return GetMyR2Searches(isSearchHistory ? MyR2Type.SearchHistory : MyR2Type.SavedSearch, institutionId);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }

            return new List<SavedSearch>();
        }

        public void SaveMyR2Search(SavedSearch search, bool isSearchHistory, int institutionId)
        {
            try
            {
                SaveMyR2Search(search, isSearchHistory ? MyR2Type.SearchHistory : MyR2Type.SavedSearch, institutionId,
                    false);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        public void DeleteMyR2Search(SavedSearch search, bool isSearchHistory, int institutionId)
        {
            try
            {
                SaveMyR2Search(search, isSearchHistory ? MyR2Type.SearchHistory : MyR2Type.SavedSearch, institutionId,
                    true);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }


        private void SetupMyR2Folder(UserContentFolder folder, MyR2Type type, int institutionId, bool isDelete)
        {
            var response = HttpContext.Current.Response;
            var request = HttpContext.Current.Request;
            try
            {
                var guid = GetNewGuid();
                if (request.Cookies[MyR2DataCookieName] != null)
                {
                    guid = request.Cookies[MyR2DataCookieName].Value;
                }

                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        var myR2Datas = _myR2Data.Where(x =>
                            x.GuidCookieValue == guid && x.Type == (int)type && x.FolderName == folder.FolderName &&
                            x.InstitutionId == institutionId).ToList();
                        if (!isDelete)
                        {
                            var myR2Data = new MyR2Data
                            {
                                FolderName = folder.FolderName,
                                DefaultFolder = false,
                                GuidCookieValue = guid,
                                Type = (int)type,
                                InstitutionId = institutionId
                            };
                            uow.Save(myR2Data);
                        }
                        else
                        {
                            foreach (var myR2Data in myR2Datas)
                            {
                                uow.Delete(myR2Data);
                            }
                        }

                        uow.Commit();
                        transaction.Commit();
                    }
                }

                var cookie = new HttpCookie(MyR2DataCookieName, guid)
                {
                    Expires = DateTime.Now.AddMonths(3)
                };
                response.Cookies.Add(cookie);
            }
            catch (Exception ex)
            {
                var errormessage = new StringBuilder();
                if (request.Cookies[MyR2DataCookieName] != null)
                {
                    errormessage.AppendLine(request.Cookies[MyR2DataCookieName].Value);
                }

                errormessage.Append(ex.Message);
                _log.Warn(errormessage, ex);
            }
        }

        private void RenameMyR2Folder(MyR2Type type, string oldName, string newName, int institutionId)
        {
            var request = HttpContext.Current.Request;
            var response = HttpContext.Current.Response;
            string guid = null;
            if (request.Cookies[MyR2DataCookieName] != null)
            {
                guid = request.Cookies[MyR2DataCookieName].Value;
            }

            if (guid == null)
            {
                return;
            }

            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var myR2Datas = _myR2Data.Where(x =>
                        x.GuidCookieValue == guid && x.Type == (int)type && x.FolderName == oldName &&
                        x.InstitutionId == institutionId).ToList();
                    foreach (var myR2Data in myR2Datas)
                    {
                        myR2Data.FolderName = newName;
                        uow.Update(myR2Data);
                    }

                    uow.Commit();
                    transaction.Commit();
                }
            }

            var cookie = new HttpCookie(MyR2DataCookieName, guid)
            {
                Expires = DateTime.Now.AddMonths(3)
            };
            response.Cookies.Add(cookie);
        }

        private List<UserContentFolder> GetMyR2Folders(MyR2Type type, int institutionId)
        {
            var request = HttpContext.Current.Request;
            var folders = new List<UserContentFolder>();
            folders.Add(new UserContentFolder
                { DefaultFolder = true, FolderName = GetDefaultFolderName(type), Id = 1 });
            try
            {
                if (request.Cookies[MyR2DataCookieName] != null)
                {
                    var myR2Datas = _myR2Data.Where(x =>
                        x.GuidCookieValue == request.Cookies[MyR2DataCookieName].Value && x.Type == (int)type &&
                        x.InstitutionId == institutionId).ToList();
                    foreach (var myR2Data in myR2Datas)
                    {
                        if (myR2Data.Json != null)
                        {
                            var userContentItem = JsonConvert.DeserializeObject<UserContentItem>(myR2Data.Json);
                            userContentItem.Id = myR2Data.Id;
                            var folder = folders.FirstOrDefault(x => x.FolderName == myR2Data.FolderName);
                            if (folder == null)
                            {
                                folder = new UserContentFolder
                                {
                                    FolderName = myR2Data.FolderName,
                                    DefaultFolder = myR2Data.DefaultFolder,
                                    Id = userContentItem.FolderId
                                };
                                folder.AddUserContentItem(userContentItem);
                                folders.Add(folder);
                            }
                            else
                            {
                                folder.AddUserContentItem(userContentItem);
                            }
                        }
                        else
                        {
                            var folder = new UserContentFolder
                            {
                                FolderName = myR2Data.FolderName,
                                DefaultFolder = myR2Data.DefaultFolder,
                                Id = myR2Data.Id
                            };
                            folders.Add(folder);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errormessage = new StringBuilder();
                errormessage.AppendLine(request.Cookies[MyR2DataCookieName].Value);
                errormessage.Append(ex.Message);
                _log.Warn(errormessage, ex);
            }

            return folders;
        }

        private List<SavedSearch> GetMyR2Searches(MyR2Type type, int institutionId)
        {
            var response = HttpContext.Current.Response;
            var request = HttpContext.Current.Request;
            var savedSearches = new List<SavedSearch>();
            try
            {
                if (request.Cookies[MyR2DataCookieName] != null)
                {
                    var myR2Datas = _myR2Data
                        .Where(x => x.GuidCookieValue == request.Cookies[MyR2DataCookieName].Value &&
                                    x.Type == (int)type && x.InstitutionId == institutionId)
                        .OrderByDescending(x => x.CreationDate).Take(20).ToList();
                    foreach (var myR2Data in myR2Datas)
                    {
                        var savedSearch = JsonConvert.DeserializeObject<SavedSearch>(myR2Data.Json);
                        savedSearch.Id = myR2Data.Id;
                        savedSearches.Add(savedSearch);
                    }

                    var cookie = new HttpCookie(MyR2DataCookieName, request.Cookies[MyR2DataCookieName].Value)
                    {
                        Expires = DateTime.Now.AddMonths(3)
                    };
                    response.Cookies.Add(cookie);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine(request.Cookies[MyR2DataCookieName].Value);
                errorMessage.Append(ex.Message);
                _log.Warn(errorMessage, ex);
            }

            return savedSearches;
        }

        private void SaveMyR2Data(MyR2Type type, UserContentItem userContentItem, string folderName,
            bool isDefaultFolder, int institutionId)
        {
            var response = HttpContext.Current.Response;
            var request = HttpContext.Current.Request;
            var guid = GetNewGuid();
            if (request.Cookies[MyR2DataCookieName] != null)
            {
                guid = request.Cookies[MyR2DataCookieName].Value;
            }

            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var myR2Data = new MyR2Data
                    {
                        FolderName = folderName,
                        DefaultFolder = isDefaultFolder,
                        GuidCookieValue = guid,
                        Type = (int)type,
                        InstitutionId = institutionId,
                        Json = JsonConvert.SerializeObject(userContentItem, new JsonSerializerSettings
                        {
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        })
                    };
                    uow.Save(myR2Data);
                    uow.Commit();
                    transaction.Commit();
                }
            }

            var cookie = new HttpCookie(MyR2DataCookieName, guid)
            {
                Expires = DateTime.Now.AddMonths(3)
            };
            response.Cookies.Add(cookie);
        }

        private void DeleteMyR2Data(MyR2Type type, int id, int institutionId)
        {
            var request = HttpContext.Current.Request;
            var response = HttpContext.Current.Response;
            string guid = null;
            if (request.Cookies[MyR2DataCookieName] != null)
            {
                guid = request.Cookies[MyR2DataCookieName].Value;
            }

            if (guid == null)
            {
                return;
            }

            using (var uow = _unitOfWorkProvider.Start())
            {
                using (var transaction = uow.BeginTransaction())
                {
                    var myR2Data = _myR2Data.FirstOrDefault(x =>
                        x.GuidCookieValue == guid && x.Type == (int)type && x.Id == id &&
                        x.InstitutionId == institutionId);
                    if (myR2Data != null)
                    {
                        uow.Delete(myR2Data);
                        uow.Commit();
                        transaction.Commit();
                    }
                }
            }

            var cookie = new HttpCookie(MyR2DataCookieName, guid)
            {
                Expires = DateTime.Now.AddMonths(3)
            };
            response.Cookies.Add(cookie);
        }

        private void SaveMyR2Search(SavedSearch savedSearch, MyR2Type type, int institutionId, bool isDelete)
        {
            var response = HttpContext.Current.Response;
            var request = HttpContext.Current.Request;
            try
            {
                var guid = GetNewGuid();
                if (request.Cookies[MyR2DataCookieName] != null)
                {
                    guid = request.Cookies[MyR2DataCookieName].Value;
                }

                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        MyR2Data myR2Data;

                        if (!isDelete)
                        {
                            myR2Data = new MyR2Data
                            {
                                FolderName = GetDefaultFolderName(type),
                                DefaultFolder = true,
                                GuidCookieValue = guid,
                                Type = (int)type,
                                InstitutionId = institutionId,
                                Json = JsonConvert.SerializeObject(savedSearch, new JsonSerializerSettings
                                {
                                    DefaultValueHandling = DefaultValueHandling.Ignore
                                })
                            };
                            uow.Evict(myR2Data);
                            uow.Save(myR2Data);
                        }
                        else
                        {
                            myR2Data = _myR2Data.FirstOrDefault(x =>
                                x.GuidCookieValue == guid && x.Type == (int)type && x.Id == savedSearch.Id &&
                                x.InstitutionId == institutionId);
                            if (myR2Data != null)
                            {
                                uow.Evict(myR2Data);
                                uow.Delete(myR2Data);
                            }
                        }

                        uow.Commit();
                        transaction.Commit();
                    }
                }

                var cookie = new HttpCookie(MyR2DataCookieName, guid)
                {
                    Expires = DateTime.Now.AddMonths(3)
                };
                response.Cookies.Add(cookie);
            }
            catch (Exception ex)
            {
                var errormessage = new StringBuilder();
                if (request.Cookies[MyR2DataCookieName] != null)
                {
                    errormessage.AppendLine(request.Cookies[MyR2DataCookieName].Value);
                }

                errormessage.Append(ex.Message);
                _log.Warn(errormessage, ex);
            }
        }


        private static string GetDefaultFolderName(MyR2Type type)
        {
            switch (type)
            {
                case MyR2Type.Reference:
                    return "my references".ToUpper();
                case MyR2Type.Image:
                    return "my images".ToUpper();
                case MyR2Type.CourseLink:
                    return "my course links".ToUpper();
                case MyR2Type.SavedSearch:
                    return "MY SAVED SEARCHES".ToUpper();
                case MyR2Type.SearchHistory:
                    return "MY SEARCH HISTORY".ToUpper();
                default:
                    return "my bookmarks".ToUpper();
            }
        }

        private string GetNewGuid()
        {
            return Guid.NewGuid().ToString();
        }
    }
}