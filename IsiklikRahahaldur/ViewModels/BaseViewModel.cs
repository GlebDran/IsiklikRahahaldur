using CommunityToolkit.Mvvm.ComponentModel;

namespace IsiklikRahahaldur.ViewModels
{
    // Указываем, что наш базовый ViewModel наследуется от ObservableObject
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _title;

        public bool IsNotBusy => !_isBusy;
    }
}

