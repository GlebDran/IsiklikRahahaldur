using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IsiklikRahahaldur.Services;
using IsiklikRahahaldur.Views;
using IsiklikRahahaldur.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microcharts;
using SkiaSharp;
using System.Collections.Generic;
using System;
using System.Globalization; // <-- Добавлен using для CultureInfo

namespace IsiklikRahahaldur.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private ObservableCollection<TransactionDisplayViewModel> _transactions;

        [ObservableProperty]
        private decimal _balance;

        [ObservableProperty]
        private Chart _expensesChart;

        // --- НОВОЕ: Свойства для фильтра и сумм за период ---
        [ObservableProperty]
        private TimePeriod _selectedPeriod = TimePeriod.Month; // По умолчанию показываем за месяц

        [ObservableProperty]
        private decimal _periodIncome;

        [ObservableProperty]
        private decimal _periodExpense;
        // --- КОНЕЦ НОВОГО ---

        private Dictionary<string, SKColor> _categoryColors = new Dictionary<string, SKColor>();
        private Random _random = new Random();

        public MainViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Мой кошелек";
            Transactions = new ObservableCollection<TransactionDisplayViewModel>();
            ExpensesChart = new PieChart { Entries = new List<ChartEntry>() };
        }

        // --- НОВОЕ: Команда для смены периода ---
        [RelayCommand]
        private async Task SetPeriodAsync(string period)
        {
            TimePeriod newPeriod;
            switch (period?.ToLower())
            {
                case "today": newPeriod = TimePeriod.Today; break;
                case "week": newPeriod = TimePeriod.Week; break;
                case "month": newPeriod = TimePeriod.Month; break;
                case "year": newPeriod = TimePeriod.Year; break;
                case "all": newPeriod = TimePeriod.All; break;
                // TODO: Добавить обработку "custom" / "Выбрать..."
                default: newPeriod = TimePeriod.Month; break; // По умолчанию - месяц
            }

            if (SelectedPeriod != newPeriod)
            {
                SelectedPeriod = newPeriod;
                await LoadTransactionsAsync(); // Перезагружаем транзакции для нового периода
            }
        }
        // --- КОНЕЦ НОВОГО ---

        // --- ИЗМЕНЕНО: Добавлена фильтрация и управление IsBusy ---
        [RelayCommand]
        private async Task LoadTransactionsAsync()
        {
            if (IsBusy) return; // Предотвращаем повторный запуск, если уже загружается

            IsBusy = true;
            try // Оборачиваем в try..finally для управления IsBusy
            {
                var transactionsFromDb = await _databaseService.GetTransactionsAsync();
                var categoriesFromDb = await _databaseService.GetCategoriesAsync();
                var categoriesDict = categoriesFromDb.ToDictionary(c => c.Id, c => c.Name);

                // Фильтрация по дате
                DateTime startDate = DateTime.MinValue;
                DateTime endDate = DateTime.MaxValue;
                var now = DateTime.Now;

                switch (SelectedPeriod)
                {
                    case TimePeriod.Today:
                        startDate = now.Date;
                        endDate = startDate.AddDays(1);
                        break;
                    case TimePeriod.Week:
                        // Неделя начинается с понедельника (для CultureInfo "et-EE")
                        int diff = (7 + (now.DayOfWeek - CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)) % 7;
                        startDate = now.AddDays(-1 * diff).Date;
                        endDate = startDate.AddDays(7);
                        break;
                    case TimePeriod.Month:
                        startDate = new DateTime(now.Year, now.Month, 1);
                        endDate = startDate.AddMonths(1);
                        break;
                    case TimePeriod.Year:
                        startDate = new DateTime(now.Year, 1, 1);
                        endDate = startDate.AddYears(1);
                        break;
                    case TimePeriod.All:
                        // startDate и endDate уже Min/Max Value
                        break;
                        // TODO: Обработка TimePeriod.Custom
                }

                // Отбираем транзакции за нужный период
                var filteredTransactions = transactionsFromDb
                    .Where(t => t.Date >= startDate && t.Date < endDate) // Используем >= и <
                    .OrderByDescending(x => x.Date)
                    .ToList();

                Transactions.Clear();
                // Используем отфильтрованный список filteredTransactions
                foreach (var t in filteredTransactions)
                {
                    Transactions.Add(new TransactionDisplayViewModel
                    {
                        Transaction = t,
                        CategoryName = categoriesDict.TryGetValue(t.CategoryId, out var name) ? name : "Без категории"
                    });
                }

                // Передаем отфильтрованный список для расчетов
                CalculateBalanceAndTotals(filteredTransactions);
                UpdateChart(filteredTransactions, categoriesDict);
            }
            finally
            {
                IsBusy = false; // Убеждаемся, что IsBusy сбросится, даже если была ошибка
            }
        }

        // --- ИЗМЕНЕНО: Переименован и обновлен для расчета сумм за период ---
        private void CalculateBalanceAndTotals(List<Transaction> transactions)
        {
            PeriodIncome = transactions.Where(t => t.IsIncome).Sum(t => t.Amount);
            PeriodExpense = transactions.Where(t => !t.IsIncome).Sum(t => t.Amount);
            Balance = PeriodIncome - PeriodExpense; // Баланс тоже считаем по отфильтрованным
        }

        // --- ИЗМЕНЕНО: Теперь принимает список транзакций ---
        private void UpdateChart(List<Transaction> transactions, Dictionary<int, string> categoryNames)
        {
            var expenseEntries = transactions
                .Where(t => !t.IsIncome && t.CategoryId != 0)
                .GroupBy(t => t.CategoryId)
                .Select(group =>
                {
                    string categoryName = categoryNames.TryGetValue(group.Key, out var name) ? name : "Категория?";
                    // --- ИЗМЕНИ ЭТУ СТРОКУ ---
                    string valueLabel = group.Sum(t => t.Amount).ToString("F2") + " €";
                    // --- КОНЕЦ ИЗМЕНЕНИЯ ---

                    return new ChartEntry((float)group.Sum(t => t.Amount))
                    {
                        Label = categoryName,
                        ValueLabel = valueLabel, // Используем переменную
                        Color = GetCategoryColor(categoryName)
                    };
                })
                .ToList();

            // Остальной код метода без изменений...
            ExpensesChart = new PieChart
            {
                Entries = expenseEntries,
                LabelTextSize = 28f,
                BackgroundColor = SKColors.Transparent,
                LabelMode = LabelMode.RightOnly
            };
        }

        private SKColor GetCategoryColor(string categoryName)
        {
            if (_categoryColors.TryGetValue(categoryName, out var color))
            {
                return color;
            }
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
            await LoadTransactionsAsync(); // Перезагружаем данные с учетом текущего фильтра
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

        [RelayCommand]
        private async Task GoToCategoriesAsync()
        {
            await Shell.Current.GoToAsync(nameof(CategoriesPage));
        }
    }
}