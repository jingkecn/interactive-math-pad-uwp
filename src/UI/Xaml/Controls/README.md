MyScript Interactive Ink | `InteractiveInkCanvas`
=================================================

The `InteractiveInkCanvas` control demonstrates how to implement the rendering commands with MyScript Interactive Ink SDK, using **Win2D** rendering.

1. [Input | Pointer Events](#input--pointer-events)
2. [Output: The Entry Point | IRenderTarget](#output-the-entry-point--irendertarget)
3. [Output: Drawing Commands | ICanvas and IPath](#output-drawing-commands--icanvas-and-ipath)
4. [Disclaimer](#disclaimer)

This control basically implements the following interfaces:

- `IRenderTarget`: triggers the rendering commands;
- `ICanvas`: applies rendering styles from the SDK and implements the platform drawings (shapes & texts);
- `IPath`: groups and implements the platform path builder.

Furthermore, this control also contains the layers to compromise the structure of the SDK rendering system:

- `BackgroundLayer`: corresponding to the background drawings, for example, the text baseline guides;
- `ModelLayer`: corresponding to the final drawings processed by the `Engine`, for example, the strokes revision;
- `TemporaryLayer`: corresponding to the temporary drawings, for example, the drag & drop selection items (not implemented yet in this control);
- `CaptureLayer`: corresponding to the ink capturing, this is the triggering layer for user interfaces.

Input | Pointer Events
---------------------------

The capture layer should listen pointer inputs:

```xml
<canvas:CanvasVirtualControl
    x:Name="CaptureLayer"
    PointerCanceled="OnPointerCanceled"
    PointerMoved="OnPointerMoved"
    PointerPressed="OnPointerPressed"
    PointerReleased="OnPointerReleased" />
```

Then send the relevant inputs to `Editor`:

- `PointerPressed` => `Editor.PointerDown`;
- `PointerMoved` => `Editor.PointerMove`;
- `PointerReleased` => `Editor.PointerUp`;
- `PointerCanceled` => `Editor.PointerCancel`;

For example, when we point down on the `CaptureLayer`:

```csharp
private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
{
    if (!(sender is UIElement element))
    {
        return;
    }

    var point = e.GetCurrentPoint(element);
    var (x, y) = (point.Position.X, point.Position.Y);
    // Converts timestamp from microseconds to milliseconds:
    var timestampInMilliseconds = point.Timestamp / 1000;
    var pressure = point.Properties.Pressure;
    var pointerType = point.PointerDevice.PointerDeviceType switch {
        PointerDeviceType.Touch => PointerType.TOUCH,
        PointerDeviceType.Pen => PointerType.PEN,
        PointerDeviceType.Mouse => PointerType.TOUCH,
        _ => throw new ArgumentOutOfRangeException()
    };
    var pointerId = point.PointerId;
    // Sends pointer inputs to editor:
    Editor.PointerDown(x, y,
        timestampInMilliseconds, pressure,
        pointerType, pointerId);
    // Prevents events from sinking to the under layers:
    e.Handled = true;
}
```

Then handle the other pointer events in a similar way.

Output: The Entry Point | `IRenderTarget`
-----------------------------------------

During the initialization of `Editor`, a `Renderer` instance is created with an attached render target implementing `IRenderTarget` interface:

```csharp
// Call stack:
// -> MainPage.OnNavigateTo(NavigationEventArgs)
// -> MainViewModel.Initialize(IRenderTarget)
// Assumes that the DPI is the default value 96.
var dpi = 96;
var renderer = engine.CreateRenderer(dpi, dpi, InteractiveInkCanvas);
var editor = engine.CreateEditor(renderer);
```

Here is the definition of `IRenderTarget`:

```csharp
interface IRenderTarget
{
    // Invalidates layers within a specified region.
    void Invalidate(Renderer renderer, int x, int y, int width, int height, LayerType layers);
    // Invalidates layers.
    void Invalidate(Renderer renderer, LayerType layers);
}
```

Basically, the second overload method can be considered as a special case of the first method with a region that equals to the entire layout:

```csharp
// Invalidates layers.
public void Invalidate(Renderer renderer, LayerType layers)
{
    Invalidate(render, 0, 0, ActualWidth, ActualHeight, layers);
}
```

Once the pointer inputs are sent to the `Editor`, the attached `Renderer` will tell the `IRenderTarget` which layers and which regions should be invalidated:

```csharp
// Invalidates layers.
public void Invalidate(Renderer renderer, int x, int y, int width, int height, LayerType layers)
{
    // Invalidate controls on UI thread.
    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
        if (layers.HasFlag(LayerType.BACKGROUND))
        {
            BackgroundLayer.Invalidate(new Rect(x, y, width, height));
        }

        if (layers.HasFlag(LayerType.CAPTURE))
        {
            CaptureLayer.Invalidate(new Rect(x, y, width, height));
        }

        if (layers.HasFlag(LayerType.MODEL))
        {
            ModelLayer.Invalidate(new Rect(x, y, width, height));
        }

        if (layers.HasFlag(LayerType.TEMPORARY))
        {
            TemporaryLayer.Invalidate(new Rect(x, y, width, height));
        }
    }).AsTask();
}
```

Then each layer should listen its own `RegionsInvalidated` events:

```xml
<canvas:CanvasVirtualControl
    x:Name="BackgroundLayer"
    RegionsInvalidated="OnRegionsInvalidated" />
<canvas:CanvasVirtualControl
    x:Name="ModelLayer"
    RegionsInvalidated="OnRegionsInvalidated" />
<canvas:CanvasVirtualControl
    x:Name="TemporaryLayer"
    RegionsInvalidated="OnRegionsInvalidated" />
<canvas:CanvasVirtualControl
    x:Name="CaptureLayer"
    RegionsInvalidated="OnRegionsInvalidated" />
```

And tell the `Renderer` to draw the corresponding layer:

```csharp
private void OnRegionsInvalidated(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs args)
{
    // Invoke drawing commands on UI thread.
    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
    {
        foreach (var region in args.InvalidatedRegions)
        {
            var (x, y, width, height) = (region.X, region.Y, region.Width, region.Height);
            // Initializations...
            switch(sender.Name)
            {
                case "BackgroundLayer":
                    Editor.Renderer.DrawBackground(x, y, width, height, this /* InteractiveInkCanvas implements ICanvas */);
                    break;
                case "CaptureLayer":
                    Editor.Renderer.DrawCaptureStrokes(x, y, width, height, this /* InteractiveInkCanvas implements ICanvas */);
                    break;
                case "ModelLayer":
                    Editor.Renderer.DrawModel(x, y, width, height, this /* InteractiveInkCanvas implements ICanvas */);
                    break;
                case "TemporaryLayer":
                    Editor.Renderer.DrawTemporaryItems(x, y, width, height, this /* InteractiveInkCanvas implements ICanvas */);
                    break;
                default:
                    break;
            }
            // Finalizations...
        }
    }).AsTask();
}
```

Quite simple!

Output: Drawing Commands | `ICanvas` and `IPath`
------------------------------------------------

A bunch of drawing commands are defined in the interface `ICanvas`:

- **Styles**: the `Renderer` tells the `ICanvas` instance to apply the MyScript Interactive Ink SDK styles to the platform views, so, basically, they are the styles mapping between SDK values and platform values;
- **Rendering**: the `Renderer` tells the `ICanvas` instance to draw shapes (lines, rectangles and paths), text and images;
- **Groupings**: the `Renderer` tells the `ICanvas` instance to draw a group box if necessary, this is basically supposed for content clipping.

Within the implementation of `ICanvas`, two path drawing commands are required to draw paths using an `IPath` object:

```csharp
public IPath CreatePath()
{
    // Initialization...
    return this; /* InteractiveInkCanvas implements IPath */
}

public void DrawPath(IPath path)
{
    // Draws a platform path from the SDK IPath object.
    // See source code for more details.
}
```

So an implementation of an `IPath` is required, and it is quite easy to draw platform paths according to the SDK drawing commands. See the [source code](InteractiveInkCanvas.xaml.cs#L253) for more details.

> See [more information](https://developer.myscript.com/docs/interactive-ink/1.3/windows/fundamentals/rendering/) about how MyScript Interactive Ink SDK manages the rendering.

Disclaimer
----------

> This is not the official implementation of MyScript Interactive Ink UI control, instead, it tries to demonstrate on how to better understand the rendering system of SDK and on how to implement the SDK rendering on Windows.
> If you're interested in the official implementation, please go to the [official source code](https://github.com/MyScript/interactive-ink-examples-uwp/blob/master/UIReferenceImplementation/UserControls/EditorUserControl.xaml) for more information.
