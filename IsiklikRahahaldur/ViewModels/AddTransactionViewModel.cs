using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IsiklikRahahaldur.Models;
using IsiklikRahahaldur.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace IsiklikRahahaldur.ViewModels;

[QueryProperty(nameof(IsIncome), "IsIncome")]
public partial class AddTransactionViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private Transaction _transaction;

    [ObservableProperty]
    private bool _isIncome;

    // Новые свойства для списка категорий и выбранной категории
    [ObservableProperty]
    private ObservableCollection<Category> _categories;

    [ObservableProperty]
    private Category _selectedCategory;

    public AddTransactionViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        Transaction = new Transaction();
        Categories = new ObservableCollection<Category>();

        // Загружаем категории при создании ViewModel
        _ = LoadCategoriesAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        var cats = await _databaseService.GetCategoriesAsync();
        Categories.Clear();
        foreach (var cat in cats)
        {
            Categories.Add(cat);
        }
    }

    partial void OnIsIncomeChanged(bool value)
    {
        Transaction.IsIncome = value;
        Title = value ? "Добавить доход" : "Добавить расход";
    }

    [RelayCommand]
    private async Task SaveTransactionAsync()
    {
        if (SelectedCategory is null)
        {
            await Shell.Current.DisplayAlert("Ошибка", "Пожалуйста, выберите категорию.", "OK");
            return;
        }

        if (Transaction.Amount <= 0 || string.IsNullOrWhiteSpace(Transaction.Description))
        {
            await Shell.Current.DisplayAlert("Ошибка", "Пожалуйста, введите описание и сумму.", "OK");
            return;
        }

        Transaction.Date = System.DateTime.Now;
        // Присваиваем ID выбранной категории нашей транзакции
        Transaction.CategoryId = SelectedCategory.Id;

        await _databaseService.SaveTransactionAsync(Transaction);

        await Shell.Current.GoToAsync("..");
    }
}

