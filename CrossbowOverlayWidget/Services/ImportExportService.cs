using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CrossbowOverlayWidget.Models;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace CrossbowOverlayWidget.Services
{
    public class ImportExportService
    {
        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        public async Task<bool> ExportConfigAsync(AppConfig config)
        {
            try
            {
                var picker = new FileSavePicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                    SuggestedFileName = "crossbow_config"
                };
                picker.FileTypeChoices.Add("JSON", new[] { ".json" });

                var file = await picker.PickSaveFileAsync();
                if (file == null) return false;

                string json = JsonSerializer.Serialize(config, JsonOpts);
                await FileIO.WriteTextAsync(file, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<AppConfig> ImportConfigAsync()
        {
            try
            {
                var picker = new FileOpenPicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };
                picker.FileTypeFilter.Add(".json");

                var file = await picker.PickSingleFileAsync();
                if (file == null) return null;

                string json = await FileIO.ReadTextAsync(file);
                return JsonSerializer.Deserialize<AppConfig>(json, JsonOpts);
            }
            catch
            {
                return null;
            }
        }
    }
}
