using IsiklikRahahaldur.Views;

namespace IsiklikRahahaldur;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Регистрируем маршрут для страницы добавления транзакции
        Routing.RegisterRoute(nameof(AddTransactionPage), typeof(AddTransactionPage));
    }
}

