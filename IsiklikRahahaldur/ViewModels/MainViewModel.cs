using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IsiklikRahahaldur.Services;
using IsiklikRahahaldur.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microcharts; // <-- Добавляем using
using SkiaSharp; // <-- Добавляем using
using System.Collections.Generic; // <-- Добавляем using
using System; // <-- Добавляем using

namespace IsiklikRahahaldur.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private ObservableCollection<TransactionDisplayViewModel> _transactions;

        [ObservableProperty]
        private decimal _balance;

        // --- НОВОЕ СВОЙСТВО ДЛЯ ГРАФИКА ---
        [ObservableProperty]
        private Chart _expensesChart;

        // Список для хранения цветов категорий
        private Dictionary<string, SKColor> _categoryColors = new Dictionary<string, SKColor>();
        private Random _random = new Random();

        public MainViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Мой кошелек";
            Transactions = new ObservableCollection<TransactionDisplayViewModel>();
            // Инициализируем пустой график, чтобы не было ошибок при запуске
            ExpensesChart = new PieChart { Entries = new List<ChartEntry>() };
        }

        [RelayCommand]
        private async Task LoadTransactionsAsync()
        {
            var transactionsFromDb = await _databaseService.GetTransactionsAsync();
            var categoriesFromDb = await _databaseService.GetCategoriesAsync();
            var categoriesDict = categoriesFromDb.ToDictionary(c => c.Id, c => c.Name);

            Transactions.Clear();
            List<Transaction> currentTransactions = new List<Transaction>(); // Список для расчета

            foreach (var t in transactionsFromDb.OrderByDescending(x => x.Date))
            {
                Transactions.Add(new TransactionDisplayViewModel
                {
                    Transaction = t,
                    CategoryName = categoriesDict.TryGetValue(t.CategoryId, out var name) ? name : "Без категории"
                });
                currentTransactions.Add(t);
            }

            CalculateBalance(currentTransactions);
            UpdateChart(currentTransactions, categoriesDict); // Обновляем график
        }

        // Принимает список транзакций, чтобы не обращаться к свойству Transactions
        private void CalculateBalance(List<Transaction> transactions)
        {
            decimal totalIncome = transactions.Where(t => t.IsIncome).Sum(t => t.Amount);
            decimal totalExpense = transactions.Where(t => !t.IsIncome).Sum(t => t.Amount);
            Balance = totalIncome - totalExpense;
        }

        // Метод для обновления данных графика
        private void UpdateChart(List<Transaction> transactions, Dictionary<int, string> categoryNames)
        {
            var expenseEntries = transactions
                .Where(t => !t.IsIncome && t.CategoryId != 0) // Берем только расходы с категориями
                .GroupBy(t => t.CategoryId)
                .Select(group =>
                {
                    string categoryName = categoryNames.TryGetValue(group.Key, out var name) ? name : "Категория?";
                    return new ChartEntry((float)group.Sum(t => t.Amount))
                    {
                        Label = categoryName,
                        ValueLabel = group.Sum(t => t.Amount).ToString("F2") + " €",
                        Color = GetCategoryColor(categoryName) // Используем постоянный цвет для категории
                    };
                })
                .ToList();

            ExpensesChart = new PieChart
            {
                Entries = expenseEntries,
                LabelTextSize = 28f, // Немного увеличим шрифт
                BackgroundColor = SKColors.Transparent,
                LabelMode = LabelMode.RightOnly // Метки справа от графика
            };
        }

        // Вспомогательный метод для получения постоянного случайного цвета для категории
        private SKColor GetCategoryColor(string categoryName)
        {
            if (_categoryColors.TryGetValue(categoryName, out var color))
            {
                return color;
            }

            // Генерируем красивый, не слишком темный цвет
            color = new SKColor((byte)_random.Next(100, 256), (byte)_random.Next(100, 256), (byte)_random.Next(100, 256));
            _categoryColors[categoryName] = color;
            return color;
        }


        [RelayCommand]
        private async Task GoToAddTransactionAsync(bool isIncome)
        {
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

            bool answer = await Shell.Current.DisplayAlert("Подтверждение", $"Вы уверены, что хотите удалить '{transactionVM.Transaction.Description}'?", "Да", "Нет");
            if (!answer) return;

            await _databaseService.DeleteTransactionAsync(transactionVM.Transaction);
            // Перезагружаем все данные, чтобы обновить и список, и график
            await LoadTransactionsAsync();
        }

        [RelayCommand]
        private async Task GoToEditTransactionAsync(TransactionDisplayViewModel transactionVM)
        {
            if (transactionVM is null) return;

            var navigationParameter = new Dictionary<string, object>
            {
                { "TransactionId", transactionVM.Transaction.Id }
            };
            await Shell.Current.GoToAsync(nameof(AddTransactionPage), navigationParameter);
        }
    }
}

