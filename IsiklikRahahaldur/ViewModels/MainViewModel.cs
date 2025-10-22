using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IsiklikRahahaldur.Services;
using IsiklikRahahaldur.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace IsiklikRahahaldur.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private ObservableCollection<TransactionDisplayViewModel> _transactions;

        [ObservableProperty]
        private decimal _balance;

        public MainViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Мой кошелек";
            Transactions = new ObservableCollection<TransactionDisplayViewModel>();
        }

        [RelayCommand]
        private async Task LoadTransactionsAsync()
        {
            var transactionsFromDb = await _databaseService.GetTransactionsAsync();
            var categoriesFromDb = await _databaseService.GetCategoriesAsync();
            var categoriesDict = categoriesFromDb.ToDictionary(c => c.Id, c => c.Name);

            Transactions.Clear();

            foreach (var t in transactionsFromDb.OrderByDescending(x => x.Date))
            {
                Transactions.Add(new TransactionDisplayViewModel
                {
                    Transaction = t,
                    CategoryName = categoriesDict.TryGetValue(t.CategoryId, out var name) ? name : "Без категории"
                });
            }
            CalculateBalance();
        }

        private void CalculateBalance()
        {
            decimal totalIncome = Transactions.Where(t => t.Transaction.IsIncome).Sum(t => t.Transaction.Amount);
            decimal totalExpense = Transactions.Where(t => !t.Transaction.IsIncome).Sum(t => t.Transaction.Amount);
            Balance = totalIncome - totalExpense;
        }

        [RelayCommand]
        private async Task GoToAddTransactionAsync(bool isIncome)
        {
            // --- ИЗМЕНЕНО ---
            // Передаем ID=0, чтобы страница знала, что это НОВАЯ транзакция
            var navigationParameter = new Dictionary<string, object>
            {
                { "IsIncome", isIncome },
                { "TransactionId", 0 }
            };
            await Shell.Current.GoToAsync(nameof(AddTransactionPage), navigationParameter);
        }

        [RelayCommand]
        private async Task DeleteTransactionAsync(TransactionDisplayViewModel transactionVM)
        {
            if (transactionVM is null) return;

            // Спрашиваем подтверждение
            bool answer = await Shell.Current.DisplayAlert("Подтверждение", $"Вы уверены, что хотите удалить '{transactionVM.Transaction.Description}'?", "Да", "Нет");
            if (!answer) return;

            await _databaseService.DeleteTransactionAsync(transactionVM.Transaction);
            Transactions.Remove(transactionVM);
            CalculateBalance();
        }

        [RelayCommand]
        private async Task GoToEditTransactionAsync(TransactionDisplayViewModel transactionVM)
        {
            if (transactionVM is null) return;

            // --- ИЗМЕНЕНО ---
            // Убираем уведомление и передаем ID существующей транзакции
            var navigationParameter = new Dictionary<string, object>
            {
                { "TransactionId", transactionVM.Transaction.Id }
            };
            await Shell.Current.GoToAsync(nameof(AddTransactionPage), navigationParameter);
        }
    }
}

