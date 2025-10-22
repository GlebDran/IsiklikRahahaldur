using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IsiklikRahahaldur.Models;
using IsiklikRahahaldur.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace IsiklikRahahaldur.ViewModels;

[QueryProperty("IsIncome", "IsIncome")]
[QueryProperty("TransactionId", "TransactionId")]
public partial class AddTransactionViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private Transaction _transaction;

    [ObservableProperty]
    private ObservableCollection<Category> _categories;

    [ObservableProperty]
    private Category _selectedCategory;

    private bool _isIncome;
    public bool IsIncome
    {
        get => _isIncome;
        set => SetProperty(ref _isIncome, value);
    }

    // --- ИЗМЕНЕНИЕ: Устанавливаем начальное значение -1 ---
    private int _transactionId = -1;
    public int TransactionId
    {
        get => _transactionId;
        set
        {
            // Теперь (0 != -1) вернет true, и LoadDataAsync() запустится
            if (SetProperty(ref _transactionId, value))
            {
                _ = LoadDataAsync();
            }
        }
    }

    public AddTransactionViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        Transaction = new Transaction();
        Categories = new ObservableCollection<Category>();
    }

    private async Task LoadDataAsync()
    {
        var cats = await _databaseService.GetCategoriesAsync();
        Categories.Clear();
        foreach (var cat in cats)
        {
            Categories.Add(cat);
        }

        if (TransactionId == 0)
        {
            // --- РЕЖИМ СОЗДАНИЯ ---
            Transaction = new Transaction();
            Transaction.IsIncome = IsIncome;
            Title = IsIncome ? "Добавить доход" : "Добавить расход";
        }
        else
        {
            // --- РЕЖИМ РЕДАКТИРОВАНИЯ ---
            Transaction = await _databaseService.GetTransactionByIdAsync(TransactionId);
            SelectedCategory = Categories.FirstOrDefault(c => c.Id == Transaction.CategoryId);
            Title = "Редактировать";
        }
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

        if (Transaction.Id == 0)
        {
            Transaction.Date = System.DateTime.Now;
        }

        Transaction.CategoryId = SelectedCategory.Id;

        await _databaseService.SaveTransactionAsync(Transaction);

        await Shell.Current.GoToAsync("..");
    }
}

