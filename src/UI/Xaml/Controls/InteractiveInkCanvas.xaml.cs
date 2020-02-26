using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Toolkit.Helpers;
using MyScript.IInk;
using MyScript.IInk.Graphics;
using MyScript.InteractiveInk.Annotations;
using MyScript.InteractiveInk.UI.Extensions;
using Color = Windows.UI.Color;

namespace MyScript.InteractiveInk.UI.Xaml.Controls
{
    public sealed partial class InteractiveInkCanvas
    {
        public static readonly DependencyProperty EditorProperty =
            DependencyProperty.Register("Editor", typeof(Editor), typeof(InteractiveInkCanvas),
                new PropertyMetadata(default(Editor)));

        public InteractiveInkCanvas()
        {
            InitializeComponent();
        }

        public Editor Editor
        {
            get => GetValue(EditorProperty) as Editor;
            set => SetValue(EditorProperty, value);
        }
    }

    /// <summary>Implements drawing commands.</summary>
    public sealed partial class InteractiveInkCanvas : ICanvas
    {
        private CanvasActiveLayer ActiveLayer { get; set; }
        private Dictionary<string, Rect> Layers { get; } = new Dictionary<string, Rect>();

        public void StartGroup(string id, float x, float y, float width, float height, bool clipContent)
        {
            if (!clipContent)
            {
                return;
            }

            ActiveLayer?.Dispose();
            ActiveLayer = Session?.CreateLayer(1, Layers[id] = new Rect(x, y, width, height));
        }

        public void EndGroup(string id)
        {
            if (!Layers.ContainsKey(id))
            {
                return;
            }

            ActiveLayer?.Dispose();
            Layers.Remove(id, out var rect);
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return;
            }

            ActiveLayer = Session?.CreateLayer(1, rect);
        }

        public void StartItem(string id)
        {
            //throw new NotImplementedException();
        }

        public void EndItem(string id)
        {
            //throw new NotImplementedException();
        }

        #region Rendering

        private CanvasDrawingSession Session { get; set; }

        public IPath CreatePath()
        {
            PathBuilder = new CanvasPathBuilder(Session?.Device);
            return this;
        }

        public void DrawPath(IPath path)
        {
            var geometry = CreateGeometry(path);
            if (geometry == null)
            {
                return;
            }

            Session?.DrawGeometry(geometry, StrokeColor, StrokeThickness, StrokeStyle);
            Session?.FillGeometry(geometry, FillColor);
        }

        public void DrawRectangle(float x, float y, float width, float height)
        {
            Session?.DrawRectangle(x, y, width, height, StrokeColor, StrokeThickness, StrokeStyle);
            Session?.FillRectangle(x, y, width, height, FillColor);
        }

        public void DrawLine(float x1, float y1, float x2, float y2)
        {
            Session?.DrawLine(x1, y1, x2, y2, StrokeColor, StrokeThickness, StrokeStyle);
        }

        public void DrawObject(string url, string mimeType, float x, float y, float width, float height)
        {
            throw new NotImplementedException();
        }

        public void DrawText(string label, float x, float y, float minX, float minY, float maxX, float maxY)
        {
            Session?.DrawText(label, Math.Max(minX, Math.Min(x, maxX)),
                Math.Max(minY, Math.Min(y, maxY)) - TextBaseLine, FillColor, TextFormat);
        }

        public Transform Transform
        {
            get => _transform;
            set
            {
                if (Session != null)
                {
                    Session.Transform = value.ToPlatformTransform();
                }

                _transform = value;
            }
        }

        #endregion

        #region Styles

        private Transform _transform = Matrix3x2.Identity.ToNativeTransform();

        private Color FillColor { get; set; } = Colors.Black;
        private Color StrokeColor { get; set; } = Colors.Black;
        private CanvasStrokeStyle StrokeStyle { get; } = Singleton<CanvasStrokeStyle>.Instance;
        private float StrokeThickness { get; set; } = 1;
        private float TextBaseLine { get; set; } = 1;
        private CanvasTextFormat TextFormat { get; } = Singleton<CanvasTextFormat>.Instance;

        public void SetStrokeColor(IInk.Graphics.Color color)
        {
            StrokeColor = color.ToPlatformColor();
        }

        public void SetStrokeWidth(float width)
        {
            StrokeThickness = width;
        }

