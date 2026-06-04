using CrossbowOverlayWidget.Models;
using Microsoft.Graphics.Canvas;
using Windows.UI;

namespace CrossbowOverlayWidget.Rendering
{
    /// <summary>
    /// Circle reticle shape — draws open circles at each distance mark.
    /// Major marks get a larger ring; minor marks a smaller one.
    /// </summary>
    public class CircleRenderer : IShapeRenderer
    {
        public void DrawMark(CanvasDrawingSession ds, float x, float y,
                             bool isMajor, bool isSelected, ReticleStyle style,
                             float scale, float opacity,
                             Color? colorOverride = null, float taperFactor = 0f)
        {
            float radius = (float)(style.CircleRadius * scale * 0.4
                         * (isMajor ? 1.4 : 1.0)
                         * (isSelected ? 1.3 : 1.0)
                         * (1f + taperFactor * 0.3f));
            float lw = (float)((isMajor ? style.MajorLineWidth : style.MinorLineWidth)
                     * scale * (isSelected ? 1.5 : 1.0));

            Color color = colorOverride ?? ColorHelper.ParseHex(
                isSelected ? "#CCFFFF00"
                : isMajor ? style.MajorColor : style.MinorColor);
            color = ColorHelper.WithOpacity(color, opacity);

            ds.DrawCircle(x, y, radius, color, lw);
        }

        public void DrawCenterReticle(CanvasDrawingSession ds, float x, float y,
                                      ReticleStyle style, float scale, float opacity)
        {
            var color = ColorHelper.WithOpacity(ColorHelper.ParseHex(style.CenterLineColor), opacity);
            float r = (float)(style.CircleRadius * scale * 0.5);
            float lw = 1.5f * scale;

            ds.DrawCircle(x, y, r, color, lw);

            // Small crosshair inside for precision
            float inner = r * 0.35f;
            var white = ColorHelper.WithOpacity(Colors.White, opacity * 0.8f);
            ds.DrawLine(x - inner, y, x + inner, y, white, 0.5f);
            ds.DrawLine(x, y - inner, x, y + inner, white, 0.5f);
        }
    }
}
