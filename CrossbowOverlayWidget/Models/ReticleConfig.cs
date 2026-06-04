using System.Collections.Generic;

namespace CrossbowOverlayWidget.Models
{
    public class ReticleConfig
    {
        public double OffsetX { get; set; } = 0;
        public double OffsetY { get; set; } = 0;
        public double Scale { get; set; } = 1.0;
        public double MarkSpacing { get; set; } = 50;
        public bool IsVisible { get; set; } = true;
        public bool ShowStatus { get; set; } = true;
        public bool CenterLocked { get; set; } = true;
        public bool DiamondVisible { get; set; } = true;
        public int SelectedIndex { get; set; } = -1;
        public int CalibrationRangeMin { get; set; } = 0;
        public int CalibrationRangeMax { get; set; } = 350;
        public int CalibrationStep { get; set; } = 50;
        public List<DistanceMark> Marks { get; set; } = new List<DistanceMark>
        {
            new DistanceMark { Distance = 50, Label = "50m", IsMajor = true },
            new DistanceMark { Distance = 100, Label = "100m", IsMajor = true },
            new DistanceMark { Distance = 150, Label = "150m", IsMajor = false },
            new DistanceMark { Distance = 200, Label = "200m", IsMajor = true },
            new DistanceMark { Distance = 250, Label = "250m", IsMajor = false },
            new DistanceMark { Distance = 300, Label = "300m", IsMajor = true },
        };
        public ReticleStyle Style { get; set; } = new ReticleStyle();
    }
}
