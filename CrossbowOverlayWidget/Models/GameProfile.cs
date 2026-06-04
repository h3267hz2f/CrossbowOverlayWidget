using System;

namespace CrossbowOverlayWidget.Models
{
    public class GameProfile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = "和平精英";
        public string GameExecutable { get; set; } = "Gameloop.exe";
        public string ActivePresetId { get; set; } = "";
        public double OffsetX { get; set; } = 0;
        public double OffsetY { get; set; } = 0;
        public DateTime LastUsed { get; set; } = DateTime.UtcNow;
    }
}
