using System.Runtime.CompilerServices;

namespace MyScript.InteractiveInk.Common.ViewModels
{
    public interface IObservable
    {
        void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null);
    }
}
