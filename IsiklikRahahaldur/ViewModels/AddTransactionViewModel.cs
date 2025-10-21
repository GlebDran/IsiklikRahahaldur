using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IsiklikRahahaldur.Models;
using IsiklikRahahaldur.Services;
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

    public AddTransactionViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        Transaction = new Transaction();
    }

    partial void OnIsIncomeChanged(bool value)
    {
        Transaction.IsIncome = value;
        Title = value ? "Добавить доход" : "Добавить расход";
    }

    [RelayCommand]
    private async Task SaveTransactionAsync()
    {
        if (Transaction.Amount <= 0 || string.IsNullOrWhiteSpace(Transaction.Description))
        {
            await Shell.Current.DisplayAlert("Ошибка", "Пожалуйста, введите описание и сумму.", "OK");
            return;
        }

        Transaction.Date = System.DateTime.Now;

        await _databaseService.SaveTransactionAsync(Transaction);

        // Просто возвращаемся назад. Главный экран обновится сам.
        await Shell.Current.GoToAsync("..");
    }
}

