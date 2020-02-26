using System.ComponentModel;
using System.Runtime.CompilerServices;
using MyScript.InteractiveInk.Annotations;

namespace MyScript.InteractiveInk.Common.ViewModels
{
    public abstract partial class Observable : IObservable
    {
        public void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(value, storage))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }
    }

    public abstract partial class Observable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
