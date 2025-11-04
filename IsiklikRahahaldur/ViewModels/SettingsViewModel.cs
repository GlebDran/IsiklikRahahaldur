using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace IsiklikRahahaldur.ViewModels
{
    public partial class SettingsViewModel : BaseViewModel
    {
        public ObservableCollection<string> Themes { get; }
        private string _selectedTheme;

        // Это свойство будет связано с Picker'ом на странице
        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                // Когда пользователь выбирает новую тему, мы обновляем свойство
                // и вызываем команду UpdateTheme
                if (SetProperty(ref _selectedTheme, value))
                {
                    UpdateTheme(value);
                }
            }
        }

        public SettingsViewModel()
        {
            Title = "Настройки";
            Themes = new ObservableCollection<string> { "Светлая", "Темная", "Как в системе" };

            // Устанавливаем текущую выбранную тему
            _selectedTheme = Application.Current.UserAppTheme switch
            {
                AppTheme.Light => "Светлая",
                AppTheme.Dark => "Темная",
                _ => "Как в системе"
            };
        }

        private void UpdateTheme(string themeName)
        {
            AppTheme theme = themeName switch
            {
                "Светлая" => AppTheme.Light,
                "Темная" => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };

            // Переключение темы должно происходить в главном потоке
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Application.Current.UserAppTheme = theme;
            });
        }
    }
}