using System.Collections.Generic;
using CrossbowOverlayWidget.Enums;

namespace CrossbowOverlayWidget.Rendering
{
    public class RendererFactory
    {
        private readonly Dictionary<ReticleShape, IShapeRenderer> _renderers = new Dictionary<ReticleShape, IShapeRenderer>
        {
            [ReticleShape.Cross] = new CrosshairRenderer(),
            [ReticleShape.Dot] = new DotRenderer(),
            [ReticleShape.Diamond] = new DiamondRenderer(),
            [ReticleShape.Circle] = new CircleRenderer(),
            [ReticleShape.TShape] = new TShapeRenderer(),
        };

        public IShapeRenderer GetRenderer(ReticleShape shape)
        {
            return _renderers.TryGetValue(shape, out var r) ? r : _renderers[ReticleShape.Cross];
        }
    }
}
