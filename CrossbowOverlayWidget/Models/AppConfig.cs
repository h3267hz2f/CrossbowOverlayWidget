using System;
using System.Collections.Generic;

namespace CrossbowOverlayWidget.Models
{
    public class AppConfig
    {
        public int SchemaVersion { get; set; } = 1;
        public string ActiveProfileId { get; set; } = "";
        public List<GameProfile> Profiles { get; set; } = new List<GameProfile>();
        public List<ReticlePreset> Presets { get; set; } = new List<ReticlePreset>();
        public GlobalSettings Settings { get; set; } = new GlobalSettings();

        public static AppConfig CreateDefault()
        {
            var defaultPreset = new ReticlePreset { Name = "经典十字" };
            var defaultProfile = new GameProfile
            {
                Name = "和平精英",
                ActivePresetId = defaultPreset.Id
            };
            return new AppConfig
            {
                ActiveProfileId = defaultProfile.Id,
                Profiles = new List<GameProfile> { defaultProfile },
                Presets = new List<ReticlePreset> { defaultPreset }
            };
        }
    }
}
