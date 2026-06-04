using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CrossbowOverlayWidget.Models;
using CrossbowOverlayWidget.Services;

namespace CrossbowOverlayWidget.ViewModels
{
    public class WidgetViewModel : INotifyPropertyChanged
    {
        private readonly PresetService _presetService;
        private readonly ConfigService _configService;

        private ReticleConfig _currentConfig;
        private string _activePresetName = "";
        private string _activeProfileName = "";
        private bool _isVisible = true;
        private bool _showStatus = true;

        public event PropertyChangedEventHandler PropertyChanged;

        public WidgetViewModel(PresetService presetService, ConfigService configService)
        {
            _presetService = presetService;
            _configService = configService;
        }

        public ReticleConfig CurrentConfig
        {
            get => _currentConfig;
            set { _currentConfig = value; OnPropertyChanged(); }
        }

        public string ActivePresetName
        {
            get => _activePresetName;
            set { _activePresetName = value; OnPropertyChanged(); }
        }

        public string ActiveProfileName
        {
            get => _activeProfileName;
            set { _activeProfileName = value; OnPropertyChanged(); }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; OnPropertyChanged(); }
        }

        public bool ShowStatus
        {
            get => _showStatus;
            set { _showStatus = value; OnPropertyChanged(); }
        }

        public void Refresh()
        {
            CurrentConfig = _presetService.BuildActiveConfig();
            CurrentConfig.IsVisible = _isVisible;
            CurrentConfig.ShowStatus = _showStatus;
            var preset = _presetService.GetActive();
            ActivePresetName = preset?.Name ?? "";
            var profile = _presetService.GetActiveProfile();
            ActiveProfileName = profile?.Name ?? "";
        }

        public void ToggleVisibility()
        {
            IsVisible = !IsVisible;
            Refresh();
        }

        public void NextPreset()
        {
            var all = _presetService.GetAll();
            if (all.Count == 0) return;
            var active = _presetService.GetActive();
            int idx = active != null ? all.IndexOf(active) : -1;
            idx = (idx + 1) % all.Count;
            _presetService.ActivatePreset(all[idx].Id);
            Refresh();
        }

        public void PreviousPreset()
        {
            var all = _presetService.GetAll();
            if (all.Count == 0) return;
            var active = _presetService.GetActive();
            int idx = active != null ? all.IndexOf(active) : 0;
            idx = (idx - 1 + all.Count) % all.Count;
            _presetService.ActivatePreset(all[idx].Id);
            Refresh();
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
