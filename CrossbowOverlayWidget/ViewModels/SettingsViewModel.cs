using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CrossbowOverlayWidget.Enums;
using CrossbowOverlayWidget.Models;
using CrossbowOverlayWidget.Services;

namespace CrossbowOverlayWidget.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly PresetService _presetService;
        private readonly ProfileService _profileService;
        private readonly ConfigService _configService;
        private readonly ImportExportService _importExportService;

        private AppConfig _config;
        private ReticlePreset _selectedPreset;
        private GameProfile _selectedProfile;
        private string _statusMessage = "";

        // Style editing
        private double _opacity = 1.0;
        private double _majorLineWidth = 2.0;
        private double _minorLineWidth = 1.0;
        private string _majorColor = "#CC00FF00";
        private string _minorColor = "#8800FF00";
        private string _textColor = "#CCFFFFFF";
        private string _centerLineColor = "#CCFF0000";
        private int _shapeIndex = 0;
        private int _animationIndex = 0;
        private double _animationSpeed = 1.0;
        private double _scale = 1.0;
        private double _markSpacing = 50;
        private double _offsetX = 0;
        private double _offsetY = 0;

        // Calibration
        private int _calibPoint1Index = 0;
        private int _calibPoint2Index = 1;
        private int _rangeMin = 0;
        private int _rangeMax = 350;
        private int _rangeStep = 50;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler ConfigChanged;

        public SettingsViewModel(PresetService presetService, ProfileService profileService,
                                  ConfigService configService, ImportExportService importExportService)
        {
            _presetService = presetService;
            _profileService = profileService;
            _configService = configService;
            _importExportService = importExportService;
        }

        public void Initialize(AppConfig config)
        {
            _config = config;
            Presets = new ObservableCollection<ReticlePreset>(_presetService.GetAll());
            Profiles = new ObservableCollection<GameProfile>(_config.Profiles);

            _selectedPreset = _presetService.GetActive();
            _selectedProfile = _profileService.GetActive();
            LoadFromPreset(_selectedPreset);

            _rangeMin = _config.Settings != null ? 0 : 0;
            _rangeMax = 350;
            _rangeStep = 50;

            NotifyAll();
        }

        // Properties
        public ObservableCollection<ReticlePreset> Presets { get; set; } = new ObservableCollection<ReticlePreset>();
        public ObservableCollection<GameProfile> Profiles { get; set; } = new ObservableCollection<GameProfile>();

        public ReticlePreset SelectedPreset
        {
            get => _selectedPreset;
            set { _selectedPreset = value; OnPropertyChanged(); LoadFromPreset(value); ApplyToConfig(); }
        }

        public GameProfile SelectedProfile
        {
            get => _selectedProfile;
            set { _selectedProfile = value; OnPropertyChanged(); }
        }

        public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }
        public double Opacity { get => _opacity; set { _opacity = value; OnPropertyChanged(); ApplyToConfig(); } }
        public double MajorLineWidth { get => _majorLineWidth; set { _majorLineWidth = value; OnPropertyChanged(); ApplyToConfig(); } }
        public double MinorLineWidth { get => _minorLineWidth; set { _minorLineWidth = value; OnPropertyChanged(); ApplyToConfig(); } }
        public string MajorColor { get => _majorColor; set { _majorColor = value; OnPropertyChanged(); ApplyToConfig(); } }
        public string MinorColor { get => _minorColor; set { _minorColor = value; OnPropertyChanged(); ApplyToConfig(); } }
        public string TextColor { get => _textColor; set { _textColor = value; OnPropertyChanged(); ApplyToConfig(); } }
        public string CenterLineColor { get => _centerLineColor; set { _centerLineColor = value; OnPropertyChanged(); ApplyToConfig(); } }
        public int ShapeIndex { get => _shapeIndex; set { _shapeIndex = value; OnPropertyChanged(); ApplyToConfig(); } }
        public int AnimationIndex { get => _animationIndex; set { _animationIndex = value; OnPropertyChanged(); ApplyToConfig(); } }
        public double AnimationSpeed { get => _animationSpeed; set { _animationSpeed = value; OnPropertyChanged(); ApplyToConfig(); } }
        public double Scale { get => _scale; set { _scale = value; OnPropertyChanged(); ApplyToConfig(); } }
        public double MarkSpacing { get => _markSpacing; set { _markSpacing = value; OnPropertyChanged(); ApplyToConfig(); } }
        public double OffsetX { get => _offsetX; set { _offsetX = value; OnPropertyChanged(); ApplyToConfig(); } }
        public double OffsetY { get => _offsetY; set { _offsetY = value; OnPropertyChanged(); ApplyToConfig(); } }
        public int CalibPoint1Index { get => _calibPoint1Index; set { _calibPoint1Index = value; OnPropertyChanged(); } }
        public int CalibPoint2Index { get => _calibPoint2Index; set { _calibPoint2Index = value; OnPropertyChanged(); } }
        public int RangeMin { get => _rangeMin; set { _rangeMin = value; OnPropertyChanged(); } }
        public int RangeMax { get => _rangeMax; set { _rangeMax = value; OnPropertyChanged(); } }
        public int RangeStep { get => _rangeStep; set { _rangeStep = value; OnPropertyChanged(); } }

        private void LoadFromPreset(ReticlePreset preset)
        {
            if (preset == null) return;
            _opacity = preset.Style.OverallOpacity;
            _majorLineWidth = preset.Style.MajorLineWidth;
            _minorLineWidth = preset.Style.MinorLineWidth;
            _majorColor = preset.Style.MajorColor;
            _minorColor = preset.Style.MinorColor;
            _textColor = preset.Style.TextColor;
            _centerLineColor = preset.Style.CenterLineColor;
            _shapeIndex = (int)preset.Style.Shape;
            _animationIndex = (int)preset.Style.Animation;
            _animationSpeed = preset.Style.AnimationSpeed;
            _scale = preset.Scale;
            _markSpacing = preset.MarkSpacing;
            NotifyAll();
        }

        private void ApplyToConfig()
        {
            if (_selectedPreset == null) return;
            _selectedPreset.Style.OverallOpacity = _opacity;
            _selectedPreset.Style.MajorLineWidth = _majorLineWidth;
            _selectedPreset.Style.MinorLineWidth = _minorLineWidth;
            _selectedPreset.Style.MajorColor = _majorColor;
            _selectedPreset.Style.MinorColor = _minorColor;
            _selectedPreset.Style.TextColor = _textColor;
            _selectedPreset.Style.CenterLineColor = _centerLineColor;
            _selectedPreset.Style.Shape = (ReticleShape)_shapeIndex;
            _selectedPreset.Style.Animation = (AnimationType)_animationIndex;
            _selectedPreset.Style.AnimationSpeed = _animationSpeed;
            _selectedPreset.Scale = _scale;
            _selectedPreset.MarkSpacing = _markSpacing;

            if (_selectedProfile != null)
            {
                _selectedProfile.OffsetX = _offsetX;
                _selectedProfile.OffsetY = _offsetY;
            }

            _presetService.UpdatePreset(_selectedPreset);
            ConfigChanged?.Invoke(this, EventArgs.Empty);
        }

        public void CreateNewPreset()
        {
            _presetService.CreatePreset($"Preset {Presets.Count + 1}");
            Presets.Clear();
            foreach (var p in _presetService.GetAll()) Presets.Add(p);
            StatusMessage = "New preset created";
        }

        public void DeleteSelectedPreset()
        {
            if (_selectedPreset == null) return;
            _presetService.DeletePreset(_selectedPreset.Id);
            Presets.Clear();
            foreach (var p in _presetService.GetAll()) Presets.Add(p);
            _selectedPreset = Presets.FirstOrDefault();
            LoadFromPreset(_selectedPreset);
            StatusMessage = "Preset deleted";
        }

        public void CreateNewProfile()
        {
            _profileService.CreateProfile($"Game {Profiles.Count + 1}", "game.exe");
            Profiles.Clear();
            foreach (var p in _config.Profiles) Profiles.Add(p);
            StatusMessage = "New profile created";
        }

        public void SwitchProfile()
        {
            if (_selectedProfile == null) return;
            _profileService.SwitchProfile(_selectedProfile.Id);
            LoadFromPreset(_presetService.GetActive());
            ConfigChanged?.Invoke(this, EventArgs.Empty);
            StatusMessage = $"Switched to {_selectedProfile.Name}";
        }

        public void ExecuteCalibration()
        {
            if (_selectedPreset == null) return;
            var newMarks = CalibrationEngine.Calibrate(
                _selectedPreset.Marks, _calibPoint1Index, _calibPoint2Index,
                _scale, _markSpacing * _scale, _rangeMin, _rangeMax, _rangeStep);
            _selectedPreset.Marks = newMarks;
            _presetService.UpdatePreset(_selectedPreset);
            ConfigChanged?.Invoke(this, EventArgs.Empty);
            StatusMessage = $"Calibrated! {newMarks.Count} marks ({_rangeMin}m~{_rangeMax}m)";
        }

        public async Task SaveConfig()
        {
            await _configService.SaveAsync(_config);
            StatusMessage = "Config saved!";
        }

        public async Task RestoreDefaults()
        {
            _selectedPreset.Style = new ReticleStyle();
            _selectedPreset.Scale = 1.0;
            _selectedPreset.MarkSpacing = 50;
            _selectedPreset.Marks = new ReticlePreset().Marks;
            LoadFromPreset(_selectedPreset);
            ApplyToConfig();
            await SaveConfig();
            StatusMessage = "Defaults restored";
        }

        public async Task ExportConfig()
        {
            bool ok = await _importExportService.ExportConfigAsync(_config);
            StatusMessage = ok ? "Config exported!" : "Export cancelled";
        }

        public async Task ImportConfig()
        {
            var imported = await _importExportService.ImportConfigAsync();
            if (imported != null)
            {
                _config = imported;
                _presetService.Initialize(_config);
                _profileService.Initialize(_config);
                Initialize(_config);
                ConfigChanged?.Invoke(this, EventArgs.Empty);
                StatusMessage = "Config imported!";
            }
            else
            {
                StatusMessage = "Import cancelled or failed";
            }
        }

        private void NotifyAll()
        {
            foreach (var prop in new[] {
                nameof(Opacity), nameof(MajorLineWidth), nameof(MinorLineWidth),
                nameof(MajorColor), nameof(MinorColor), nameof(TextColor), nameof(CenterLineColor),
                nameof(ShapeIndex), nameof(AnimationIndex), nameof(AnimationSpeed),
                nameof(Scale), nameof(MarkSpacing), nameof(OffsetX), nameof(OffsetY)
            })
            {
                OnPropertyChanged(prop);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