        public void SetStrokeLineCap(LineCap lineCap)
        {
            StrokeStyle.DashCap = StrokeStyle.EndCap = StrokeStyle.StartCap = lineCap switch
            {
                LineCap.BUTT => CanvasCapStyle.Flat,
                LineCap.ROUND => CanvasCapStyle.Round,
                LineCap.SQUARE => CanvasCapStyle.Square,
                _ => throw new ArgumentOutOfRangeException(nameof(lineCap), lineCap, null)
            };
        }

        public void SetStrokeLineJoin(LineJoin lineJoin)
        {
            StrokeStyle.LineJoin = lineJoin switch
            {
                LineJoin.MITER => CanvasLineJoin.Miter,
                LineJoin.ROUND => CanvasLineJoin.Round,
                LineJoin.BEVEL => CanvasLineJoin.Bevel,
                _ => throw new ArgumentOutOfRangeException(nameof(lineJoin), lineJoin, null)
            };
        }

        public void SetStrokeMiterLimit(float limit)
        {
            StrokeStyle.MiterLimit = limit;
        }

        public void SetStrokeDashArray(float[] array)
        {
            StrokeStyle.CustomDashStyle = array;
        }

        public void SetStrokeDashOffset(float offset)
        {
            StrokeStyle.DashOffset = offset;
        }

        public void SetFillColor(IInk.Graphics.Color color)
        {
            FillColor = color.ToPlatformColor();
        }

        public void SetFillRule(FillRule rule)
        {
            //throw new NotImplementedException();
        }

        public void SetFontProperties(string family, float lineHeight, float size, string style, string variant,
            int weight)
        {
            TextFormat.FontFamily = family;
            TextFormat.FontSize = size;
            TextFormat.FontStyle = Enum.Parse<FontStyle>(style, true);
            TextFormat.FontWeight = weight switch
            {
                var value when value >= 700 => FontWeights.Bold,
                var value when value < 400 => FontWeights.Light,
                _ => FontWeights.Normal
            };
            using var layout =
                new CanvasTextLayout(Session?.Device, "k", TextFormat, float.MaxValue, float.MaxValue);
            TextBaseLine = layout.LineMetrics.FirstOrDefault().Baseline;
        }

