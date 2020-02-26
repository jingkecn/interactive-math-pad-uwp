using Windows.Foundation;
using Microsoft.Toolkit.Helpers;
using MyScript.IInk;
using MyScript.IInk.Graphics;

namespace MyScript.InteractiveInk.UI.Extensions
{
    public static class RendererExtensions
    {
        public static void Draw(this Renderer source, Rect rect, LayerType layers, ICanvas canvas)
        {
            var (x, y, width, height) = ((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);

            var style = Singleton<Style>.Instance;
            style.SetChangeFlags((uint)StyleFlag.StyleFlag_ALL);
            style.ApplyTo(canvas);

            if (layers.HasFlag(LayerType.BACKGROUND))
            {
                source.DrawBackground(x, y, width, height, canvas);
            }

            if (layers.HasFlag(LayerType.MODEL))
            {
                source.DrawModel(x, y, width, height, canvas);
            }

            if (layers.HasFlag(LayerType.TEMPORARY))
            {
                source.DrawTemporaryItems(x, y, width, height, canvas);
            }

            if (layers.HasFlag(LayerType.CAPTURE))
            {
                source.DrawCaptureStrokes(x, y, width, height, canvas);
            }
        }
    }
}
