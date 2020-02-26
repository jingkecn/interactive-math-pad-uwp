using System;
using System.IO;
using System.Numerics;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Microsoft.Toolkit.Helpers;
using MyScript.IInk;
using MyScript.InteractiveInk.Annotations;
using MyScript.InteractiveInk.Common.ViewModels;
using MyScript.InteractiveInk.UI.Services;

namespace MyScript.InteractiveInk.ViewModels
{
    public sealed partial class MainViewModel : Observable
    {
        private bool _canRedo;
        private bool _canUndo;
        private Editor _editor;

        public bool CanRedo
        {
            get => _canRedo;
            set => Set(ref _canRedo, value, nameof(CanRedo));
        }

        public bool CanUndo
        {
            get => _canUndo;
            set => Set(ref _canUndo, value, nameof(CanUndo));
        }

        public Editor Editor
        {
            get => _editor;
            set => Set(ref _editor, value, nameof(Editor));
        }
    }

    public sealed partial class MainViewModel : IDisposable
    {
        private static Vector2 Dpi => DisplayInformationService.GetDpi2();

        public void Dispose()
        {
        }

        public void Initialize([NotNull] CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
        }

        public void Initialize([NotNull] IRenderTarget target)
        {
            var engine = ((App)Application.Current).Engine;
            Initialize(Editor = engine.CreateEditor(engine.CreateRenderer(Dpi.X, Dpi.Y, target)));
        }

        public void Initialize([NotNull] Editor editor)
        {
            editor.SetFontMetricsProvider(Singleton<FontMetricsService>.Instance);
            var path = Path.Combine(ApplicationData.Current.LocalFolder.Path, $"{Path.GetRandomFileName()}.iink");
            editor.Part = editor.Engine.CreatePackage(path).CreatePart("Text Document");
            editor.AddListener(this);
        }
    }

    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class MainViewModel : IEditorListener
    {
        private CoreDispatcher Dispatcher { get; set; }

        public void PartChanged(Editor editor)
        {
            Dispatcher?.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                CanRedo = Editor.CanRedo();
                CanUndo = Editor.CanUndo();
            })?.AsTask();
        }

        public void ContentChanged(Editor editor, string[] blockIds)
        {
            Dispatcher?.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                CanRedo = Editor.CanRedo();
                CanUndo = Editor.CanUndo();
            })?.AsTask();
        }

        public void OnError(Editor editor, string blockId, string message)
        {
            Dispatcher?.RunAsync(CoreDispatcherPriority.Normal,
                async () => await new MessageDialog(message, blockId).ShowAsync())?.AsTask();
        }
    }

    public sealed partial class MainViewModel : IEditorListener2
    {
        public void SelectionChanged(Editor editor, string[] blockIds)
        {
        }

        public void ActiveBlockChanged(Editor editor, string blockId)
        {
        }
    }
}
