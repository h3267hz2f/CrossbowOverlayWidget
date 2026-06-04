using System;
using System.Collections.ObjectModel;
using System.Linq;
using CrossbowOverlayWidget.Enums;
using CrossbowOverlayWidget.Models;
using CrossbowOverlayWidget.Rendering;
using CrossbowOverlayWidget.Services;
using CrossbowOverlayWidget.ViewModels;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace CrossbowOverlayWidget
{
    public sealed partial class SettingsPage : Page
    {
        private readonly ConfigService _configService = new ConfigService();
        private readonly PresetService _presetService = new PresetService();
        private readonly ProfileService _profileService = new ProfileService();
        private readonly ImportExportService _importExportService = new ImportExportService();
        private readonly ReticleRenderer _previewRenderer = new ReticleRenderer();
        private SettingsViewModel _viewModel;
        private bool _isUpdating;

        // Debounced auto-save for cross-process refresh
        private DispatcherTimer _saveDebounceTimer;

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var config = await _configService.LoadAsync();
            _presetService.Initialize(config);
            _profileService.Initialize(config);

            _viewModel = new SettingsViewModel(
                _presetService, _profileService, _configService, _importExportService);
            _viewModel.Initialize(config);
            _viewModel.ConfigChanged += (s, ev) =>
            {
                PreviewCanvas.Invalidate();
                ScheduleAutoSave();
            };

            // Initialize debounce timer (fires 800ms after last config change)
            _saveDebounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(800)
            };
            _saveDebounceTimer.Tick += async (s, ev) =>
            {
                _saveDebounceTimer.Stop();
                try
                {
                    await _configService.SaveAsync(config);
                    await Services.ConfigChangeSignal.NotifyAsync();
                }
                catch { /* non-critical */ }
            };

            BindUI();
        }

        private void BindUI()
        {
            _isUpdating = true;

            // Presets
            PresetCombo.ItemsSource = _viewModel.Presets;
            PresetCombo.SelectedItem = _viewModel.SelectedPreset;

            // Profiles
            ProfileCombo.ItemsSource = _viewModel.Profiles;
            ProfileCombo.SelectedItem = _viewModel.SelectedProfile;

            // Shape & Animation
            ShapeCombo.SelectedIndex = _viewModel.ShapeIndex;
            AnimationCombo.SelectedIndex = _viewModel.AnimationIndex;
            AnimSpeedSlider.Value = _viewModel.AnimationSpeed;

            // Colors
            MajorColorBox.Text = _viewModel.MajorColor;
            MinorColorBox.Text = _viewModel.MinorColor;
            TextColorBox.Text = _viewModel.TextColor;
            CenterColorBox.Text = _viewModel.CenterLineColor;

            // v2: Gradient & Night
            GradientToggle.IsOn = _viewModel.GradientColor;
            NearColorBox.Text = _viewModel.NearColor;
            FarColorBox.Text = _viewModel.FarColor;
            NightModeToggle.IsOn = _viewModel.NightMode;

            // Sliders
            ScaleSlider.Value = _viewModel.Scale;
            SpacingSlider.Value = _viewModel.MarkSpacing;
            OpacitySlider.Value = _viewModel.Opacity;
            MajorWidthSlider.Value = _viewModel.MajorLineWidth;
            MinorWidthSlider.Value = _viewModel.MinorLineWidth;
            OffsetXSlider.Value = _viewModel.OffsetX;
            OffsetYSlider.Value = _viewModel.OffsetY;

            // v2: New sliders & toggles
            CenterGapSlider.Value = _viewModel.CenterGap;
            TaperSlider.Value = _viewModel.TaperFactor;
            CenterLockToggle.IsOn = _viewModel.CenterLocked;
            LabelPositionCombo.SelectedIndex = _viewModel.LabelPositionIndex;

            // Calibration combos
            PopulateCalibrationCombos();

            _isUpdating = false;
            PreviewCanvas.Invalidate();
        }

        private void PopulateCalibrationCombos()
        {
            var preset = _viewModel?.SelectedPreset;
            if (preset == null) return;

            Calib1Combo.ItemsSource = preset.Marks;
            Calib2Combo.ItemsSource = preset.Marks;

            if (preset.Marks.Count >= 2)
            {
                Calib1Combo.SelectedIndex = 0;
                Calib2Combo.SelectedIndex = preset.Marks.Count - 1;
            }
        }

        #region Preview Canvas

        private void OnPreviewDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var cfg = BuildPreviewConfig();
            if (cfg == null) return;

            float w = (float)sender.ActualWidth;
            float h = (float)sender.ActualHeight;
            _previewRenderer.Draw(args.DrawingSession, cfg, w, h, 0f);
        }

        private ReticleConfig BuildPreviewConfig()
        {
            if (_viewModel?.SelectedPreset == null) return null;

            var preset = _viewModel.SelectedPreset;
            var profile = _viewModel.SelectedProfile;

            return new ReticleConfig
            {
                Scale = preset.Scale,
                MarkSpacing = preset.MarkSpacing,
                Marks = new System.Collections.Generic.List<DistanceMark>(preset.Marks),
                Style = preset.Style,
                OffsetX = profile?.OffsetX ?? 0,
                OffsetY = profile?.OffsetY ?? 0,
                CenterLocked = _viewModel.CenterLocked,
                IsVisible = true,
                ShowStatus = false,
                DiamondVisible = preset.Style.ShowCenterDiamond
            };
        }

        #endregion

        #region Preset Handlers

        private void OnPresetSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdating || _viewModel == null) return;
            if (PresetCombo.SelectedItem is ReticlePreset preset)
            {
                _viewModel.SelectedPreset = preset;
                RefreshStyleUI();
                PopulateCalibrationCombos();
            }
        }

        private void OnCreatePreset(object sender, RoutedEventArgs e)
        {
            _viewModel?.CreateNewPreset();
            PresetCombo.ItemsSource = _viewModel.Presets;
            ShowStatus(_viewModel.StatusMessage, false);
        }

        private void OnDuplicatePreset(object sender, RoutedEventArgs e)
        {
            _viewModel?.DuplicateSelectedPreset();
            PresetCombo.ItemsSource = _viewModel.Presets;
            PresetCombo.SelectedItem = _viewModel.SelectedPreset;
            ShowStatus(_viewModel.StatusMessage, false);
        }

        private void OnDeletePreset(object sender, RoutedEventArgs e)
        {
            _viewModel?.DeleteSelectedPreset();
            PresetCombo.ItemsSource = _viewModel.Presets;
            PresetCombo.SelectedItem = _viewModel.SelectedPreset;
            RefreshStyleUI();
            ShowStatus(_viewModel.StatusMessage, false);
        }

        #endregion

        #region Style Handlers

        private void OnShapeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdating || _viewModel == null) return;
            _viewModel.ShapeIndex = ShapeCombo.SelectedIndex;
            PreviewCanvas.Invalidate();
        }

        private void OnAnimationChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdating || _viewModel == null) return;
            _viewModel.AnimationIndex = AnimationCombo.SelectedIndex;
            PreviewCanvas.Invalidate();
        }

        private void OnAnimSpeedChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_isUpdating || _viewModel == null) return;
            _viewModel.AnimationSpeed = AnimSpeedSlider.Value;
            PreviewCanvas.Invalidate();
        }

        private void OnColorChanged(object sender, RoutedEventArgs e)
        {
            if (_isUpdating || _viewModel == null) return;
            _viewModel.MajorColor = MajorColorBox.Text;
            _viewModel.MinorColor = MinorColorBox.Text;
            _viewModel.TextColor = TextColorBox.Text;
            _viewModel.CenterLineColor = CenterColorBox.Text;
            PreviewCanvas.Invalidate();
        }

        private void OnSliderChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_isUpdating || _viewModel == null) return;
            _viewModel.Scale = ScaleSlider.Value;
            _viewModel.MarkSpacing = SpacingSlider.Value;
            _viewModel.Opacity = OpacitySlider.Value;
            _viewModel.MajorLineWidth = MajorWidthSlider.Value;
            _viewModel.MinorLineWidth = MinorWidthSlider.Value;
            _viewModel.OffsetX = OffsetXSlider.Value;
            _viewModel.OffsetY = OffsetYSlider.Value;
            _viewModel.CenterGap = CenterGapSlider.Value;
            _viewModel.TaperFactor = TaperSlider.Value;
            PreviewCanvas.Invalidate();
        }

        // v2: Gradient handlers
        private void OnGradientToggled(object sender, RoutedEventArgs e)
        {
            if (_isUpdating || _viewModel == null) return;
            _viewModel.GradientColor = GradientToggle.IsOn;
            PreviewCanvas.Invalidate();
        }

        private void OnGradientColorChanged(object sender, RoutedEventArgs e)
        {
            if (_isUpdating || _viewModel == null) return;
            _viewModel.NearColor = NearColorBox.Text;
            _viewModel.FarColor = FarColorBox.Text;
            PreviewCanvas.Invalidate();
        }

        private void OnNightModeToggled(object sender, RoutedEventArgs e)
        {
            if (_isUpdating || _viewModel == null) return;
            _viewModel.NightMode = NightModeToggle.IsOn;
            PreviewCanvas.Invalidate();
        }

        private void OnCenterLockToggled(object sender, RoutedEventArgs e)
        {
            if (_isUpdating || _viewModel == null) return;
            _viewModel.CenterLocked = CenterLockToggle.IsOn;
            PreviewCanvas.Invalidate();
        }

        private void OnLabelPositionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdating || _viewModel == null) return;
            _viewModel.LabelPositionIndex = LabelPositionCombo.SelectedIndex;
            PreviewCanvas.Invalidate();
        }

        private void RefreshStyleUI()
        {
            _isUpdating = true;

            ShapeCombo.SelectedIndex = _viewModel.ShapeIndex;
            AnimationCombo.SelectedIndex = _viewModel.AnimationIndex;
            AnimSpeedSlider.Value = _viewModel.AnimationSpeed;

            MajorColorBox.Text = _viewModel.MajorColor;
            MinorColorBox.Text = _viewModel.MinorColor;
            TextColorBox.Text = _viewModel.TextColor;
            CenterColorBox.Text = _viewModel.CenterLineColor;

            GradientToggle.IsOn = _viewModel.GradientColor;
            NearColorBox.Text = _viewModel.NearColor;
            FarColorBox.Text = _viewModel.FarColor;
            NightModeToggle.IsOn = _viewModel.NightMode;

            ScaleSlider.Value = _viewModel.Scale;
            SpacingSlider.Value = _viewModel.MarkSpacing;
            OpacitySlider.Value = _viewModel.Opacity;
            MajorWidthSlider.Value = _viewModel.MajorLineWidth;
            MinorWidthSlider.Value = _viewModel.MinorLineWidth;
            OffsetXSlider.Value = _viewModel.OffsetX;
            OffsetYSlider.Value = _viewModel.OffsetY;

            CenterGapSlider.Value = _viewModel.CenterGap;
            TaperSlider.Value = _viewModel.TaperFactor;
            CenterLockToggle.IsOn = _viewModel.CenterLocked;
            LabelPositionCombo.SelectedIndex = _viewModel.LabelPositionIndex;

            _isUpdating = false;
            PreviewCanvas.Invalidate();
        }

        #endregion

        #region Calibration Handlers

        private void OnCalibrateClick(object sender, RoutedEventArgs e)
        {
            int idx1 = Calib1Combo.SelectedIndex;
            int idx2 = Calib2Combo.SelectedIndex;

            if (idx1 < 0 || idx2 < 0 || idx1 == idx2)
            {
                ShowStatus("错误: 请选择2个不同的校准点", true);
                return;
            }

            if (!int.TryParse(RangeMinBox.Text, out int rangeMin) ||
                !int.TryParse(RangeMaxBox.Text, out int rangeMax) ||
                !int.TryParse(RangeStepBox.Text, out int step) ||
                step <= 0 || rangeMin >= rangeMax)
            {
                ShowStatus("错误: 范围/间距参数无效", true);
                return;
            }

            _viewModel.CalibPoint1Index = idx1;
            _viewModel.CalibPoint2Index = idx2;
            _viewModel.RangeMin = rangeMin;
            _viewModel.RangeMax = rangeMax;
            _viewModel.RangeStep = step;
            _viewModel.ExecuteCalibration();

            PopulateCalibrationCombos();
            ShowStatus(_viewModel.StatusMessage, false);
            PreviewCanvas.Invalidate();
        }

        private async void OnRestoreClick(object sender, RoutedEventArgs e)
        {
            await _viewModel.RestoreDefaults();
            RefreshStyleUI();
            PopulateCalibrationCombos();
            ShowStatus(_viewModel.StatusMessage, false);
        }

        private async void OnSaveClick(object sender, RoutedEventArgs e)
        {
            await _viewModel.SaveConfig();
            ShowStatus(_viewModel.StatusMessage, false);
        }

        #endregion

        #region Profile Handlers

        private void OnProfileSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdating || _viewModel == null) return;
            if (ProfileCombo.SelectedItem is GameProfile profile)
            {
                _viewModel.SelectedProfile = profile;
                ProfileGameExeBox.Text = profile.GameExecutable;
            }
        }

        private void OnGameExeChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel?.SelectedProfile != null)
                _viewModel.SelectedProfile.GameExecutable = ProfileGameExeBox.Text;
        }

        private void OnCreateProfile(object sender, RoutedEventArgs e)
        {
            _viewModel?.CreateNewProfile();
            ProfileCombo.ItemsSource = _viewModel.Profiles;
            ShowStatus(_viewModel.StatusMessage, false);
        }

        private void OnSwitchProfile(object sender, RoutedEventArgs e)
        {
            _viewModel?.SwitchProfile();
            RefreshStyleUI();
            PopulateCalibrationCombos();
            ShowStatus(_viewModel.StatusMessage, false);
        }

        private void OnDeleteProfile(object sender, RoutedEventArgs e)
        {
            _viewModel?.DeleteSelectedProfile();
            ProfileCombo.ItemsSource = _viewModel.Profiles;
            ProfileCombo.SelectedItem = _viewModel.SelectedProfile;
            ShowStatus(_viewModel.StatusMessage, false);
        }

        #endregion

        #region Import/Export

        private async void OnExportClick(object sender, RoutedEventArgs e)
        {
            await _viewModel.ExportConfig();
            ShowStatus(_viewModel.StatusMessage, false);
        }

        private async void OnImportClick(object sender, RoutedEventArgs e)
        {
            await _viewModel.ImportConfig();
            PresetCombo.ItemsSource = _viewModel.Presets;
            PresetCombo.SelectedItem = _viewModel.SelectedPreset;
            ProfileCombo.ItemsSource = _viewModel.Profiles;
            ProfileCombo.SelectedItem = _viewModel.SelectedProfile;
            RefreshStyleUI();
            PopulateCalibrationCombos();
            ShowStatus(_viewModel.StatusMessage, false);
        }

        #endregion

        #region Status Toast

        private void ShowStatus(string message, bool isError)
        {
            StatusText.Text = message;
            StatusText.Foreground = new SolidColorBrush(
                isError ? Colors.IndianRed : Colors.LightGreen);
            StatusToast.Visibility = Visibility.Visible;

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            timer.Tick += (s, e) =>
            {
                StatusToast.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }

        #endregion

        #region Auto-Save (Cross-process Refresh)

        /// <summary>
        /// Debounced auto-save: restarts the timer on every config change.
        /// Only saves + signals after 800ms of inactivity.
        /// </summary>
        private void ScheduleAutoSave()
        {
            _saveDebounceTimer.Stop();
            _saveDebounceTimer.Start();
        }

        #endregion
    }
}
