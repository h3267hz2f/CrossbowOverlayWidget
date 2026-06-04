using System;
using Windows.UI;

namespace CrossbowOverlayWidget.Rendering
{
    public static class ColorHelper
    {
        public static Color ParseHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return Colors.White;
            try
            {
                byte a = 255, r = 0, g = 0, b = 0;
                hex = hex.TrimStart('#');
                if (hex.Length == 8)
                {
                    a = Convert.ToByte(hex.Substring(0, 2), 16);
                    r = Convert.ToByte(hex.Substring(2, 2), 16);
                    g = Convert.ToByte(hex.Substring(4, 2), 16);
                    b = Convert.ToByte(hex.Substring(6, 2), 16);
                }
                else if (hex.Length == 6)
                {
                    r = Convert.ToByte(hex.Substring(0, 2), 16);
                    g = Convert.ToByte(hex.Substring(2, 2), 16);
                    b = Convert.ToByte(hex.Substring(4, 2), 16);
                }
                else if (hex.Length == 3)
                {
                    r = Convert.ToByte(new string(hex[0], 2), 16);
                    g = Convert.ToByte(new string(hex[1], 2), 16);
                    b = Convert.ToByte(new string(hex[2], 2), 16);
                }
                else
                {
                    return Colors.White;
                }
                return Color.FromArgb(a, r, g, b);
            }
            catch
            {
                return Colors.White;
            }
        }

        public static Color WithOpacity(Color c, float opacity)
        {
            return Color.FromArgb((byte)(c.A * opacity), c.R, c.G, c.B);
        }

        public static Color InterpolateGradient(Color from, Color to, float t)
        {
            t = Math.Max(0, Math.Min(1, t));
            return Color.FromArgb(
                (byte)(from.A + (to.A - from.A) * t),
                (byte)(from.R + (to.R - from.R) * t),
                (byte)(from.G + (to.G - from.G) * t),
                (byte)(from.B + (to.B - from.B) * t));
        }

        public static Color ApplyNightMode(Color c, float dimFactor)
        {
            // Dim RGB and add red tint
            float r = c.R * dimFactor + c.R * 0.1f;
            float g = c.G * dimFactor * 0.3f;
            float b = c.B * dimFactor * 0.3f;
            return Color.FromArgb(c.A,
                (byte)Math.Min(255, r),
                (byte)Math.Min(255, g),
                (byte)Math.Min(255, b));
        }
    }
}
