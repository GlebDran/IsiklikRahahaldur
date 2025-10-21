using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IsiklikRahahaldur.Models;
using IsiklikRahahaldur.Services;
using IsiklikRahahaldur.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace IsiklikRahahaldur.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private decimal _balance;

    public ObservableCollection<Transaction> Transactions { get; } = new();

    // Переменная для хранения новой транзакции, полученной со страницы добавления
    [ObservableProperty]
    private Transaction _newTransaction;

    public MainViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "Мой кошелек";
        LoadTransactionsAsync();
    }

    // Метод, который будет вызван, когда свойство NewTransaction изменится
    partial void OnNewTransactionChanged(Transaction value)
    {
        if (value != null)
        {
            Transactions.Add(value);
            CalculateBalance();
        }
    }


    private async void LoadTransactionsAsync()
    {
        var transactions = await _databaseService.GetTransactionsAsync();
        Transactions.Clear();
        foreach (var transaction in transactions)
        {
            Transactions.Add(transaction);
        }
        CalculateBalance();
    }

    private void CalculateBalance()
    {
        Balance = Transactions.Where(t => t.IsIncome).Sum(t => t.Amount) - Transactions.Where(t => !t.IsIncome).Sum(t => t.Amount);
    }

    [RelayCommand]
    private async Task GoToAddTransactionAsync(bool isIncome)
    {
        try
        {
            // Передаем параметр IsIncome на страницу добавления
            await Shell.Current.GoToAsync(nameof(AddTransactionPage), true, new Dictionary<string, object>
            {
                { "IsIncome", isIncome }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка навигации: {ex.Message}");
        }
    }
}

