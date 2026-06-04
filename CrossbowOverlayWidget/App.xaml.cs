using System;
using System.Diagnostics;
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

        public static XboxGameBarWidget MainWidget => ((App)Current)?._widget;
        public static XboxGameBarWidget SettingsWidget => ((App)Current)?._settingsWidget;

        public App()
        {
            this.InitializeComponent();
            this.UnhandledException += OnUnhandledException;
        }

        private void OnUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"[CrossbowOverlay] Unhandled: {e.Message}");
            e.Handled = true;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Widget activation comes through OnActivated, not OnLaunched
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            XboxGameBarWidgetActivatedEventArgs widgetArgs = null;

            if (args.Kind == ActivationKind.Protocol)
            {
                var protocolArgs = args as IProtocolActivatedEventArgs;
                if (protocolArgs != null && protocolArgs.Uri.Scheme == "ms-gamebarwidget")
                {
                    widgetArgs = args as XboxGameBarWidgetActivatedEventArgs;
                }
            }

            if (widgetArgs == null)
            {
                Debug.WriteLine("[CrossbowOverlay] Non-widget activation, ignoring.");
                return;
            }

            Debug.WriteLine($"[CrossbowOverlay] Activated: ExtId={widgetArgs.AppExtensionId}, IsLaunch={widgetArgs.IsLaunchActivation}");

            // Each widget (main + settings) runs in its own process with IsLaunchActivation = true
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
                    // Pass widget reference to WidgetPage via Navigate parameter
                    rootFrame.Navigate(typeof(WidgetPage), _widget);
                }

                Window.Current.Activate();
            }
            else
            {
                // Fallback: same-process activation (shouldn't normally happen per Game Bar docs)
                Debug.WriteLine($"[CrossbowOverlay] Non-launch activation received for {widgetArgs.AppExtensionId}");
                var rootFrame = Window.Current.Content as Frame ?? new Frame();
                Window.Current.Content = rootFrame;

                if (widgetArgs.AppExtensionId == "CrossbowOverlaySettings")
                {
                    if (_settingsWidget == null)
                        _settingsWidget = new XboxGameBarWidget(widgetArgs, Window.Current.CoreWindow, rootFrame);
                    rootFrame.Navigate(typeof(SettingsPage));
                }
                else
                {
                    if (_widget == null)
                        _widget = new XboxGameBarWidget(widgetArgs, Window.Current.CoreWindow, rootFrame);
                    rootFrame.Navigate(typeof(WidgetPage), _widget);
                }

                Window.Current.Activate();
            }
        }
    }
}
