using System;
using System.Collections.Generic;
using System.Linq;
using CrossbowOverlayWidget.Models;

namespace CrossbowOverlayWidget.Services
{
    public class PresetService
    {
        private AppConfig _config;

        public event EventHandler<ReticlePreset> PresetChanged;

        public void Initialize(AppConfig config)
        {
            _config = config;
        }

        public IReadOnlyList<ReticlePreset> GetAll()
        {
            return _config?.Presets?.AsReadOnly() ?? new List<ReticlePreset>().AsReadOnly();
        }

        public ReticlePreset GetActive()
        {
            if (_config == null) return null;
            var profile = GetActiveProfile();
            if (profile == null) return _config.Presets.FirstOrDefault();
            return _config.Presets.FirstOrDefault(p => p.Id == profile.ActivePresetId)
                ?? _config.Presets.FirstOrDefault();
        }

        public GameProfile GetActiveProfile()
        {
            return _config?.Profiles?.FirstOrDefault(p => p.Id == _config.ActiveProfileId)
                ?? _config?.Profiles?.FirstOrDefault();
        }

        public void CreatePreset(string name)
        {
            var preset = new ReticlePreset { Name = name };
            _config.Presets.Add(preset);
        }

        public void UpdatePreset(ReticlePreset preset)
        {
            var idx = _config.Presets.FindIndex(p => p.Id == preset.Id);
            if (idx >= 0) _config.Presets[idx] = preset;
        }

        public void DeletePreset(string presetId)
        {
            _config.Presets.RemoveAll(p => p.Id == presetId);
        }

        public void ActivatePreset(string presetId)
        {
            var profile = GetActiveProfile();
            if (profile != null)
            {
                profile.ActivePresetId = presetId;
                PresetChanged?.Invoke(this, GetActive());
            }
        }

        public ReticleConfig BuildActiveConfig()
        {
            var preset = GetActive();
            var profile = GetActiveProfile();
            if (preset == null) return new ReticleConfig();

            return new ReticleConfig
            {
                Scale = preset.Scale,
                MarkSpacing = preset.MarkSpacing,
                Marks = new List<DistanceMark>(preset.Marks),
                Style = preset.Style,
                OffsetX = profile?.OffsetX ?? 0,
                OffsetY = profile?.OffsetY ?? 0
            };
        }
    }
}
