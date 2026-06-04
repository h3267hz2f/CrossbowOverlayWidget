using CrossbowOverlayWidget.Models;
using Microsoft.Graphics.Canvas;

namespace CrossbowOverlayWidget.Rendering
{
    public interface IShapeRenderer
    {
        void DrawMark(CanvasDrawingSession ds, float x, float y,
                      bool isMajor, bool isSelected, ReticleStyle style,
                      float scale, float opacity);

        void DrawCenterReticle(CanvasDrawingSession ds, float x, float y,
                               ReticleStyle style, float scale, float opacity);
    }
}
