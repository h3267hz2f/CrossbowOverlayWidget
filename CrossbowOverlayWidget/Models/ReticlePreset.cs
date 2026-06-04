using System;
using System.Collections.Generic;

namespace CrossbowOverlayWidget.Models
{
    public class ReticlePreset
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = "Default";
        public string Description { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ReticleStyle Style { get; set; } = new ReticleStyle();
        public double Scale { get; set; } = 1.0;
        public double MarkSpacing { get; set; } = 50;
        public List<DistanceMark> Marks { get; set; } = new List<DistanceMark>
        {
            new DistanceMark { Distance = 50, Label = "50m", IsMajor = true },
            new DistanceMark { Distance = 100, Label = "100m", IsMajor = true },
            new DistanceMark { Distance = 150, Label = "150m", IsMajor = false },
            new DistanceMark { Distance = 200, Label = "200m", IsMajor = true },
            new DistanceMark { Distance = 250, Label = "250m", IsMajor = false },
            new DistanceMark { Distance = 300, Label = "300m", IsMajor = true },
        };
    }
}
