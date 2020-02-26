using Microsoft.Graphics.Canvas.Text;
using MyScript.InteractiveInk.Annotations;

namespace MyScript.InteractiveInk.UI.Extensions
{
    public static class CanvasTextLayoutExtensions
    {
        [NotNull]
        public static CanvasTextFormat GetTextFormat(this CanvasTextLayout source, int characterIndex)
        {
            return new CanvasTextFormat
            {
                FontFamily = source.GetFontFamily(characterIndex),
                FontSize = source.GetFontSize(characterIndex),
                FontStretch = source.GetFontStretch(characterIndex),
                FontStyle = source.GetFontStyle(characterIndex),
                FontWeight = source.GetFontWeight(characterIndex)
            };
        }

        public static void SetTextFormat(this CanvasTextLayout source, int characterIndex, int characterCount,
            [NotNull] CanvasTextFormat format)
        {
            source.SetFontFamily(characterIndex, characterCount, format.FontFamily);
            source.SetFontSize(characterIndex, characterCount, format.FontSize);
            source.SetFontStretch(characterIndex, characterCount, format.FontStretch);
            source.SetFontStyle(characterIndex, characterCount, format.FontStyle);
            source.SetFontWeight(characterIndex, characterCount, format.FontWeight);
        }
    }
}
