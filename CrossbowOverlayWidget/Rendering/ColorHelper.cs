using System;
using Windows.UI;

namespace CrossbowOverlayWidget.Rendering
{
    public static class ColorHelper
    {
        public static Color ParseHex(string hex)
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
            return Color.FromArgb(a, r, g, b);
        }

        public static Color WithOpacity(Color c, float opacity)
        {
            return Color.FromArgb((byte)(c.A * opacity), c.R, c.G, c.B);
        }
    }
}
