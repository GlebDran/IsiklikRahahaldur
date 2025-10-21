using System.Globalization;

namespace IsiklikRahahaldur;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Устанавливаем культуру для корректного отображения чисел и дат
        CultureInfo.CurrentCulture = new CultureInfo("et-EE");
        CultureInfo.CurrentUICulture = new CultureInfo("et-EE");

        MainPage = new AppShell();
    }
}

