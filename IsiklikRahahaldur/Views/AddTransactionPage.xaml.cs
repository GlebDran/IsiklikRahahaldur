using IsiklikRahahaldur.ViewModels;

namespace IsiklikRahahaldur.Views;

public partial class AddTransactionPage : ContentPage
{
    public AddTransactionPage(AddTransactionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

