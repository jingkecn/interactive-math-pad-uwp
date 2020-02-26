using System;
using Windows.UI;

namespace MyScript.InteractiveInk.UI.Extensions
{
    public static class ColorExtensions
    {
        public static Color ToPlatformColor(this IInk.Graphics.Color source)
        {
            return Color.FromArgb(Convert.ToByte(source.A), Convert.ToByte(source.R), Convert.ToByte(source.G),
                Convert.ToByte(source.B));
        }
    }
}
