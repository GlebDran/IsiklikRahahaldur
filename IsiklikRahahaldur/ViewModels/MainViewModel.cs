using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IsiklikRahahaldur.Services;
using IsiklikRahahaldur.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Transaction = IsiklikRahahaldur.Models.Transaction;

namespace IsiklikRahahaldur.ViewModels;

// Мы убрали IRecipient, т.к. больше не получаем сообщения
public partial class MainViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private ObservableCollection<Transaction> _transactions;

    [ObservableProperty]
    private decimal _balance;

    public MainViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "Мой кошелек";
        Transactions = new ObservableCollection<Transaction>();
    }

    [RelayCommand]
    private async Task LoadTransactionsAsync()
    {
        var transactionsFromDb = await _databaseService.GetTransactionsAsync();
        Transactions.Clear();
        foreach (var t in transactionsFromDb.OrderByDescending(x => x.Date)) // Сортируем по дате
        {
            Transactions.Add(t);
        }
        CalculateBalance();
    }

    private void CalculateBalance()
    {
        decimal totalIncome = Transactions.Where(t => t.IsIncome).Sum(t => t.Amount);
        decimal totalExpense = Transactions.Where(t => !t.IsIncome).Sum(t => t.Amount);
        Balance = totalIncome - totalExpense;
    }

    [RelayCommand]
    private async Task GoToAddTransactionAsync(bool isIncome)
    {
        var navigationParameter = new Dictionary<string, object>
        {
            { "IsIncome", isIncome }
        };
        await Shell.Current.GoToAsync(nameof(AddTransactionPage), navigationParameter);
    }
}

