using CrossbowOverlayWidget.Models;
using Microsoft.Graphics.Canvas;
using Windows.UI;

namespace CrossbowOverlayWidget.Rendering
{
    public class DotRenderer : IShapeRenderer
    {
        public void DrawMark(CanvasDrawingSession ds, float x, float y,
                             bool isMajor, bool isSelected, ReticleStyle style,
                             float scale, float opacity,
                             Color? colorOverride = null, float taperFactor = 0f)
        {
            float radius = (float)(style.DotRadius * scale * (isMajor ? 1.5 : 1.0)
                         * (isSelected ? 1.3 : 1.0) * (1f + taperFactor * 0.5f));
            Color color = colorOverride ?? ColorHelper.ParseHex(
                isSelected ? "#CCFFFF00"
                : isMajor ? style.MajorColor : style.MinorColor);
            color = ColorHelper.WithOpacity(color, opacity);

            ds.FillCircle(x, y, radius, color);
        }

        public void DrawCenterReticle(CanvasDrawingSession ds, float x, float y,
                                      ReticleStyle style, float scale, float opacity)
        {
            var color = ColorHelper.WithOpacity(ColorHelper.ParseHex(style.CenterLineColor), opacity);
            float radius = (float)(style.DotRadius * scale * 1.5);
            ds.FillCircle(x, y, radius, color);

            var white = ColorHelper.WithOpacity(Colors.White, opacity * 0.6f);
            ds.DrawCircle(x, y, radius + 1, white, 0.5f);
        }
    }
}
