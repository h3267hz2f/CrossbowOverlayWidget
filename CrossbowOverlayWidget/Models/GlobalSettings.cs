namespace CrossbowOverlayWidget.Models
{
    public class GlobalSettings
    {
        public bool AutoDetectGame { get; set; } = true;
        public bool ShowStatusText { get; set; } = true;
        public string Hotkey_ToggleVisibility { get; set; } = "F9";
        public string Hotkey_NextPreset { get; set; } = "F10";
        public string Hotkey_Calibrate { get; set; } = "F11";
    }
}
