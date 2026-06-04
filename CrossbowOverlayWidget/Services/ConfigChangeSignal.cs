using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace CrossbowOverlayWidget.Services
{
    /// <summary>
    /// Cross-process config change signaling using LocalSettings.
    /// LocalSettings is backed by the Windows registry and is truly
    /// shared across processes of the same app package.
    /// </summary>
    public static class ConfigChangeSignal
    {
        private const string SettingsKey = "ConfigLastModified";
        private static long _lastKnownTicks = 0;

        /// <summary>
        /// Called by Settings widget after any config change — writes a timestamp.
        /// </summary>
        public static void Notify()
        {
            try
            {
                var settings = ApplicationData.Current.LocalSettings;
                var now = DateTimeOffset.UtcNow.Ticks;
                settings.Values[SettingsKey] = now.ToString();
            }
            catch { /* non-critical */ }
        }

        /// <summary>
        /// Called by main widget to check if config has changed since last check.
        /// Returns true if a newer change was detected.
        /// </summary>
        public static bool Check()
        {
            try
            {
                var settings = ApplicationData.Current.LocalSettings;
                if (settings.Values.TryGetValue(SettingsKey, out object val))
                {
                    if (long.TryParse(val?.ToString(), out long ticks))
                    {
                        if (ticks > _lastKnownTicks)
                        {
                            _lastKnownTicks = ticks;
                            return true;
                        }
                    }
                }
            }
            catch { }
            return false;
        }
    }
}
