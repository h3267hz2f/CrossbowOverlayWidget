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

        public static XboxGameBarWidget MainWidget => ((App)Current)?._widget;

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

            if (widgetArgs.IsLaunchActivation)
            {
                var rootFrame = new Frame();
                Window.Current.Content = rootFrame;
                _widget = new XboxGameBarWidget(widgetArgs, Window.Current.CoreWindow, rootFrame);
                rootFrame.Navigate(typeof(WidgetPage), _widget);
                Window.Current.Activate();
            }
        }
    }
}
