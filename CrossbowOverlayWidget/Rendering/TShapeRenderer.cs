using CrossbowOverlayWidget.Models;
using Microsoft.Graphics.Canvas;
using Windows.UI;

namespace CrossbowOverlayWidget.Rendering
{
    /// <summary>
    /// T-Shape reticle — horizontal bar with a short vertical stem pointing upward.
    /// Creates a classic T-post sight picture at each distance mark.
    /// </summary>
    public class TShapeRenderer : IShapeRenderer
    {
        public void DrawMark(CanvasDrawingSession ds, float x, float y,
                             bool isMajor, bool isSelected, ReticleStyle style,
                             float scale, float opacity,
                             Color? colorOverride = null, float taperFactor = 0f)
        {
            float tickLen = (float)((isMajor ? style.MajorTickLength : style.MinorTickLength)
                          * scale * (isSelected ? 1.3 : 1.0));
            float baseLw = (float)((isMajor ? style.MajorLineWidth : style.MinorLineWidth)
                         * scale * (isSelected ? 1.5 : 1.0));
            float lw = baseLw * (1f + taperFactor);

            Color color = colorOverride ?? ColorHelper.ParseHex(
                isSelected ? "#CCFFFF00"
                : isMajor ? style.MajorColor : style.MinorColor);
            color = ColorHelper.WithOpacity(color, opacity);

            // Horizontal bar (the top of the T)
            ds.DrawLine(x - tickLen / 2, y, x + tickLen / 2, y, color, lw);

            // Vertical stem pointing up from center
            float stemLen = tickLen * 0.4f;
            ds.DrawLine(x, y, x, y - stemLen, color, lw);
        }

        public void DrawCenterReticle(CanvasDrawingSession ds, float x, float y,
                                      ReticleStyle style, float scale, float opacity)
        {
            var color = ColorHelper.WithOpacity(ColorHelper.ParseHex(style.CenterLineColor), opacity);
            float s = 6 * scale;
            float lw = 1.5f * scale;

            // Horizontal bar
            ds.DrawLine(x - s, y, x + s, y, color, lw);

            // Vertical stem up
            ds.DrawLine(x, y, x, y - s, color, lw);

            // White outline
            var white = ColorHelper.WithOpacity(Colors.White, opacity);
            ds.DrawLine(x - s, y - 0.5f, x + s, y - 0.5f, white, 0.4f);
            ds.DrawLine(x + 0.5f, y, x + 0.5f, y - s, white, 0.4f);
        }
    }
}
