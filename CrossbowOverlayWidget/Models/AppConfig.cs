using System;
using System.Collections.Generic;

namespace CrossbowOverlayWidget.Models
{
    public class AppConfig
    {
        public int SchemaVersion { get; set; } = 1;
        public string ActiveProfileId { get; set; } = "";
        public List<GameProfile> Profiles { get; set; } = new List<GameProfile>();
        public List<ReticlePreset> Presets { get; set; } = new List<ReticlePreset>();
        public GlobalSettings Settings { get; set; } = new GlobalSettings();

        public static AppConfig CreateDefault()
        {
            var defaultPreset = new ReticlePreset { Name = "经典十字" };

            // Military Mil-Dot preset — diamond shape, finer marks, high contrast
            var milDot = new ReticlePreset
            {
                Name = "军用密位",
                Scale = 1.2,
                MarkSpacing = 40,
                Style = new ReticleStyle
                {
                    Shape = Enums.ReticleShape.Diamond,
                    MajorColor = "#DDFF4400",
                    MinorColor = "#99FF8800",
                    CenterLineColor = "#CCFF0000",
                    TextColor = "#DDFFFFFF",
                    MajorLineWidth = 2.5,
                    MinorLineWidth = 1.0,
                    ShowCenterDiamond = true,
                    DiamondSize = 6,
                    CenterGap = 8,
                    TaperFactor = 0.3
                },
                Marks = new System.Collections.Generic.List<DistanceMark>
                {
                    new DistanceMark { Distance = 25,  Label = "25",  IsMajor = false },
                    new DistanceMark { Distance = 50,  Label = "50",  IsMajor = true },
                    new DistanceMark { Distance = 75,  Label = "75",  IsMajor = false },
                    new DistanceMark { Distance = 100, Label = "100", IsMajor = true },
                    new DistanceMark { Distance = 150, Label = "150", IsMajor = false },
                    new DistanceMark { Distance = 200, Label = "200", IsMajor = true },
                    new DistanceMark { Distance = 250, Label = "250", IsMajor = false },
                    new DistanceMark { Distance = 300, Label = "300", IsMajor = true },
                    new DistanceMark { Distance = 350, Label = "350", IsMajor = false },
                    new DistanceMark { Distance = 400, Label = "400", IsMajor = true },
                }
            };

            // Quick Scope preset — dot shape, minimal, bright
            var quickScope = new ReticlePreset
            {
                Name = "竞技快瞄",
                Scale = 0.8,
                MarkSpacing = 80,
                Style = new ReticleStyle
                {
                    Shape = Enums.ReticleShape.Dot,
                    MajorColor = "#EE00FF44",
                    MinorColor = "#8800FF44",
                    CenterLineColor = "#00000000",
                    TextColor = "#CCFFFFFF",
                    DotRadius = 4,
                    ShowCenterDiamond = false,
                    Animation = Enums.AnimationType.Pulse,
                    AnimationSpeed = 2.0,
                    OverallOpacity = 0.9
                },
                Marks = new System.Collections.Generic.List<DistanceMark>
                {
                    new DistanceMark { Distance = 50,  Label = "50",  IsMajor = true },
                    new DistanceMark { Distance = 100, Label = "100", IsMajor = true },
                    new DistanceMark { Distance = 200, Label = "200", IsMajor = true },
                }
            };

            // Long Range Sniper preset — cross with many fine marks
            var sniper = new ReticlePreset
            {
                Name = "远程狙击",
                Scale = 1.5,
                MarkSpacing = 30,
                Style = new ReticleStyle
                {
                    Shape = Enums.ReticleShape.Cross,
                    MajorColor = "#CCFF0000",
                    MinorColor = "#66FF0000",
                    CenterLineColor = "#AAFF0000",
                    TextColor = "#CCFF8888",
                    MajorLineWidth = 1.5,
                    MinorLineWidth = 0.5,
                    MajorTickLength = 25,
                    MinorTickLength = 10,
                    FontSize = 10,
                    ShowCenterDiamond = true,
                    CenterGap = 12,
                    TaperFactor = 0.5
                },
                Marks = new System.Collections.Generic.List<DistanceMark>
                {
                    new DistanceMark { Distance = 50,  Label = "50m",  IsMajor = false },
                    new DistanceMark { Distance = 100, Label = "100m", IsMajor = true },
                    new DistanceMark { Distance = 150, Label = "150m", IsMajor = false },
                    new DistanceMark { Distance = 200, Label = "200m", IsMajor = true },
                    new DistanceMark { Distance = 250, Label = "250m", IsMajor = false },
                    new DistanceMark { Distance = 300, Label = "300m", IsMajor = true },
                    new DistanceMark { Distance = 400, Label = "400m", IsMajor = true },
                    new DistanceMark { Distance = 500, Label = "500m", IsMajor = true },
                    new DistanceMark { Distance = 600, Label = "600m", IsMajor = true },
                    new DistanceMark { Distance = 800, Label = "800m", IsMajor = true },
                }
            };

            // T-Shape Tactical — horizontal emphasis
            var tShape = new ReticlePreset
            {
                Name = "T形战术",
                Scale = 1.0,
                MarkSpacing = 50,
                Style = new ReticleStyle
                {
                    Shape = Enums.ReticleShape.TShape,
                    MajorColor = "#CC00CCFF",
                    MinorColor = "#6600CCFF",
                    CenterLineColor = "#AA00CCFF",
                    TextColor = "#BBCCFFFF",
                    MajorLineWidth = 2.0,
                    MinorLineWidth = 1.0,
                    ShowCenterDiamond = false,
                    TextPosition = Enums.LabelPosition.Alternate
                },
                Marks = new System.Collections.Generic.List<DistanceMark>
                {
                    new DistanceMark { Distance = 50,  Label = "50m",  IsMajor = true },
                    new DistanceMark { Distance = 100, Label = "100m", IsMajor = true },
                    new DistanceMark { Distance = 150, Label = "150m", IsMajor = false },
                    new DistanceMark { Distance = 200, Label = "200m", IsMajor = true },
                    new DistanceMark { Distance = 250, Label = "250m", IsMajor = false },
                    new DistanceMark { Distance = 300, Label = "300m", IsMajor = true },
                    new DistanceMark { Distance = 400, Label = "400m", IsMajor = true },
                }
            };

            var defaultProfile = new GameProfile
            {
                Name = "和平精英",
                ActivePresetId = defaultPreset.Id
            };
            return new AppConfig
            {
                ActiveProfileId = defaultProfile.Id,
                Profiles = new System.Collections.Generic.List<GameProfile> { defaultProfile },
                Presets = new System.Collections.Generic.List<ReticlePreset>
                {
                    defaultPreset, milDot, quickScope, sniper, tShape
                }
            };
        }
    }
}
