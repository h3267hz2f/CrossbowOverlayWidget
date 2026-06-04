using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CrossbowOverlayWidget.Models;
using Windows.Storage;

namespace CrossbowOverlayWidget.Services
{
    public class ConfigService
    {
        private const string ConfigFileName = "config.json";
        private const string BackupFileName = "config.backup.json";

        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };

        public async Task<AppConfig> LoadAsync()
        {
            try
            {
                var folder = ApplicationData.Current.LocalFolder;
                var file = await folder.TryGetItemAsync(ConfigFileName) as StorageFile;
                if (file == null) return AppConfig.CreateDefault();

                string json = await FileIO.ReadTextAsync(file);
                return JsonSerializer.Deserialize<AppConfig>(json, JsonOpts) ?? AppConfig.CreateDefault();
            }
            catch
            {
                return AppConfig.CreateDefault();
            }
        }

        public async Task SaveAsync(AppConfig config)
        {
            try
            {
                var folder = ApplicationData.Current.LocalFolder;

                // Auto-backup
                var existing = await folder.TryGetItemAsync(ConfigFileName) as StorageFile;
                if (existing != null)
                    await existing.CopyAsync(folder, BackupFileName, NameCollisionOption.ReplaceExisting);

                var file = await folder.CreateFileAsync(ConfigFileName, CreationCollisionOption.ReplaceExisting);
                string json = JsonSerializer.Serialize(config, JsonOpts);
                await FileIO.WriteTextAsync(file, json);
            }
            catch { }
        }

        public string SerializeToJson(AppConfig config)
        {
            return JsonSerializer.Serialize(config, JsonOpts);
        }

        public AppConfig DeserializeFromJson(string json)
        {
            return JsonSerializer.Deserialize<AppConfig>(json, JsonOpts) ?? AppConfig.CreateDefault();
        }
    }
}
