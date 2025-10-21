using IsiklikRahahaldur.Views;

namespace IsiklikRahahaldur;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Указываем, что стартовая страница теперь MainPage
        MainPage = new AppShell();
    }
}