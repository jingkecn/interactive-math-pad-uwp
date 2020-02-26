using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using MyScript.IInk.Graphics;
using MyScript.InteractiveInk.Annotations;

namespace MyScript.InteractiveInk.UI.Extensions
{
    public static class RectangleExtensions
    {
        public static Rect Clamp(this Rect source, [NotNull] FrameworkElement element)
        {
            return RectHelper.Intersect(LayoutInformation.GetLayoutSlot(element), source);
        }

        public static Rectangle ToNativeRect(this Rect source)
        {
            return new Rectangle((float)source.X, (float)source.Y,
                (float)source.Width, (float)source.Height);
        }

        public static Rect ToPlatformRect(this Rectangle source)
        {
            return new Rect(source.X, source.Y, source.Width, source.Height);
        }
    }
}
