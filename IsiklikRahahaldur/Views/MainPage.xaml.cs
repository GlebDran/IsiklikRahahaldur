using IsiklikRahahaldur.ViewModels;

namespace IsiklikRahahaldur.Views;

public partial class MainPage : ContentPage
{
    // Этот конструктор получает MainViewModel через dependency injection
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();

        // Эта строка - самая важная. Она связывает View и ViewModel.
        BindingContext = viewModel;
    }
}

