using System;
using CrossbowOverlayWidget.Enums;
using CrossbowOverlayWidget.Models;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Windows.UI;
using Windows.UI.Text;

namespace CrossbowOverlayWidget.Rendering
{
    public class ReticleRenderer
    {
        private readonly RendererFactory _factory = new RendererFactory();
        private CanvasStrokeStyle _dashStyle;

        public ReticleRenderer()
        {
            _dashStyle = new CanvasStrokeStyle
            {
                DashStyle = CanvasDashStyle.Dash,
                DashCap = CanvasCapStyle.Round
            };
        }

        public void Draw(CanvasDrawingSession ds, ReticleConfig config,
                          float canvasWidth, float canvasHeight, float animationPhase)
        {
            ds.Antialiasing = CanvasAntialiasing.Antialiased;
            if (!config.IsVisible || config.Marks.Count == 0) return;

            float scale = (float)config.Scale;
            float spacing = (float)(config.MarkSpacing * config.Scale);
            var style = config.Style;

            // Animation
            float animScale = 1.0f;
            float animOpacity = (float)style.OverallOpacity;
            switch (style.Animation)
            {
                case AnimationType.Pulse:
                    animScale = 1.0f + 0.05f * (float)Math.Sin(animationPhase * (float)style.AnimationSpeed);
                    break;
                case AnimationType.Breathing:
                    animOpacity *= 0.7f + 0.3f * ((float)Math.Sin(animationPhase * (float)style.AnimationSpeed) + 1) / 2;
                    break;
                case AnimationType.FadeIn:
                    animOpacity *= Math.Min(1.0f, animationPhase * 0.5f);
                    break;
            }

            float baseX = canvasWidth / 2 + (float)config.OffsetX;
            float baseY = canvasHeight / 2 + (float)config.OffsetY;

            int centerIdx = config.Marks.FindIndex(m => m.Distance == 200);
            if (centerIdx < 0) centerIdx = config.Marks.Count / 2;

            // Dashed center line
            if (config.Marks.Count >= 2)
            {
                float topOff = (float)(config.Marks[0].OffsetY * scale);
                float botOff = (float)(config.Marks[config.Marks.Count - 1].OffsetY * scale);
                float topY = baseY - centerIdx * spacing + topOff;
                float botY = baseY + (config.Marks.Count - 1 - centerIdx) * spacing + botOff;

                var centerColor = ColorHelper.WithOpacity(
                    ColorHelper.ParseHex(style.CenterLineColor), animOpacity);
                ds.DrawLine(baseX, topY - spacing * 0.3f,
                            baseX, botY + spacing * 0.3f,
                            centerColor, 1.0f * scale, _dashStyle);
            }

            // Draw marks via shape-specific renderer
            var shapeRenderer = _factory.GetRenderer(style.Shape);
            for (int i = 0; i < config.Marks.Count; i++)
            {
                var mark = config.Marks[i];
                float y = baseY + (i - centerIdx) * spacing
                        + (float)(mark.OffsetY * scale) * animScale;
                bool isMajor = mark.IsMajor;
                bool isSelected = (i == config.SelectedIndex);

                shapeRenderer.DrawMark(ds, baseX, y, isMajor, isSelected,
                                       style, scale * animScale, animOpacity);
                DrawLabel(ds, mark.Label, baseX, y, isSelected, isMajor,
                          style, scale, animOpacity);
            }

            // Center reticle
            if (style.ShowCenterDiamond && config.DiamondVisible)
            {
                shapeRenderer.DrawCenterReticle(ds, baseX, baseY, style, scale, animOpacity);
            }

            // Status text (top-left)
            if (config.ShowStatus)
                DrawStatusText(ds, config, animOpacity);
        }

        private void DrawLabel(CanvasDrawingSession ds, string label,
                                float baseX, float y, bool isSelected, bool isMajor,
                                ReticleStyle style, float scale, float opacity)
        {
            float tickLen = (float)((isMajor ? style.MajorTickLength : style.MinorTickLength) * scale);
            float fontSize = (float)(style.FontSize * scale * (isSelected ? 1.2 : 1.0));

            string text = isSelected ? $">> {label} <<" : label;
            var color = ColorHelper.WithOpacity(
                ColorHelper.ParseHex(isSelected ? "#CCFFFF00" : style.TextColor), opacity);

            var format = new Microsoft.Graphics.Canvas.Text.CanvasTextFormat
            {
                FontFamily = "Consolas",
                FontSize = fontSize,
                FontWeight = (isMajor || isSelected) ? FontWeight.Bold : FontWeight.Normal
            };

            ds.DrawText(text, baseX + tickLen / 2 + 4 * scale, y - fontSize / 2, color, format);
        }

        private void DrawStatusText(CanvasDrawingSession ds, ReticleConfig config, float opacity)
        {
            string status = $"[CrossbowOverlay] Scale:{config.Scale:F2} Spacing:{(int)config.MarkSpacing}px\n"
                          + $"Offset: ({(int)config.OffsetX}, {(int)config.OffsetY})";

            if (config.SelectedIndex >= 0 && config.SelectedIndex < config.Marks.Count)
            {
                var sm = config.Marks[config.SelectedIndex];
                status += $"\n>> Selected: {sm.Label} (OffsetY: {sm.OffsetY:F1})";
            }

            var color = ColorHelper.WithOpacity(ColorHelper.ParseHex("#AAFFFFFF"), opacity);
            var format = new Microsoft.Graphics.Canvas.Text.CanvasTextFormat
            {
                FontFamily = "Consolas",
                FontSize = 11,
                FontWeight = FontWeight.Normal
            };
            ds.DrawText(status, 10, 10, color, format);
        }
    }
}
