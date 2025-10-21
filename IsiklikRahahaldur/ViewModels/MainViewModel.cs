using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using IsiklikRahahaldur.Messages;
// Вот это исправление. Мы говорим, что Transaction - это наш класс.
using Transaction = IsiklikRahahaldur.Models.Transaction;
using IsiklikRahahaldur.Services;
using IsiklikRahahaldur.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace IsiklikRahahaldur.ViewModels;

// Добавляем IRecipient, чтобы ViewModel мог получать сообщения
public partial class MainViewModel : BaseViewModel, IRecipient<TransactionAddedMessage>
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

        // Подписываемся на получение сообщений
        WeakReferenceMessenger.Default.Register(this);

        // Загружаем данные при старте
        _ = LoadTransactionsAsync();
    }

    // Этот метод автоматически вызывается, когда приходит новое сообщение
    public void Receive(TransactionAddedMessage message)
    {
        // Важно: обновление интерфейса должно происходить в основном потоке
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Transactions.Add(message.Value);
            CalculateBalance();
        });
    }

    [RelayCommand]
    private async Task LoadTransactionsAsync()
    {
        var transactionsFromDb = await _databaseService.GetTransactionsAsync();
        Transactions.Clear();
        foreach (var t in transactionsFromDb)
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

