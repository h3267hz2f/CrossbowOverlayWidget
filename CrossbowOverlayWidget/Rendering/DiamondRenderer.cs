using CrossbowOverlayWidget.Models;
using Microsoft.Graphics.Canvas;
using Windows.UI;

namespace CrossbowOverlayWidget.Rendering
{
    public class DiamondRenderer : IShapeRenderer
    {
        public void DrawMark(CanvasDrawingSession ds, float x, float y,
                             bool isMajor, bool isSelected, ReticleStyle style,
                             float scale, float opacity)
        {
            float s = (float)(style.DiamondSize * scale * 0.3 * (isMajor ? 1.5 : 1.0)
                    * (isSelected ? 1.3 : 1.0));
            var color = ColorHelper.ParseHex(
                isSelected ? "#CCFFFF00"
                : isMajor ? style.MajorColor : style.MinorColor);
            color = ColorHelper.WithOpacity(color, opacity);

            float lw = (float)((isMajor ? style.MajorLineWidth : style.MinorLineWidth) * scale);

            ds.DrawLine(x - s, y, x, y - s, color, lw);
            ds.DrawLine(x, y - s, x + s, y, color, lw);
            ds.DrawLine(x + s, y, x, y + s, color, lw);
            ds.DrawLine(x, y + s, x - s, y, color, lw);
        }

        public void DrawCenterReticle(CanvasDrawingSession ds, float x, float y,
                                      ReticleStyle style, float scale, float opacity)
        {
            var color = ColorHelper.WithOpacity(ColorHelper.ParseHex(style.CenterLineColor), opacity);
            float s = (float)(style.DiamondSize * scale);
            float lw = 1.5f * scale;

            ds.DrawLine(x, y - s, x + s, y, color, lw);
            ds.DrawLine(x + s, y, x, y + s, color, lw);
            ds.DrawLine(x, y + s, x - s, y, color, lw);
            ds.DrawLine(x - s, y, x, y - s, color, lw);

            var white = ColorHelper.WithOpacity(Colors.White, opacity);
            ds.DrawLine(x, y - s, x + s, y, white, 0.5f);
            ds.DrawLine(x + s, y, x, y + s, white, 0.5f);
            ds.DrawLine(x, y + s, x - s, y, white, 0.5f);
            ds.DrawLine(x - s, y, x, y - s, white, 0.5f);
        }
    }
}
