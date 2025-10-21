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
            var navigationParameter = new Dictionary<string, object>
            {
                { "IsIncome", isIncome }
            };
            await Shell.Current.GoToAsync(nameof(AddTransactionPage), navigationParameter);
        }

        // --- НОВЫЕ КОМАНДЫ ---

        [RelayCommand]
        private async Task DeleteTransactionAsync(TransactionDisplayViewModel transactionVM)
        {
            if (transactionVM is null) return;

            // Удаляем из базы данных
            await _databaseService.DeleteTransactionAsync(transactionVM.Transaction);
            // Удаляем из списка на экране
            Transactions.Remove(transactionVM);
            // Пересчитываем баланс
            CalculateBalance();
        }

        [RelayCommand]
        private async Task GoToEditTransactionAsync(TransactionDisplayViewModel transactionVM)
        {
            if (transactionVM is null) return;

            // Пока что эта функция не реализована, мы сделаем ее на следующем шаге.
            // Сейчас просто покажем уведомление.
            await Shell.Current.DisplayAlert("В разработке", "Функция редактирования будет добавлена в следующем шаге.", "OK");
        }
    }
}

