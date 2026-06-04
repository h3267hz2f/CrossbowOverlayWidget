namespace CrossbowOverlayWidget.Models
{
    public class DistanceMark
    {
        public int Distance { get; set; }
        public string Label { get; set; } = "";
        public bool IsMajor { get; set; }
        public double OffsetY { get; set; } = 0;
    }
}
