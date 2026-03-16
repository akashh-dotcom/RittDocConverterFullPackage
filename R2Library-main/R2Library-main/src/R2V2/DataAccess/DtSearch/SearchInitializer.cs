#region

using System;
using System.Reflection;
using log4net;
using dtSearch.Engine;
using R2V2.Infrastructure.Settings;

#endregion

namespace R2V2.DataAccess.DtSearch
{
    public class SearchInitializer : ISearchInitializer
    {
        //private readonly ILog _log;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ContentSettings _contentSettings;

        //public SearchInitializer(ILog<SearchInitializer> log, ContentSettings contentSettings)
        public SearchInitializer(ContentSettings contentSettings)
        {
            //_log = log;
            _contentSettings = contentSettings;
        }

        public string DtSearchVersion { get; private set; }


        public bool Init()
        {
            return SetOptions();
        }

        private bool SetOptions()
        {
            try
            {
                // do not do in production!
                Log.DebugFormat("DtSearchLogFilePath: {0}", _contentSettings.DtSearchLogFilePath);
                if (!string.IsNullOrEmpty(_contentSettings.DtSearchLogFilePath))
                {
                    Log.ErrorFormat("dtSearch log file set to {0} - NEVER SET 'DtSearchLogFilePath' IN PRODUCTION!!!",
                        _contentSettings.DtSearchLogFilePath);
                    Server.SetDebugLogging(_contentSettings.DtSearchLogFilePath,
                        DebugLogFlags.dtsLogTime | DebugLogFlags.dtsLogAppend);
                }

                // Set the HomeDir in Options, or stemming won't work
                // (stemming needs to find the stemming.dat file)
                Log.InfoFormat("DtSearchBinLocation: {0}", _contentSettings.DtSearchBinLocation);
                var opts = new Options { HomeDir = _contentSettings.DtSearchBinLocation };
                opts.Save();

                Log.Debug("DtSearch options successfully saved");

                var dtSearchServer = new Server();
                DtSearchVersion = $"{dtSearchServer.MajorVersion}.{dtSearchServer.MinorVersion}.{dtSearchServer.Build}";
                Log.DebugFormat("DtSearchVersion: {0}", DtSearchVersion);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return false;
            }
        }
    }
}