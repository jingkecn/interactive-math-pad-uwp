using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Toolkit.Helpers;
using MyScript.IInk.Graphics;
using MyScript.IInk.Text;
using MyScript.InteractiveInk.UI.Extensions;

namespace MyScript.InteractiveInk.UI.Services
{
    public partial class FontMetricsService
    {
        private Vector2? _dpi;
        private Vector2 Dpi => _dpi ??= DisplayInformationService.GetDpi2();
    }

    // ReSharper disable once RedundantExtendsListEntry
    public partial class FontMetricsService : IFontMetricsProvider
    {
        public Rectangle[] GetCharacterBoundingBoxes(Text text, TextSpan[] spans)
        {
            return GetGlyphMetrics(text, spans).Select(metrics => metrics.BoundingBox).ToArray();
        }

        /// <summary>
        ///     Get font size. This method influences the line spacing of text document.
        ///     [!Attention]
        ///     Don't return font size in pixels, otherwise it will be double converted in pixels.
        /// </summary>
        /// <param name="style"></param>
        /// <returns>Should return font size in millimeter.</returns>
        public float GetFontSizePx(Style style)
        {
            return style.FontSize;
        }
    }

    public partial class FontMetricsService : IFontMetricsProvider2
    {
        private const float VirtualSize = 10000;

        public bool SupportsGlyphMetrics()
        {
            return true;
        }

        /// <summary>
        ///     Gets the metrics of all glyphs from the given <code cref="Text">text</code> and
        ///     <code cref="TextSpan">text span</code>s.
        ///     TODO: Get glyph metrics from <code cref="CanvasFontFace">font face</code>.
        ///     See: http://microsoft.github.io/Win2D/html/T_Microsoft_Graphics_Canvas_Text_ICanvasTextRenderer.htm.
        /// </summary>
        /// <param name="text">The given <code cref="Text">text</code></param>
        /// <param name="spans">The given <code cref="TextSpan">text span</code>s.</param>
        /// <returns>The metrics of each glyphs.</returns>
        public GlyphMetrics[] GetGlyphMetrics(Text text, TextSpan[] spans)
        {
            var metrics = new List<GlyphMetrics>();

            // Create a virtual text layout for all glyphs.
            var device = CanvasDevice.GetSharedDevice();
            using var labelLayout = new CanvasTextLayout(device, text.Label, Singleton<CanvasTextFormat>.Instance,
                VirtualSize, VirtualSize);

            // For each span, apply span style to the text layout.
            foreach (var span in spans)
            {
                var index = span.BeginPosition;
                var count = span.EndPosition - span.BeginPosition;
                var format = span.Style.ToCanvasTextFormat(Dpi.Y);
                labelLayout.SetTextFormat(index, count, format);
            }

            // For each glyph, calculate its metrics.
            for (var index = 0; index < text.GlyphCount; index++)
            {
                // Create a virtual text layout for the single glyph.
                var glyph = text.GetGlyphLabelAt(index);
                var format = labelLayout.GetTextFormat(index);
                using var glyphLayout = new CanvasTextLayout(device, glyph, format, VirtualSize, VirtualSize);
                // TODO: explain why we choose draw bounds over layout bounds.
                // See: https://microsoft.github.io/Win2D/html/P_Microsoft_Graphics_Canvas_Text_CanvasTextLayout_DrawBounds.htm
                var drawBounds = glyphLayout.DrawBounds;
                // Calculate horizontal spacing (for horizontal side bearing).
                var leftSpacing = drawBounds.Left;
                var caret = glyphLayout.GetCaretPosition(0, false);
                var rightSpacing = drawBounds.Right - caret.X;
                // Translation
                var position = labelLayout.GetCaretPosition(index, false);
                drawBounds.X += position.X;
                drawBounds.Y -= glyphLayout.LineMetrics.First().Baseline;
                // Add metrics into the metrics collection.
                var boundingBox = drawBounds.FromPixelToMillimeter(Dpi).ToNativeRect();
                var leftSideBearing = -leftSpacing.FromPixelToMillimeter(Dpi.X);
                var rightSideBearing = -rightSpacing.FromPixelToMillimeter(Dpi.X);
                metrics.Add(new GlyphMetrics(boundingBox, leftSideBearing, rightSideBearing));
            }

            return metrics.ToArray();
        }
    }
}
