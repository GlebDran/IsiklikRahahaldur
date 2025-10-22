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
        // �������� ViewModel � �������� ������� ��������
        if (BindingContext is CategoriesViewModel vm)
        {
            // ���������� ExecuteAsync, ���� ������� �����������,
            // ��� Execute, ���� ����������. � ��� ��� AsyncRelayCommand.
            vm.LoadCategoriesCommand.Execute(null);
        }
    }
}