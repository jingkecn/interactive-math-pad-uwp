MyScript Interactive Ink SDK | UI
=================================

This project implements most of the UI elements:

- [`InteractiveInkCanvas`](Xaml/Controls/InteractiveInkCanvas.xaml): the main UI control implementing rendering commands, implementing:
  - [`IRenderTarget`](https://developer.myscript.com/refguides/interactive-ink/android/1.3/com/myscript/iink/IRenderTarget.html): represents a render target for the SDK renderer, see [implementation](Xaml/Controls/InteractiveInkCanvas.xaml.cs#L320);
  - [`ICanvas`](https://developer.myscript.com/refguides/interactive-ink/android/1.3/index.html?com/myscript/iink/graphics/ICanvas.html): receives drawing commands, see [implementation](Xaml/Controls/InteractiveInkCanvas.xaml.cs#L43);
  - [`IPath`](https://developer.myscript.com/refguides/interactive-ink/android/1.3/index.html?com/myscript/iink/graphics/IPath.html): receives SVG path drawing commands, see [implementation](Xaml/Controls/InteractiveInkCanvas.xaml.cs#L253);
- [`FontMetricsService`](Services/FontMetricsService.cs) implements:
  - [`IFontMetricsProvider`](https://developer.myscript.com/refguides/interactive-ink/android/1.3/index.html?com/myscript/iink/text/IFontMetricsProvider.html): provides digital text typesetting operations, see [implementation](Services/FontMetricsService.cs#L20).
