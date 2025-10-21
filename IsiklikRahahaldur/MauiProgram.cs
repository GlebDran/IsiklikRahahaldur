using Microsoft.Extensions.Logging;
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

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Регистрация сервиса для базы данных
        builder.Services.AddSingleton<DatabaseService>();

        // Регистрация ViewModel'ов
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddTransient<AddTransactionViewModel>(); // Transient, т.к. страница создается каждый раз заново

        // Регистрация страниц (Views)
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<AddTransactionPage>(); // Transient для страниц, которые не должны "запоминать" состояние

        return builder.Build();
    }
}

