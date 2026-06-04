using System;
using Microsoft.Gaming.XboxGameBar;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CrossbowOverlayWidget
{
    sealed partial class App : Application
    {
        private XboxGameBarWidget _widget = null;
        private XboxGameBarWidget _settingsWidget = null;

        // Shared widget reference for cross-page communication
        public static XboxGameBarWidget MainWidget => ((App)Current)._widget;
        public static XboxGameBarWidget SettingsWidget => ((App)Current)._settingsWidget;

        public App()
        {
            this.InitializeComponent();
            this.UnhandledException += OnUnhandledException;
        }

        private void OnUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // Prevent crash on Game Bar API quirks
            System.Diagnostics.Debug.WriteLine($"[CrossbowOverlay] Unhandled: {e.Message}");
            e.Handled = true;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Normal launch - widget activation comes through OnActivated
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            XboxGameBarWidgetActivatedEventArgs widgetArgs = null;

            if (args.Kind == ActivationKind.Protocol)
            {
                var protocolArgs = args as IProtocolActivatedEventArgs;
                if (protocolArgs != null && protocolArgs.Uri.SchemeName == "ms-gamebarwidget")
                {
                    widgetArgs = args as XboxGameBarWidgetActivatedEventArgs;
                }
            }

            if (widgetArgs != null)
            {
                if (widgetArgs.IsLaunchActivation)
                {
                    var rootFrame = new Frame();
                    Window.Current.Content = rootFrame;

                    if (widgetArgs.AppExtensionId == "CrossbowOverlaySettings")
                    {
                        _settingsWidget = new XboxGameBarWidget(widgetArgs, Window.Current.CoreWindow, rootFrame);
                        rootFrame.Navigate(typeof(SettingsPage));
                    }
                    else
                    {
                        _widget = new XboxGameBarWidget(widgetArgs, Window.Current.CoreWindow, rootFrame);
                        rootFrame.Navigate(typeof(WidgetPage));
                    }

                    Window.Current.Activate();
                }
            }
        }
    }
}
