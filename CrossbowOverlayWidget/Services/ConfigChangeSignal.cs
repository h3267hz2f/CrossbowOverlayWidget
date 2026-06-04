using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace CrossbowOverlayWidget.Services
{
    /// <summary>
    /// Cross-process config change signaling.
    /// Settings widget writes a signal file when config is saved;
    /// main widget polls for it and reloads config.
    /// </summary>
    public static class ConfigChangeSignal
    {
        private const string SignalFileName = ".config_updated";

        /// <summary>
        /// Called by Settings widget after saving config — creates a signal file.
        /// </summary>
        public static async Task NotifyAsync()
        {
            try
            {
                var folder = ApplicationData.Current.LocalFolder;
                var file = await folder.CreateFileAsync(
                    SignalFileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, DateTimeOffset.UtcNow.Ticks.ToString());
            }
            catch { /* non-critical */ }
        }

        /// <summary>
        /// Called by main widget to check if config has changed.
        /// Returns true and deletes the signal file if a change was detected.
        /// </summary>
        public static async Task<bool> CheckAndConsumeAsync()
        {
            try
            {
                var folder = ApplicationData.Current.LocalFolder;
                var item = await folder.TryGetItemAsync(SignalFileName);
                if (item != null)
                {
                    await item.DeleteAsync();
                    return true;
                }
            }
            catch { }
            return false;
        }
    }
}
