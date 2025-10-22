using Microsoft.Extensions.Logging;
using IsiklikRahahaldur.Services;
using IsiklikRahahaldur.ViewModels;
using IsiklikRahahaldur.Views;
using Microcharts.Maui;

namespace IsiklikRahahaldur;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMicrocharts()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Регистрация сервисов, ViewModel'ов и страниц (остается без изменений)
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddTransient<AddTransactionViewModel>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<AddTransactionPage>();
        builder.Services.AddTransient<CategoriesViewModel>();
        builder.Services.AddTransient<CategoriesPage>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<AddTransactionPage>();

        return builder.Build();
    }
}


