namespace IsiklikRahahaldur.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        // Устанавливаем BindingContext. 
        // В XAML это сделано статически, но можно и программно:
        // this.BindingContext = new ViewModels.MainViewModel();
    }
}