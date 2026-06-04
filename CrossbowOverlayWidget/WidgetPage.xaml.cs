using System;
using CrossbowOverlayWidget.Enums;
using CrossbowOverlayWidget.Models;
using CrossbowOverlayWidget.Rendering;
using CrossbowOverlayWidget.Services;
using CrossbowOverlayWidget.ViewModels;
using Microsoft.Gaming.XboxGameBar;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CrossbowOverlayWidget
{
    public sealed partial class WidgetPage : Page
    {
        private readonly ConfigService _configService = new ConfigService();
        private readonly PresetService _presetService = new PresetService();
        private readonly ReticleRenderer _renderer = new ReticleRenderer();
        private WidgetViewModel _viewModel;

        private DispatcherTimer _animTimer;
        private float _animPhase = 0f;
        private bool _hasAnimation = false;

        // Game Bar widget reference for settings activation
        private XboxGameBarWidget _widget;

        public WidgetPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Receive the XboxGameBarWidget from App.xaml.cs and hook up settings
            if (e.Parameter is XboxGameBarWidget widget)
            {
                _widget = widget;
                _widget.SettingsClicked += Widget_SettingsClicked;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (_widget != null)
            {
                _widget.SettingsClicked -= Widget_SettingsClicked;
            }
            base.OnNavigatedFrom(e);
        }

        private async void Widget_SettingsClicked(XboxGameBarWidget sender, object args)
        {
            // This activates the settings widget (declared in manifest as CrossbowOverlaySettings)
            await sender.ActivateSettingsAsync();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Load config and initialize services
            var config = await _configService.LoadAsync();
            _presetService.Initialize(config);

            _viewModel = new WidgetViewModel(_presetService, _configService);

            // Subscribe to preset changes for animation re-evaluation
            _presetService.PresetChanged += (s, p) =>
            {
                _viewModel.Refresh();
                UpdateStatusBar();
                EvaluateAnimation();
                ReticleCanvas.Invalidate();
            };

            _viewModel.Refresh();
            UpdateStatusBar();
            EvaluateAnimation();
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

        private void OnCanvasDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var cfg = _viewModel?.CurrentConfig;
            if (cfg == null) return;

            float w = (float)sender.ActualWidth;
            float h = (float)sender.ActualHeight;

            _renderer.Draw(args.DrawingSession, cfg, w, h, _animPhase);

            // Advance animation phase
            if (_hasAnimation)
            {
                _animPhase += 0.05f;
                if (_animPhase > 1000f) _animPhase = 0f;
            }
        }

        private void EvaluateAnimation()
        {
            var cfg = _viewModel?.CurrentConfig;
            _hasAnimation = cfg != null && cfg.Style != null
                            && cfg.Style.Animation != AnimationType.None;

            if (_hasAnimation)
                StartAnimation();
            else
                StopAnimation();
        }

        private void StartAnimation()
        {
            if (_animTimer != null) return;
            _animTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60fps
            };
            _animTimer.Tick += (s, e) => ReticleCanvas.Invalidate();
            _animTimer.Start();
        }

        private void StopAnimation()
        {
            if (_animTimer != null)
            {
                _animTimer.Stop();
                _animTimer = null;
            }
        }

        private void UpdateStatusBar()
        {
            if (_viewModel == null) return;
            ProfileLabel.Text = _viewModel.ActiveProfileName;
            PresetLabel.Text = _viewModel.ActivePresetName;
        }

        // Bottom button handlers
        private void OnPrevPreset(object sender, RoutedEventArgs e)
        {
            _viewModel?.PreviousPreset();
            UpdateStatusBar();
            EvaluateAnimation();
            ReticleCanvas.Invalidate();
        }

        private void OnNextPreset(object sender, RoutedEventArgs e)
        {
            _viewModel?.NextPreset();
            UpdateStatusBar();
            EvaluateAnimation();
            ReticleCanvas.Invalidate();
        }

        private void OnToggleVisibility(object sender, RoutedEventArgs e)
        {
            _viewModel?.ToggleVisibility();
            ReticleCanvas.Invalidate();
        }

        // Public method for SettingsPage to notify config changes
        public void RefreshFromSettings()
        {
            _viewModel?.Refresh();
            UpdateStatusBar();
            EvaluateAnimation();
            ReticleCanvas.Invalidate();
        }
    }
}
