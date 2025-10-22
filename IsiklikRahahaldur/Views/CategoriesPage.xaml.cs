using IsiklikRahahaldur.ViewModels;

namespace IsiklikRahahaldur.Views;

public partial class CategoriesPage : ContentPage
{
    public CategoriesPage(CategoriesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Получаем ViewModel и вызываем команду загрузки
        if (BindingContext is CategoriesViewModel vm)
        {
            // Используем ExecuteAsync, если команда асинхронная,
            // или Execute, если синхронная. У нас она AsyncRelayCommand.
            vm.LoadCategoriesCommand.Execute(null);
        }
    }
}