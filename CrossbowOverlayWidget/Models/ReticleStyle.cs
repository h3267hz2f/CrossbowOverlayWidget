using CrossbowOverlayWidget.Enums;

namespace CrossbowOverlayWidget.Models
{
    public class ReticleStyle
    {
        public string MajorColor { get; set; } = "#CC00FF00";
        public string MinorColor { get; set; } = "#8800FF00";
        public string TextColor { get; set; } = "#CCFFFFFF";
        public string CenterLineColor { get; set; } = "#CCFF0000";
        public double MajorLineWidth { get; set; } = 2.0;
        public double MinorLineWidth { get; set; } = 1.0;
        public double MajorTickLength { get; set; } = 30;
        public double MinorTickLength { get; set; } = 15;
        public double FontSize { get; set; } = 12;
        public bool ShowCenterDiamond { get; set; } = true;

        // Extended properties
        public ReticleShape Shape { get; set; } = ReticleShape.Cross;
        public double OverallOpacity { get; set; } = 1.0;
        public AnimationType Animation { get; set; } = AnimationType.None;
        public double AnimationSpeed { get; set; } = 1.0;
        public double DotRadius { get; set; } = 3.0;
        public double DiamondSize { get; set; } = 8.0;
        public double CircleRadius { get; set; } = 20.0;
    }
}
