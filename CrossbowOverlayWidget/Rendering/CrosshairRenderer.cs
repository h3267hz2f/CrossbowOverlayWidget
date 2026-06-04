using CrossbowOverlayWidget.Models;
using Microsoft.Graphics.Canvas;
using Windows.UI;

namespace CrossbowOverlayWidget.Rendering
{
    public class CrosshairRenderer : IShapeRenderer
    {
        public void DrawMark(CanvasDrawingSession ds, float x, float y,
                             bool isMajor, bool isSelected, ReticleStyle style,
                             float scale, float opacity)
        {
            float tickLen = (float)((isMajor ? style.MajorTickLength : style.MinorTickLength)
                          * scale * (isSelected ? 1.3 : 1.0));
            float lw = (float)((isMajor ? style.MajorLineWidth : style.MinorLineWidth)
                     * scale * (isSelected ? 1.5 : 1.0));

            Color color = ColorHelper.ParseHex(
                isSelected ? "#CCFFFF00"
                : isMajor ? style.MajorColor : style.MinorColor);
            color = ColorHelper.WithOpacity(color, opacity);

            ds.DrawLine(x - tickLen / 2, y, x + tickLen / 2, y, color, lw);
        }

        public void DrawCenterReticle(CanvasDrawingSession ds, float x, float y,
                                      ReticleStyle style, float scale, float opacity)
        {
            var color = ColorHelper.WithOpacity(ColorHelper.ParseHex(style.CenterLineColor), opacity);
            float s = 5 * scale;
            float lw = 1.5f * scale;

            ds.DrawLine(x, y - s, x + s, y, color, lw);
            ds.DrawLine(x + s, y, x, y + s, color, lw);
            ds.DrawLine(x, y + s, x - s, y, color, lw);
            ds.DrawLine(x - s, y, x, y - s, color, lw);

            // White outline
            var white = ColorHelper.WithOpacity(Colors.White, opacity);
            ds.DrawLine(x, y - s, x + s, y, white, 0.5f);
            ds.DrawLine(x + s, y, x, y + s, white, 0.5f);
            ds.DrawLine(x, y + s, x - s, y, white, 0.5f);
            ds.DrawLine(x - s, y, x, y - s, white, 0.5f);
        }
    }
}
