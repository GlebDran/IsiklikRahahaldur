using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IsiklikRahahaldur.ViewModels
{
    /// <summary>
    /// Базовый класс для всех ViewModel. Реализует интерфейс INotifyPropertyChanged
    /// для автоматического обновления UI при изменении свойств.
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}