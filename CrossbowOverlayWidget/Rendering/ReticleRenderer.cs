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

            // v2: Center locked — center always at canvas midpoint
            float baseX, baseY;
            if (config.CenterLocked)
            {
                baseX = canvasWidth / 2;
                baseY = canvasHeight / 2;
            }
            else
            {
                baseX = canvasWidth / 2 + (float)config.OffsetX;
                baseY = canvasHeight / 2 + (float)config.OffsetY;
            }

            int centerIdx = config.Marks.FindIndex(m => m.Distance == 200);
            if (centerIdx < 0) centerIdx = config.Marks.Count / 2;

            // v2: Center gap
            float gapHalf = (float)(style.CenterGap * scale / 2);
            int totalMarks = config.Marks.Count;

            // v2: Gradient colors (precomputed)
            Color nearColor = ColorHelper.ParseHex(style.NearColor);
            Color farColor = ColorHelper.ParseHex(style.FarColor);

            // v2: Night mode factor
            float nightDim = style.NightMode ? (float)style.NightDimFactor : 1.0f;

            // Dashed center line (split when gap > 0)
            if (config.Marks.Count >= 2)
            {
                float topOff = (float)(config.Marks[0].OffsetY * scale);
                float botOff = (float)(config.Marks[config.Marks.Count - 1].OffsetY * scale);
                float topY = ComputeMarkY(baseY, 0, centerIdx, spacing, gapHalf, topOff, scale, animScale);
                float botY = ComputeMarkY(baseY, config.Marks.Count - 1, centerIdx, spacing, gapHalf, botOff, scale, animScale);

                var centerColor = ApplyNight(ColorHelper.WithOpacity(
                    ColorHelper.ParseHex(style.CenterLineColor), animOpacity), nightDim);

                if (gapHalf > 0.5f)
                {
                    // Two segments with gap in center
                    ds.DrawLine(baseX, topY - spacing * 0.3f, baseX, baseY - gapHalf,
                                centerColor, 1.0f * scale, _dashStyle);
                    ds.DrawLine(baseX, baseY + gapHalf, baseX, botY + spacing * 0.3f,
                                centerColor, 1.0f * scale, _dashStyle);
                }
                else
                {
                    ds.DrawLine(baseX, topY - spacing * 0.3f, baseX, botY + spacing * 0.3f,
                                centerColor, 1.0f * scale, _dashStyle);
                }
            }

            // Draw marks via shape-specific renderer
            var shapeRenderer = _factory.GetRenderer(style.Shape);
            for (int i = 0; i < config.Marks.Count; i++)
            {
                var mark = config.Marks[i];
                float y = ComputeMarkY(baseY, i, centerIdx, spacing, gapHalf,
                                        (float)(mark.OffsetY * scale), scale, animScale);
                bool isMajor = mark.IsMajor;
                bool isSelected = (i == config.SelectedIndex);

                // v2: Gradient color override
                Color? colorOverride = null;
                if (style.GradientColor && totalMarks > 1)
                {
                    float t = (float)i / (totalMarks - 1);
                    colorOverride = ColorHelper.InterpolateGradient(nearColor, farColor, t);
                }

                // v2: Taper factor per mark (center marks thicker)
                float distFromCenter = Math.Abs(i - centerIdx);
                float maxDist = Math.Max(centerIdx, totalMarks - 1 - centerIdx);
                float taperForMark = maxDist > 0
                    ? (float)style.TaperFactor * (1f - distFromCenter / maxDist)
                    : 0f;

                // v2: Night mode applied via colorOverride
                if (style.NightMode && colorOverride.HasValue)
                    colorOverride = ApplyNight(colorOverride.Value, nightDim);

                shapeRenderer.DrawMark(ds, baseX, y, isMajor, isSelected,
                                       style, scale * animScale, animOpacity,
                                       colorOverride, taperForMark);
                DrawLabel(ds, mark.Label, baseX, y, isSelected, isMajor,
                          style, scale, animOpacity, i, centerIdx, nightDim);
            }

            // Center reticle (always at baseX, baseY)
            if (style.ShowCenterDiamond && config.DiamondVisible)
            {
                shapeRenderer.DrawCenterReticle(ds, baseX, baseY, style, scale, animOpacity);
            }

            // Status text (top-left)
            if (config.ShowStatus)
                DrawStatusText(ds, config, animOpacity, nightDim);
        }

        /// <summary>
        /// Compute Y position for a mark, accounting for center gap.
        /// </summary>
        private float ComputeMarkY(float baseY, int i, int centerIdx, float spacing,
                                    float gapHalf, float offsetY, float scale, float animScale)
        {
            float rawY = (i - centerIdx) * spacing + offsetY * animScale;
            if (gapHalf > 0.5f)
            {
                // Push marks away from center
                if (rawY >= 0) return baseY + gapHalf + rawY;
                else return baseY - gapHalf + rawY;
            }
            return baseY + rawY;
        }

        private Color ApplyNight(Color c, float nightDim)
        {
            if (nightDim >= 1.0f) return c;
            return ColorHelper.ApplyNightMode(c, nightDim);
        }

        private void DrawLabel(CanvasDrawingSession ds, string label,
                                float baseX, float y, bool isSelected, bool isMajor,
                                ReticleStyle style, float scale, float opacity,
                                int markIndex, int centerIdx, float nightDim)
        {
            // v2: Hidden labels
            if (style.TextPosition == LabelPosition.Hidden) return;

            float tickLen = (float)((isMajor ? style.MajorTickLength : style.MinorTickLength) * scale);
            float fontSize = (float)(style.FontSize * scale * (isSelected ? 1.2 : 1.0));

            string text = isSelected ? $">> {label} <<" : label;
            var baseTextColor = ColorHelper.ParseHex(isSelected ? "#CCFFFF00" : style.TextColor);
            var color = ApplyNight(ColorHelper.WithOpacity(baseTextColor, opacity), nightDim);

            var format = new Microsoft.Graphics.Canvas.Text.CanvasTextFormat
            {
                FontFamily = "Consolas",
                FontSize = fontSize,
                FontWeight = (isMajor || isSelected) ? FontWeights.Bold : FontWeights.Normal
            };

            // v2: Label position
            float textX;
            bool isAlternateRight = (markIndex % 2 == 0);

            switch (style.TextPosition)
            {
                case LabelPosition.Left:
                    textX = baseX - tickLen / 2 - MeasureTextWidth(ds, text, format) - 4 * scale;
                    break;
                case LabelPosition.Alternate:
                    if (isAlternateRight)
                        textX = baseX + tickLen / 2 + 4 * scale;
                    else
                        textX = baseX - tickLen / 2 - MeasureTextWidth(ds, text, format) - 4 * scale;
                    break;
                case LabelPosition.Right:
                default:
                    textX = baseX + tickLen / 2 + 4 * scale;
                    break;
            }

            ds.DrawText(text, textX, y - fontSize / 2, color, format);
        }

        private float MeasureTextWidth(CanvasDrawingSession ds, string text,
                                      Microsoft.Graphics.Canvas.Text.CanvasTextFormat format)
        {
            // Approximate: each character ≈ fontSize * 0.6 for Consolas
            return (float)(text.Length * format.FontSize * 0.62);
        }

        private void DrawStatusText(CanvasDrawingSession ds, ReticleConfig config,
                                     float opacity, float nightDim)
        {
            string status = $"[CrossbowOverlay] Scale:{config.Scale:F2} Spacing:{(int)config.MarkSpacing}px\n"
                          + $"Offset: ({(int)config.OffsetX}, {(int)config.OffsetY})";

            if (config.SelectedIndex >= 0 && config.SelectedIndex < config.Marks.Count)
            {
                var sm = config.Marks[config.SelectedIndex];
                status += $"\n>> Selected: {sm.Label} (OffsetY: {sm.OffsetY:F1})";
            }

            var baseColor = ColorHelper.ParseHex("#AAFFFFFF");
            var color = ApplyNight(ColorHelper.WithOpacity(baseColor, opacity), nightDim);
            var format = new Microsoft.Graphics.Canvas.Text.CanvasTextFormat
            {
                FontFamily = "Consolas",
                FontSize = 11,
                FontWeight = FontWeights.Normal
            };
            ds.DrawText(status, 10, 10, color, format);
        }
    }
}
