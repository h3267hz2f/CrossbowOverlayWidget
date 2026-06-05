using System;
using System.Linq;
using CrossbowOverlayWidget.Enums;
using CrossbowOverlayWidget.Models;
using CrossbowOverlayWidget.Rendering;
using CrossbowOverlayWidget.Services;
using CrossbowOverlayWidget.ViewModels;
using Microsoft.Gaming.XboxGameBar;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CrossbowOverlayWidget
{
    public sealed partial class WidgetPage : Page
    {
        // Services
        private readonly ConfigService _configService = new ConfigService();
        private readonly PresetService _presetService = new PresetService();
        private readonly ProfileService _profileService = new ProfileService();
        private readonly ImportExportService _importExportService = new ImportExportService();
        private readonly ReticleRenderer _renderer = new ReticleRenderer();

        // ViewModels
        private WidgetViewModel _widgetViewModel;
        private SettingsViewModel _settingsVM;
        private AppConfig _config;

        // Animation
        private DispatcherTimer _animTimer;
        private float _animPhase = 0f;
        private bool _hasAnimation = false;

        // UI state
        private bool _isUpdating;
        private bool _settingsVisible = false;
        private XboxGameBarWidget _widget;

        // Debounced auto-save (save to disk after user stops changing)
        private DispatcherTimer _saveDebounceTimer;

        public WidgetPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is XboxGameBarWidget widget)
            {
                _widget = widget;
                _widget.SettingsClicked += Widget_SettingsClicked;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (_widget != null)
                _widget.SettingsClicked -= Widget_SettingsClicked;
            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Game Bar settings button clicked → toggle our embedded settings panel
        /// </summary>
        private void Widget_SettingsClicked(XboxGameBarWidget sender, object args)
        {
            ToggleSettingsPanel();
        }

        // =====================================================================
        // Initialization
        // =====================================================================

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _config = await _configService.LoadAsync();
            _presetService.Initialize(_config);
            _profileService.Initialize(_config);

            // Widget rendering ViewModel
            _widgetViewModel = new WidgetViewModel(_presetService, _configService);
            _presetService.PresetChanged += (s, p) =>
            {
                _widgetViewModel.Refresh();
                UpdateStatusBar();
                EvaluateAnimation();
                ReticleCanvas.Invalidate();
            };
            _widgetViewModel.Refresh();
            UpdateStatusBar();
            EvaluateAnimation();

            // Settings ViewModel
            _settingsVM = new SettingsViewModel(
                _presetService, _profileService, _configService, _importExportService);
            _settingsVM.Initialize(_config);
            _settingsVM.ConfigChanged += (s, ev) =>
            {
                // Immediate visual update on canvas
                _widgetViewModel.Refresh();
                UpdateStatusBar();
                EvaluateAnimation();
                ReticleCanvas.Invalidate();
                // Debounced save to disk
                ScheduleAutoSave();
            };

            // Debounced save: fires 800ms after last change
            _saveDebounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(800)
            };
            _saveDebounceTimer.Tick += async (s, ev) =>
            {
                _saveDebounceTimer.Stop();
                try { await _configService.SaveAsync(_config); }
                catch { }
            };

            BindSettingsUI();
            ReticleCanvas.Invalidate();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            StopAnimation();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ReticleCanvas.Invalidate();
        }

        // =====================================================================
        // Canvas Rendering
        // =====================================================================

        private void OnCanvasDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var cfg = _widgetViewModel?.CurrentConfig;
            if (cfg == null) return;

            float w = (float)sender.ActualWidth;
            float h = (float)sender.ActualHeight;
            _renderer.Draw(args.DrawingSession, cfg, w, h, _animPhase);

            if (_hasAnimation)
            {
                _animPhase += 0.05f;
                if (_animPhase > 1000f) _animPhase = 0f;
            }
        }

        private void EvaluateAnimation()
        {
            var cfg = _widgetViewModel?.CurrentConfig;
            _hasAnimation = cfg != null && cfg.Style != null
                            && cfg.Style.Animation != AnimationType.None;
            if (_hasAnimation) StartAnimation();
            else StopAnimation();
        }

        private void StartAnimation()
        {
            if (_animTimer != null) return;
            _animTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            _animTimer.Tick += (s, e) => ReticleCanvas.Invalidate();
            _animTimer.Start();
        }

        private void StopAnimation()
        {
            _animTimer?.Stop();
            _animTimer = null;
        }

        private void UpdateStatusBar()
        {
            if (_widgetViewModel == null) return;
            ProfileLabel.Text = _widgetViewModel.ActiveProfileName;
            PresetLabel.Text = _widgetViewModel.ActivePresetName;
        }

        // =====================================================================
        // Settings Panel Toggle
        // =====================================================================

        private void ToggleSettingsPanel()
        {
            _settingsVisible = !_settingsVisible;
            SettingsPanel.Visibility = _settingsVisible ? Visibility.Visible : Visibility.Collapsed;
            QuickButtons.Visibility = _settingsVisible ? Visibility.Collapsed : Visibility.Visible;

            if (_settingsVisible)
                BindSettingsUI();
        }

        private void OnToggleSettings(object sender, RoutedEventArgs e) => ToggleSettingsPanel();

        private void OnCloseSettings(object sender, RoutedEventArgs e) => ToggleSettingsPanel();

        // =====================================================================
        // Quick Buttons (always visible when settings hidden)
        // =====================================================================

        private void OnPrevPreset(object sender, RoutedEventArgs e)
        {
            _widgetViewModel?.PreviousPreset();
            UpdateStatusBar();
            EvaluateAnimation();
            ReticleCanvas.Invalidate();
        }

        private void OnNextPreset(object sender, RoutedEventArgs e)
        {
            _widgetViewModel?.NextPreset();
            UpdateStatusBar();
            EvaluateAnimation();
            ReticleCanvas.Invalidate();
        }

        private void OnToggleVisibility(object sender, RoutedEventArgs e)
        {
            _widgetViewModel?.ToggleVisibility();
            ReticleCanvas.Invalidate();
        }

        // =====================================================================
        // Settings UI Binding
        // =====================================================================

        private void BindSettingsUI()
        {
            if (_settingsVM == null) return;
            _isUpdating = true;

            PresetCombo.ItemsSource = _settingsVM.Presets;
            PresetCombo.SelectedItem = _settingsVM.SelectedPreset;
            ProfileCombo.ItemsSource = _settingsVM.Profiles;
            ProfileCombo.SelectedItem = _settingsVM.SelectedProfile;

            ShapeCombo.SelectedIndex = _settingsVM.ShapeIndex;
            AnimationCombo.SelectedIndex = _settingsVM.AnimationIndex;
            AnimSpeedSlider.Value = _settingsVM.AnimationSpeed;

            MajorColorBox.Text = _settingsVM.MajorColor;
            MinorColorBox.Text = _settingsVM.MinorColor;
            TextColorBox.Text = _settingsVM.TextColor;
            CenterColorBox.Text = _settingsVM.CenterLineColor;

            GradientToggle.IsOn = _settingsVM.GradientColor;
            NightModeToggle.IsOn = _settingsVM.NightMode;
            CenterLockToggle.IsOn = _settingsVM.CenterLocked;

            ScaleSlider.Value = _settingsVM.Scale;
            SpacingSlider.Value = _settingsVM.MarkSpacing;
            OpacitySlider.Value = _settingsVM.Opacity;
            MajorWidthSlider.Value = _settingsVM.MajorLineWidth;
            MinorWidthSlider.Value = _settingsVM.MinorLineWidth;
            OffsetXSlider.Value = _settingsVM.OffsetX;
            OffsetYSlider.Value = _settingsVM.OffsetY;
            CenterGapSlider.Value = _settingsVM.CenterGap;
            TaperSlider.Value = _settingsVM.TaperFactor;
            LabelPositionCombo.SelectedIndex = _settingsVM.LabelPositionIndex;

            PopulateCalibrationCombos();
            _isUpdating = false;
        }

        private void RefreshSettingsUI()
        {
            if (_settingsVM == null) return;
            _isUpdating = true;

            ShapeCombo.SelectedIndex = _settingsVM.ShapeIndex;
            AnimationCombo.SelectedIndex = _settingsVM.AnimationIndex;
            AnimSpeedSlider.Value = _settingsVM.AnimationSpeed;

            MajorColorBox.Text = _settingsVM.MajorColor;
            MinorColorBox.Text = _settingsVM.MinorColor;
            TextColorBox.Text = _settingsVM.TextColor;
            CenterColorBox.Text = _settingsVM.CenterLineColor;

            GradientToggle.IsOn = _settingsVM.GradientColor;
            NightModeToggle.IsOn = _settingsVM.NightMode;
            CenterLockToggle.IsOn = _settingsVM.CenterLocked;

            ScaleSlider.Value = _settingsVM.Scale;
            SpacingSlider.Value = _settingsVM.MarkSpacing;
            OpacitySlider.Value = _settingsVM.Opacity;
            MajorWidthSlider.Value = _settingsVM.MajorLineWidth;
            MinorWidthSlider.Value = _settingsVM.MinorLineWidth;
            OffsetXSlider.Value = _settingsVM.OffsetX;
            OffsetYSlider.Value = _settingsVM.OffsetY;
            CenterGapSlider.Value = _settingsVM.CenterGap;
            TaperSlider.Value = _settingsVM.TaperFactor;
            LabelPositionCombo.SelectedIndex = _settingsVM.LabelPositionIndex;

            _isUpdating = false;
        }

        private void PopulateCalibrationCombos()
        {
            var preset = _settingsVM?.SelectedPreset;
            if (preset == null) return;
            Calib1Combo.ItemsSource = preset.Marks;
            Calib2Combo.ItemsSource = preset.Marks;
            if (preset.Marks.Count >= 2)
            {
                Calib1Combo.SelectedIndex = 0;
                Calib2Combo.SelectedIndex = preset.Marks.Count - 1;
            }
        }

        // =====================================================================
        // Settings Handlers
        // =====================================================================

        private void OnPresetSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdating || _settingsVM == null) return;
            if (PresetCombo.SelectedItem is ReticlePreset preset)
            {
                _settingsVM.SelectedPreset = preset;
                RefreshSettingsUI();
                PopulateCalibrationCombos();
            }
        }

        private void OnCreatePreset(object sender, RoutedEventArgs e)
        {
            _settingsVM?.CreateNewPreset();
            PresetCombo.ItemsSource = _settingsVM.Presets;
            ShowStatus(_settingsVM.StatusMessage, false);
        }

        private void OnDuplicatePreset(object sender, RoutedEventArgs e)
        {
            _settingsVM?.DuplicateSelectedPreset();
            PresetCombo.ItemsSource = _settingsVM.Presets;
            PresetCombo.SelectedItem = _settingsVM.SelectedPreset;
            ShowStatus(_settingsVM.StatusMessage, false);
        }

        private void OnDeletePreset(object sender, RoutedEventArgs e)
        {
            _settingsVM?.DeleteSelectedPreset();
            PresetCombo.ItemsSource = _settingsVM.Presets;
            PresetCombo.SelectedItem = _settingsVM.SelectedPreset;
            RefreshSettingsUI();
            ShowStatus(_settingsVM.StatusMessage, false);
        }

        private void OnShapeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdating || _settingsVM == null) return;
            _settingsVM.ShapeIndex = ShapeCombo.SelectedIndex;
        }

        private void OnAnimationChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdating || _settingsVM == null) return;
            _settingsVM.AnimationIndex = AnimationCombo.SelectedIndex;
        }

        private void OnAnimSpeedChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_isUpdating || _settingsVM == null) return;
            _settingsVM.AnimationSpeed = AnimSpeedSlider.Value;
        }

        private void OnColorChanged(object sender, RoutedEventArgs e)
        {
            if (_isUpdating || _settingsVM == null) return;
            _settingsVM.MajorColor = MajorColorBox.Text;
            _settingsVM.MinorColor = MinorColorBox.Text;
            _settingsVM.TextColor = TextColorBox.Text;
            _settingsVM.CenterLineColor = CenterColorBox.Text;
        }

        private void OnSliderChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_isUpdating || _settingsVM == null) return;
            _settingsVM.Scale = ScaleSlider.Value;
            _settingsVM.MarkSpacing = SpacingSlider.Value;
            _settingsVM.Opacity = OpacitySlider.Value;
            _settingsVM.MajorLineWidth = MajorWidthSlider.Value;
            _settingsVM.MinorLineWidth = MinorWidthSlider.Value;
            _settingsVM.OffsetX = OffsetXSlider.Value;
            _settingsVM.OffsetY = OffsetYSlider.Value;
            _settingsVM.CenterGap = CenterGapSlider.Value;
            _settingsVM.TaperFactor = TaperSlider.Value;
        }

        private void OnGradientToggled(object sender, RoutedEventArgs e)
        {
            if (_isUpdating || _settingsVM == null) return;
            _settingsVM.GradientColor = GradientToggle.IsOn;
        }

        private void OnNightModeToggled(object sender, RoutedEventArgs e)
        {
            if (_isUpdating || _settingsVM == null) return;
            _settingsVM.NightMode = NightModeToggle.IsOn;
        }

        private void OnCenterLockToggled(object sender, RoutedEventArgs e)
        {
            if (_isUpdating || _settingsVM == null) return;
            _settingsVM.CenterLocked = CenterLockToggle.IsOn;
        }

        private void OnLabelPositionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdating || _settingsVM == null) return;
            _settingsVM.LabelPositionIndex = LabelPositionCombo.SelectedIndex;
        }

        // =====================================================================
        // Calibration
        // =====================================================================

        private void OnCalibrateClick(object sender, RoutedEventArgs e)
        {
            int idx1 = Calib1Combo.SelectedIndex;
            int idx2 = Calib2Combo.SelectedIndex;
            if (idx1 < 0 || idx2 < 0 || idx1 == idx2)
            {
                ShowStatus("请选择2个不同的校准点", true);
                return;
            }
            if (!int.TryParse(RangeMinBox.Text, out int rangeMin) ||
                !int.TryParse(RangeMaxBox.Text, out int rangeMax) ||
                !int.TryParse(RangeStepBox.Text, out int step) ||
                step <= 0 || rangeMin >= rangeMax)
            {
                ShowStatus("范围/间距参数无效", true);
                return;
            }
            _settingsVM.CalibPoint1Index = idx1;
            _settingsVM.CalibPoint2Index = idx2;
            _settingsVM.RangeMin = rangeMin;
            _settingsVM.RangeMax = rangeMax;
            _settingsVM.RangeStep = step;
            _settingsVM.ExecuteCalibration();
            PopulateCalibrationCombos();
            ShowStatus(_settingsVM.StatusMessage, false);
        }

        private async void OnRestoreClick(object sender, RoutedEventArgs e)
        {
            await _settingsVM.RestoreDefaults();
            RefreshSettingsUI();
            PopulateCalibrationCombos();
            ShowStatus(_settingsVM.StatusMessage, false);
        }

        private async void OnSaveClick(object sender, RoutedEventArgs e)
        {
            await _settingsVM.SaveConfig();
            ShowStatus(_settingsVM.StatusMessage, false);
        }

        // =====================================================================
        // Profile
        // =====================================================================

        private void OnProfileSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdating || _settingsVM == null) return;
            if (ProfileCombo.SelectedItem is GameProfile profile)
                _settingsVM.SelectedProfile = profile;
        }

        private void OnCreateProfile(object sender, RoutedEventArgs e)
        {
            _settingsVM?.CreateNewProfile();
            ProfileCombo.ItemsSource = _settingsVM.Profiles;
            ShowStatus(_settingsVM.StatusMessage, false);
        }

        private void OnSwitchProfile(object sender, RoutedEventArgs e)
        {
            _settingsVM?.SwitchProfile();
            RefreshSettingsUI();
            PopulateCalibrationCombos();
            ShowStatus(_settingsVM.StatusMessage, false);
        }

        private void OnDeleteProfile(object sender, RoutedEventArgs e)
        {
            _settingsVM?.DeleteSelectedProfile();
            ProfileCombo.ItemsSource = _settingsVM.Profiles;
            ProfileCombo.SelectedItem = _settingsVM.SelectedProfile;
            ShowStatus(_settingsVM.StatusMessage, false);
        }

        // =====================================================================
        // Import/Export
        // =====================================================================

        private async void OnExportClick(object sender, RoutedEventArgs e)
        {
            await _settingsVM.ExportConfig();
            ShowStatus(_settingsVM.StatusMessage, false);
        }

        private async void OnImportClick(object sender, RoutedEventArgs e)
        {
            await _settingsVM.ImportConfig();
            PresetCombo.ItemsSource = _settingsVM.Presets;
            PresetCombo.SelectedItem = _settingsVM.SelectedPreset;
            ProfileCombo.ItemsSource = _settingsVM.Profiles;
            ProfileCombo.SelectedItem = _settingsVM.SelectedProfile;
            RefreshSettingsUI();
            PopulateCalibrationCombos();
            ShowStatus(_settingsVM.StatusMessage, false);
        }

        // =====================================================================
        // Status Toast
        // =====================================================================

        private void ShowStatus(string message, bool isError)
        {
            StatusText.Text = message;
            StatusText.Foreground = new SolidColorBrush(
                isError ? Colors.IndianRed : Colors.LightGreen);
            StatusToast.Visibility = Visibility.Visible;
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            timer.Tick += (s, e) =>
            {
                StatusToast.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }

        // =====================================================================
        // Debounced Auto-Save
        // =====================================================================

        private void ScheduleAutoSave()
        {
            _saveDebounceTimer.Stop();
            _saveDebounceTimer.Start();
        }
    }
}
