using IsiklikRahahaldur.Views;

namespace IsiklikRahahaldur;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Регистрируем маршрут для страницы добавления транзакции
        Routing.RegisterRoute(nameof(AddTransactionPage), typeof(AddTransactionPage));
        Routing.RegisterRoute(nameof(CategoriesPage), typeof(CategoriesPage));

        // --- ДОБАВЛЕНА НОВАЯ СТРОКА ---
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
    }
}