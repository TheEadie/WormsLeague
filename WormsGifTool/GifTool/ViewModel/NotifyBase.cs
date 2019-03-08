using System.ComponentModel;
using System.Runtime.CompilerServices;
using GifTool.Annotations;

namespace GifTool.ViewModel
{
    internal abstract class NotifyBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void SetProperty<T>(out T backingField, T value, [CallerMemberName] string propertyName = null)
        {
            backingField = value;
            OnPropertyChanged(propertyName);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
