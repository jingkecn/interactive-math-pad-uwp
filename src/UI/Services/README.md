MyScript Interactive Ink | `IFontMetricsProvider`
=================================================

1. [IFontMetricsProvider | Definitions](#ifontmetricsprovider--definitions)
2. [IFontMetricsProvider | Solution](#ifontmetricsprovider--solution)
3. [Disclaimer](#disclaimer)

According to the SDK [documentations](https://developer.myscript.com/docs/interactive-ink/1.3/windows/fundamentals/conversion/#computing-font-metrics), the computed font metrics for each glyph is required to convert the text content between handwritten strokes and typeset/digital text.

`IFontMetricsProvider` | Definitions
------------------------------------

The interface `IFontMetricsProvider` defines the APIs that we have to implements:

```csharp
// Decompiled with JetBrains decompiler
// Type: MyScript.IInk.Text.IFontMetricsProvider

/// <summary>Interface providing fontified text typesetting operations.</summary>
interface IFontMetricsProvider
{
    /// <summary>
    /// Returns the bounding box of each glyph of the specified text as if it were
    /// displayed at 0,0 using the specified styles.
    /// </summary>
    /// <param name="text">the text.</param>
    /// <param name="spans">an array of glyph intervals with associated style.</param>
    /// <returns>the bounding boxes.</returns>
    Rectangle[] GetCharacterBoundingBoxes(Text text, TextSpan[] spans);
    /// <summary>
    /// Returns the size of the specified font in pixels. On input the font size is
    /// expressed in logical units as specified by the platform.
    /// </summary>
    /// <param name="style">the style that specifies the font.</param>
    /// <returns>the size of the font in pixels.</returns>
    float GetFontSizePx(Style style);
}
```

By definition, in this implementation we should:

- Compute the **bounding box** of each character/glyph, given the text label and text span containing rendering range and styles;
- Convert SDK font size to platform units if necessary (controversial, see [source code](FontMetricsService.cs#L27) for more details).

Before long, another interface `IFontMetricsProvider2` defines more precise APIs:

```csharp
// Decompiled with JetBrains decompiler
// Type: MyScript.IInk.Text.IFontMetricsProvider2

/// <summary>
/// Interface providing fontified text typesetting operations. This version of the
/// interface adds new functions introduced in version 1.2 that will be merged
/// back into base interface in version 2.0.
/// </summary>
/// <since>1.2</since>
interface IFontMetricsProvider2 : IFontMetricsProvider
{
    /// <summary>
    /// Determine whether implementation supports <code>GetGlyphMetrics()</code>.
    /// </summary>
    /// <returns>
    /// <code>true</code> if <code>GetGlyphMetrics()</code> is implemented, otherwise <code>false</code>.</returns>
    bool SupportsGlyphMetrics();
    /// <summary>Get the detailed glyph metrics from the label according to the platform.</summary>
    /// <param name="text">the text.</param>
    /// <param name="spans">an array of glyph intervals with associated style.</param>
    /// <returns>the glyph metrics.</returns>
    GlyphMetrics[] GetGlyphMetrics(Text text, TextSpan[] spans);
}
```

> According to the SDK documentations, the `IFontMetricsProvider2` would be merged into the base interface `IFontMetricsProvider` in further version.

By definition, in this implementation we should:

- Compute the **font metrics** of each character/glyph, including left and right side bearing according to the font, given the text label and text span containing rendering range and styles;
- Enable/disable font metric support (`true` in this project).

The `GlyphMetrics`:

```csharp
// Decompiled with JetBrains decompiler
// Type: MyScript.IInk.Text.GlyphMetrics

/// <summary>Describes glyphs spans in a label.</summary>
class GlyphMetrics
{
    /// <summary>The glyph left side bearing.</summary>
    float LeftSideBearing { get; }
    /// <summary>The glyph right side bearing.</summary>
    int RightSideBearing { get; }
    /// <summary>The glyph bounding box.</summary>
    Rectangle BoundingBox { get; }
}
```

Here is an illustration on what we have to compute:

![alt](https://learnopengl.com/img/in-practice/glyph_offset.png)

`IFontMetricsProvider` | Solution
---------------------------------

Before going any further, let's figure out what is given to us:

The `Text`:

```csharp
// Decompiled with JetBrains decompiler
// Type: MyScript.IInk.Text.Text

/// <summary>Allows to access the glyphs (i.e. grapheme clusters) within a label.</summary>
/// @note what is called glyph here, is actually a grapheme cluster.
/// For more details refer to http://unicode.org/reports/tr29/
class Text
{
    /// <summary>The label of this text.</summary>
    string Label { get; }
    /// <summary>The number of glyphs associated with this text.</summary>
    int GlyphCount { get; }
    /// <summary>Returns the label of the glyph at a given index.</summary>
    /// <param name="index">the index of the glyph to retrieve, in glyph count.</param>
    /// <returns>the label of the glyph at <c>index</c>.</returns>
    string GetGlyphAt(int index);
    /// <summary>Returns the starting position in the label, in bytes, of a given glyph.</summary>
    /// <param name="index">the index of the glyph to retrieve, in glyph count.</param>
    /// <returns>the starting position on the glyph in the label, in bytes.</returns>
    int GetGlyphBeginAt(int index);
    /// <summary>Returns the end position in the label, in bytes, of a given glyph.</summary>
    /// <param name="index">the index of the glyph to retrieve, in glyph count.</param>
    /// <returns>the starting position on the glyph in the label, in bytes.</returns>
    int GetGlyphEndAt(int index);
}
```

The `TextSpan`:

```csharp
// Decompiled with JetBrains decompiler
// Type: MyScript.IInk.Text.TextSpan

/// <summary>Describes glyphs spans in a label.</summary>
class TextSpan
{
    /// <summary>
    /// The begin position in a label of this span, in glyph (i.e. grapheme cluster) count.
    /// </summary>
    int BeginPosition { get; }
    /// <summary>
    /// The end position in a label of this span, in glyph (i.e. grapheme cluster) count.
    /// </summary>
    int EndPosition { get; }
    /// <summary>The style associated with this span.</summary>
    Style Style { get; }
}
```

To simplify the problem:

> **Given the text label and the rendering style of each text span, compute the `GlyphMetrics` of each character/glyph.**

Step 1 - **for each text span, apply span style to the corresponding glyphs**:

```csharp
var device = CanvasDevice.GetSharedDevice();
// Sizes don't count as long as it is large enough to contain the text.
var virtualSize = 10000;
var defaultFormat = new CanvasTextFormat();
// Create a virtual text layout for all glyphs.
using var labelLayout = new CanvasTextLayout(device, text.Label, defaultFormat,
    virtualSize, virtualSize);
// For each span, apply span style to the text layout.
foreach (var span in spans)
{
    var index = span.BeginPosition;
    var count = span.EndPosition - span.BeginPosition;
    // See: StyleExtensions.ToCanvasTextFormat(dpi)
    var format = span.Style.ToCanvasTextFormat(96);
    // See: CanvasTextLayoutExtensions.SetTextFormat(characterIndex, characterCount, format).
    labelLayout.SetTextFormat(index, count, format);
}
```

Step 2 - **for each glyph in rendered text, compute its font metrics** (the green bounding boxes in the following illustration):

![alt](https://microsoft.github.io/Win2D/media/TextMetrics.png)

```csharp
var metrics = new List<GlyphMetrics>();
// For each glyph, calculate its metrics.
for (var index = 0; index < text.GlyphCount; index++)
{
    var glyph = text.GetGlyphLabelAt(index);
    // See: CanvasTextLayoutExtensions.GetTextFormat(characterIndex).
    var format = labelLayout.GetTextFormat(index);
    // Create a virtual text layout for the single glyph.
    using var glyphLayout = new CanvasTextLayout(device, glyph, format, virtualSize, virtualSize);
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
    var boundingBox = drawBounds.FromPixelToMillimeter(96).ToNativeRect();
    var leftSideBearing = -leftSpacing.FromPixelToMillimeter(96);
    var rightSideBearing = -rightSpacing.FromPixelToMillimeter(96);
    metrics.Add(new GlyphMetrics(boundingBox, leftSideBearing, rightSideBearing));
}
```

> MyScript Interactive Ink SDK computes rendering units in millimeter.

Step 3 - **return computed glyph metrics in an array**:

```csharp
return metrics.ToArray();
```

Disclaimer
----------

> This is not the official implementation of `IFontMetricsProvider`, instead, it tries to demonstrate on how to better understand the computation of font metrics and on how to implement `IFontMetricsProvider` on Windows.
> If you're interested in the official implementation, please go to the [official source code](https://github.com/MyScript/interactive-ink-examples-uwp/blob/master/UIReferenceImplementation/FontMetricsProvider.cs) for more information.
