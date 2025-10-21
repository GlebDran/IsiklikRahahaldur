using IsiklikRahahaldur.ViewModels;

namespace IsiklikRahahaldur.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    // Этот метод вызывается каждый раз, когда страница появляется на экране
    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Принудительно запускаем команду загрузки транзакций
        _viewModel.LoadTransactionsCommand.Execute(null);
    }
}

