using System;
using System.Numerics;
using Windows.Graphics.Display;

namespace MyScript.InteractiveInk.UI.Services
{
    public static class DisplayInformationService
    {
        public static Vector2 GetDpi2()
        {
            var info = DisplayInformation.GetForCurrentView();
            var (dpiX, dpiY, scale) = (info.RawDpiX, info.RawDpiY, (float)info.RawPixelsPerViewPixel);

            if (!(Math.Abs(scale) > 0))
            {
                scale = (float)(Convert.ToDouble(info.ResolutionScale) / 100);
            }

            if (scale > 0)
            {
                dpiX /= scale;
                dpiY /= scale;
            }

            if (!(Math.Abs(dpiX) > 0) || !(Math.Abs(dpiY) > 0))
            {
                dpiX = dpiY = 96;
            }

            return new Vector2(dpiX, dpiY);
        }

        public static float GetDpi()
        {
            var dpi = GetDpi2();
            return (dpi.X + dpi.Y) / 2;
        }
    }
}
