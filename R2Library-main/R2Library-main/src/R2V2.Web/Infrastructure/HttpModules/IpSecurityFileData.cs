#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.Web.Infrastructure.HttpModules
{
    public class IpSecurityFileData
    {
        private static readonly ILog<IpSecurityFileData> Log = new Log<IpSecurityFileData>();
        private static readonly object Object = new object();
        private readonly string[] _additionalFileDependencies;
        private readonly string _appContentKey;
        private readonly string _fileName;
        private string[] _dependenciesFilesFullPath;

        private string _fileNameFullPath;
        //private readonly TimeSpan _cacheTimeToLive;

        public IpSecurityFileData(string filename, string appContentKey, string[] additionalFileDependencies)
        {
            _fileName = filename;
            _appContentKey = appContentKey;
            //_cacheTimeToLive = new TimeSpan(0, cacheTimeToLiveInMinutes, 0);
            _additionalFileDependencies = additionalFileDependencies;
        }

        public Dictionary<string, int> GetList(HttpContext context, int cacheTimeToLiveInMinutes)
        {
            var list = (Dictionary<string, int>)context.Cache[_appContentKey];
            if (list == null)
            {
                list = GetList(GetFromCurrentContext(context));
                context.Cache.Insert(_appContentKey, list, new CacheDependency(GetFromCurrentContext(context)),
                    Cache.NoAbsoluteExpiration,
                    new TimeSpan(0, cacheTimeToLiveInMinutes, 0));
            }

            return list;
        }

        private Dictionary<string, int> GetList(string configPath)
        {
            var list = new Dictionary<string, int>();
            try
            {
                using (var reader = new StreamReader(configPath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.Length != 0)
                        {
                            var parts = line.Split('\t');
                            Log.DebugFormat("Value: {0}, description: {1}", parts[0], parts.Length > 1 ? parts[1] : "");
                            list.Add(parts[0], 0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // swallow exception and log it.
                var msg = new StringBuilder();
                msg.AppendFormat("configPath: {0}", configPath).AppendLine().AppendLine(ex.Message);
                Log.ErrorFormat(msg.ToString(), ex);
            }

            return list;
        }

        public string GetFromCurrentContext(HttpContext context)
        {
            if (_fileNameFullPath != null)
            {
                Log.DebugFormat("GetFromCurrentContext(1) - _fileNameFullPath: {0}", _fileNameFullPath);
                return _fileNameFullPath;
            }

            lock (Object)
            {
                if (_fileNameFullPath == null)
                {
                    //_fileNameFullPath = string.Format("{0}\\{1}", context.Server.MapPath("/"), _fileName);
                    _fileNameFullPath = Path.Combine(context.Server.MapPath("/_configs"), _fileName);
                    Log.DebugFormat("_fileNameFullPath: {0}", _fileNameFullPath);
                }
            }

            Log.DebugFormat("GetFromCurrentContext(2) - _fileNameFullPath: {0}", _fileNameFullPath);
            return _fileNameFullPath;
        }

        public string[] GetListOfDependencyFiles(HttpContext context)
        {
            if (_dependenciesFilesFullPath != null)
            {
                Log.DebugFormat("GetListOfDependencyFiles(1) - _dependenciesFilesFullPath: {0}",
                    string.Join(", ", _dependenciesFilesFullPath));
                return _dependenciesFilesFullPath;
            }

            var files = new List<string>();
            lock (Object)
            {
                _dependenciesFilesFullPath = files.ToArray();
                if (_dependenciesFilesFullPath == null)
                {
                    files.AddRange(_additionalFileDependencies.Select(additionalFileDependency =>
                        Path.Combine(context.Server.MapPath("/"), additionalFileDependency)));
                    _dependenciesFilesFullPath = files.ToArray();
                    Log.DebugFormat("_dependenciesFilesFullPath: {0}", string.Join(", ", _dependenciesFilesFullPath));
                }

                Log.DebugFormat("GetListOfDependencyFiles(2) - _dependenciesFilesFullPath: {0}",
                    string.Join(", ", _dependenciesFilesFullPath));
                return _dependenciesFilesFullPath;
            }
        }
    }
}