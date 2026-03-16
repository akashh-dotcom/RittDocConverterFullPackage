#region

using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;
using R2V2.Core.ReserveShelf;
using R2V2.Core.Resource;
using R2V2.Infrastructure.Logging;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.Core.Institution
{
    public class ReserveShelfService
    {
        private readonly ILog<ReserveShelfService> _log;
        private readonly IQueryable<ReserveShelfUrl> _reserveShelfUrls;
        private readonly IQueryable<ReserveShelf.ReserveShelf> _reserveShelves;

        private readonly IResourceService _resourceService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public ReserveShelfService(
            ILog<ReserveShelfService> log
            , IUnitOfWorkProvider unitOfWorkProvider
            , IQueryable<ReserveShelf.ReserveShelf> reserveShelves
            , IQueryable<ReserveShelfUrl> reserveShelfUrls
            , IResourceService resourceService
        )
        {
            _log = log;
            _unitOfWorkProvider = unitOfWorkProvider;
            _reserveShelves = reserveShelves;
            _reserveShelfUrls = reserveShelfUrls;
            _resourceService = resourceService;
        }

        public IEnumerable<ReserveShelf.ReserveShelf> GetInstititionsReserveShelves(int institutionId)
        {
            var reserveShelves = _reserveShelves.Where(x => x.Institution.Id == institutionId)
                .Fetch(y => y.ReserveShelfUrls)
                .ToList();

            var reserveShelvesToReturn = new List<ReserveShelf.ReserveShelf>();
            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        foreach (var reserveShelf in reserveShelves)
                        {
                            var updateShelf = false;
                            var reserveShelfResourcesToReturn = reserveShelf.ReserveShelfResources.ToList();
                            foreach (var reserveShelfReserveShelfResource in reserveShelf.ReserveShelfResources)
                            {
                                var resource =
                                    _resourceService.GetResource(reserveShelfReserveShelfResource.ResourceId);

                                if (resource == null)
                                {
                                    reserveShelfResourcesToReturn.Remove(reserveShelfReserveShelfResource);
                                    uow.Delete(reserveShelfReserveShelfResource);
                                    updateShelf = true;
                                }
                            }

                            if (updateShelf)
                            {
                                uow.Commit();
                                transaction.Commit();
                                uow.Evict(reserveShelf);
                                reserveShelf.ReserveShelfResources = reserveShelfResourcesToReturn;
                            }

                            reserveShelvesToReturn.Add(reserveShelf);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }

            return reserveShelvesToReturn;
        }

        public ReserveShelfUrl GetReserveShelfUrl(int reserveShelfUrlId, int reserveShelfListId)
        {
            return _reserveShelfUrls.SingleOrDefault(x =>
                x.Id == reserveShelfUrlId && x.ReserveShelfId == reserveShelfListId);
        }

        public int DeleteReserveShelfList(int reserveShelfId, int instititonId)
        {
            return SaveUpdateReserveShelfList(reserveShelfId, instititonId, null, null, null, false);
        }

        /// <summary>
        ///     Null Name and Description will remove delete the shelf
        /// </summary>
        public int SaveUpdateReserveShelfList(int reserveShelfId, int instititonId, string name, string description,
            string defaultSortBy, bool isAscending)
        {
            var reserveShelf = reserveShelfId == 0
                ? new ReserveShelf.ReserveShelf()
                : _reserveShelves.SingleOrDefault(x => x.Id == reserveShelfId && x.Institution.Id == instititonId);
            if (reserveShelf == null)
            {
                _log.ErrorFormat("Could not find reserveShelf to save | reserveShelfId:{0}  instititonId:{1}",
                    reserveShelfId, instititonId);
                return 0;
            }

            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(description))
            {
                reserveShelf.Name = name;
                reserveShelf.Description = description;
                reserveShelf.DefaultSortBy = defaultSortBy;
                reserveShelf.RecordStatus = true;
                reserveShelf.IsAscending = isAscending;
                reserveShelf.Institution = new Institution { Id = instititonId };

                if (reserveShelf.LibraryLocation == 0)
                {
                    reserveShelf.LibraryLocation = 10;
                }
            }
            else
            {
                reserveShelf.RecordStatus = false;
            }

            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        uow.SaveOrUpdate(reserveShelf);

                        uow.Commit();
                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }

            return reserveShelf.Id;
        }

        public void AddDeleteReserveShelfResource(int instititonId, int reserveShelfId, int resourceId,
            bool addInstitutionResource)
        {
            var reserveShelf =
                _reserveShelves.SingleOrDefault(x => x.Id == reserveShelfId && x.Institution.Id == instititonId);

            ReserveShelfResource reserveShelfResource = null;

            if (reserveShelf == null)
            {
                _log.ErrorFormat(
                    "Could not find reserveShelf to add resources too | reserveShelfId:{0}  instititonId:{1}",
                    reserveShelfId, instititonId);
                return;
            }

            if (addInstitutionResource)
            {
                reserveShelfResource = new ReserveShelfResource
                {
                    ReserveShelfListId = reserveShelfId,
                    //InstitutionResource = _institutionResources.SingleOrDefault(x => x.InstitutionId == instititonId && x.ResourceId == resourceId),
                    ResourceId = resourceId,
                    RecordStatus = true
                };
            }
            else
            {
                foreach (var institutionResource in reserveShelf.ReserveShelfResources.Where(x =>
                             x.ResourceId == resourceId))
                {
                    institutionResource.RecordStatus = false;
                }
            }

            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        if (addInstitutionResource)
                        {
                            uow.SaveOrUpdate(reserveShelfResource);
                        }
                        else
                        {
                            uow.SaveOrUpdate(reserveShelf);
                        }

                        uow.Commit();
                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }

        public void AddReserveShelfUrl(int reserveShelfId, string url, string description)
        {
            var newReserveShelfUrl = new ReserveShelfUrl
            {
                ReserveShelfId = reserveShelfId,
                Url = url,
                Description = description,
                RecordStatus = true
            };

            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        uow.SaveOrUpdate(newReserveShelfUrl);

                        uow.Commit();
                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }

        public void UpdateReserveShelfUrl(int reserveShelfId, int reserveShelfUrlId, string url, string description)
        {
            var editReserveShelfUrl =
                _reserveShelfUrls.SingleOrDefault(x => x.ReserveShelfId == reserveShelfId && x.Id == reserveShelfUrlId);

            if (editReserveShelfUrl == null)
            {
                _log.ErrorFormat("Could not find ReserveShelfUrl to update | reserveShelfId:{0}  reserveShelfUrlId:{1}",
                    reserveShelfId, reserveShelfUrlId);
                return;
            }

            editReserveShelfUrl.Url = url;
            editReserveShelfUrl.Description = description;

            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        uow.SaveOrUpdate(editReserveShelfUrl);

                        uow.Commit();
                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }

        public void DeleteReserveShelfUrl(int reserveShelfId, int reserveShelfUrlId)
        {
            var deleteReserveShelfUrl =
                _reserveShelfUrls.SingleOrDefault(x => x.ReserveShelfId == reserveShelfId && x.Id == reserveShelfUrlId);
            if (deleteReserveShelfUrl == null)
            {
                _log.ErrorFormat("Could not find ReserveShelfUrl to delete | reserveShelfId:{0}  reserveShelfUrlId:{1}",
                    reserveShelfId, reserveShelfUrlId);
                return;
            }

            deleteReserveShelfUrl.RecordStatus = false;

            try
            {
                using (var uow = _unitOfWorkProvider.Start())
                {
                    using (var transaction = uow.BeginTransaction())
                    {
                        uow.SaveOrUpdate(deleteReserveShelfUrl);

                        uow.Commit();
                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw;
            }
        }
    }
}