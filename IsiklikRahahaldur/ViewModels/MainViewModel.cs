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

        // Теперь коллекция хранит нашу новую "обертку"
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
            // Сначала загружаем все транзакции и категории
            var transactionsFromDb = await _databaseService.GetTransactionsAsync();
            var categoriesFromDb = await _databaseService.GetCategoriesAsync();

            // Превращаем список категорий в словарь для быстрого поиска
            var categoriesDict = categoriesFromDb.ToDictionary(c => c.Id, c => c.Name);

            Transactions.Clear();

            // "Склеиваем" транзакции с именами их категорий
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
            // Логика расчета баланса теперь использует вложенный объект Transaction
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
    }
}

