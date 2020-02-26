using Windows.UI.Xaml.Input;
using MyScript.IInk;
using MyScript.InteractiveInk.UI.Extensions;
using MyScript.InteractiveInk.ViewModels;

namespace MyScript.InteractiveInk.Views.Controls
{
    public sealed partial class MainCommandBar
    {
        private MainViewModel _viewModel;

        public MainCommandBar()
        {
            InitializeComponent();
        }

        private Editor Editor => ViewModel.Editor;
        private MainViewModel ViewModel => _viewModel ??= DataContext as MainViewModel;

        private void ClearAllCommand_OnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (!Editor.IsIdle())
            {
                Editor.WaitForIdle();
            }

            Editor.Clear();
        }

        private void RedoCommand_OnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (!Editor.IsIdle())
            {
                Editor.WaitForIdle();
            }

            Editor.Redo();
        }

        private void TypesetCommand_OnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (!Editor.IsIdle())
            {
                Editor.WaitForIdle();
            }

            Editor.Typeset();
        }

        private void UndoCommand_OnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            if (!Editor.IsIdle())
            {
                Editor.WaitForIdle();
            }

            Editor.Undo();
        }
    }
}
