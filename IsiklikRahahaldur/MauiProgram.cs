using IsiklikRahahaldur.Services; 
using IsiklikRahahaldur.ViewModels; 
using IsiklikRahahaldur.Views; 

namespace IsiklikRahahaldur;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // <-- НАЧАЛО ИЗМЕНЕНИЙ -->
        // Регистрируем наш сервис как Singleton (один экземпляр на все приложение)
        builder.Services.AddSingleton<DatabaseService>();

        // Регистрируем View и ViewModel
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<MainViewModel>();
        // <-- КОНЕЦ ИЗМЕНЕНИЙ -->

        return builder.Build();
    }
}
