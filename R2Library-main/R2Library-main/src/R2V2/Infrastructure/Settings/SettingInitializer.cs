#region

using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Common.Logging;
using R2V2.Infrastructure.DependencyInjection;
using R2V2.Exceptions;
using R2V2.Extensions;
using R2V2.Infrastructure.Initializers;

#endregion

namespace R2V2.Infrastructure.Settings
{
    public class SettingInitializer : IInitializer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        public void Initialize()
        {
            var missingSettings = new List<string>();

            var settings = ServiceLocator.Current.GetAllInstances<IAutoSettings>();
            settings.ForEach(s =>
            {
                if (s.MissingSettings.IsNotEmpty())
                {
                    missingSettings.AddRange(s.MissingSettings);
                }
            });
            var sb = new StringBuilder();
            foreach (var missingSetting in missingSettings)
            {
                sb.AppendFormat("[Missing Setting: {0}] ", missingSetting);
                Log.Debug(missingSetting);
            }

            if (sb.Length > 0)
            {
                Log.Error(sb);
            }

            if (missingSettings.IsNotEmpty())
            {
                throw new InvalidConfigurationException(missingSettings);
            }
        }
    }
}