        #endregion
    }

    /// <summary>Releases unmanaged resources.</summary>
    public sealed partial class InteractiveInkCanvas : IDisposable
    {
        public void Dispose()
        {
            BackgroundLayer.RemoveFromVisualTree();
            CaptureLayer.RemoveFromVisualTree();
            ModelLayer.RemoveFromVisualTree();
            TemporaryLayer.RemoveFromVisualTree();
        }

        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        private void Initialize([NotNull] Editor editor)
        {
            editor.SetViewSize((int)ActualWidth, (int)ActualHeight);
        }
    }

    /// <summary>Implements drawing commands for SVG path.</summary>
    public sealed partial class InteractiveInkCanvas : IPath
    {
        private bool IsInFigure { get; set; }
        private CanvasPathBuilder PathBuilder { get; set; }

        public void MoveTo(float x, float y)
        {
            PathBuilder.SetFilledRegionDetermination(CanvasFilledRegionDetermination.Winding);
            PathBuilder.SetSegmentOptions(CanvasFigureSegmentOptions.None);
            if (IsInFigure)
            {
                PathBuilder.EndFigure(CanvasFigureLoop.Open);
            }

            PathBuilder.BeginFigure(x, y);
            IsInFigure = true;
        }

        public void LineTo(float x, float y)
        {
            PathBuilder.AddLine(x, y);
        }

        public void CurveTo(float x1, float y1, float x2, float y2, float x, float y)
        {
            PathBuilder.AddCubicBezier(new Vector2(x1, y1), new Vector2(x2, y2), new Vector2(x, y));
        }

        public void QuadTo(float x1, float y1, float x, float y)
        {
            PathBuilder.AddQuadraticBezier(new Vector2(x1, y1), new Vector2(x, y));
        }

        public void ArcTo(float rx, float ry, float phi, bool fA, bool fS, float x, float y)
        {
            throw new NotImplementedException();
        }

        public void ClosePath()
        {
            PathBuilder.EndFigure(CanvasFigureLoop.Closed);
            IsInFigure = false;
        }

        public uint UnsupportedOperations => (uint)PathOperation.ARC_OPS;

        [CanBeNull]
        private static CanvasGeometry CreateGeometry(IPath path)
        {
            if (!(path is InteractiveInkCanvas canvas))
            {
                return null;
            }

            if (!canvas.IsInFigure)
            {
                return CanvasGeometry.CreatePath(canvas.PathBuilder);
            }

            canvas.PathBuilder.EndFigure(CanvasFigureLoop.Open);
            canvas.IsInFigure = false;

            return CanvasGeometry.CreatePath(canvas.PathBuilder);
        }
    }

    /// <summary>Implements a render target for interactive ink renderer.</summary>
    public sealed partial class InteractiveInkCanvas : IRenderTarget
    {
        public void Invalidate(Renderer renderer, int x, int y, int width, int height, LayerType layers)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Invalidate(new Rect(x, y, width, height), layers))
                .AsTask();
        }

        public void Invalidate(Renderer renderer, LayerType layers)
        {
            Invalidate(renderer, 0, 0, (int)ActualWidth, (int)ActualHeight, layers);
        }

        private void Invalidate(Rect region, LayerType layers)
        {
            if (layers.HasFlag(LayerType.BACKGROUND))
            {
                BackgroundLayer.Invalidate(region.Clamp(BackgroundLayer));
            }

            if (layers.HasFlag(LayerType.CAPTURE))
            {
                CaptureLayer.Invalidate(region.Clamp(CaptureLayer));
            }

            if (layers.HasFlag(LayerType.MODEL))
            {
                ModelLayer.Invalidate(region.Clamp(ModelLayer));
            }

            if (layers.HasFlag(LayerType.TEMPORARY))
            {
                TemporaryLayer.Invalidate(region.Clamp(TemporaryLayer));
            }
        }
    }

    /// <summary>Handles pointer events on canvas.</summary>
    public sealed partial class InteractiveInkCanvas
    {
        private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (!(sender is UIElement element))
            {
                return;
            }

            Editor.Typeset(e.GetPosition(element));
            e.Handled = true;
        }

        private void OnPointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            if (!(sender is UIElement element))
            {
                return;
            }

            Editor.PointerCancel(e.GetCurrentPoint(element));
            e.Handled = true;
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!(sender is UIElement element))
            {
                return;
            }

            Editor.PointerMove(e.GetCurrentPoint(element));
            e.Handled = true;
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!(sender is UIElement element))
            {
                return;
            }

            Editor.PointerDown(e.GetCurrentPoint(element));
            e.Handled = true;
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!(sender is UIElement element))
            {
                return;
            }

            Editor.PointerUp(e.GetCurrentPoint(element));
        }
    }

    /// <summary>Handles regional invalidation events on canvas.</summary>
    public sealed partial class InteractiveInkCanvas
    {
        private void OnRegionsInvalidated(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs args)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var layer = sender.Name switch
                {
                    nameof(BackgroundLayer) => LayerType.BACKGROUND,
                    nameof(CaptureLayer) => LayerType.CAPTURE,
                    nameof(ModelLayer) => LayerType.MODEL,
                    nameof(TemporaryLayer) => LayerType.TEMPORARY,
                    _ => LayerType.LayerType_ALL
                };
                foreach (var region in args.InvalidatedRegions)
                {
                    if (region.Width <= 0 || region.Height <= 0)
                    {
                        continue;
                    }

                    var clamped = region.Clamp(sender);
                    using var session = Session = sender.CreateDrawingSession(clamped);
                    session.Antialiasing = CanvasAntialiasing.Antialiased;
                    session.TextAntialiasing = CanvasTextAntialiasing.Auto;
                    Editor.Renderer.Draw(clamped, layer, this);
                    session.Flush();
                }
            }).AsTask();
        }
    }

    /// <summary>Handles lifecycle events on this control.</summary>
    public sealed partial class InteractiveInkCanvas
    {
        private void OnLoaded(object sender, RoutedEventArgs _)
        {
            (sender as InteractiveInkCanvas)?.Initialize(Editor);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            (sender as InteractiveInkCanvas)?.Editor.SetViewSize((int)e.NewSize.Width, (int)e.NewSize.Height);
        }

        private void OnUnloaded(object sender, RoutedEventArgs _)
        {
            (sender as IDisposable)?.Dispose();
        }
    }
}
