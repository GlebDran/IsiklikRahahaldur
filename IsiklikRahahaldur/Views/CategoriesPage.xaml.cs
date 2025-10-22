using IsiklikRahahaldur.ViewModels;

namespace IsiklikRahahaldur.Views;

public partial class CategoriesPage : ContentPage
{
    public CategoriesPage(CategoriesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}