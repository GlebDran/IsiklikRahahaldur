using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IsiklikRahahaldur.Services;
using IsiklikRahahaldur.Views;
using IsiklikRahahaldur.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microcharts; // Убедитесь, что using есть
using SkiaSharp; // Убедитесь, что using есть
using System.Collections.Generic;
using System;
using System.Globalization; // Using для CultureInfo

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
        private Chart _spendingBarChart; // Используем BarChart

        [ObservableProperty]
        private TimePeriod _selectedPeriod = TimePeriod.Week; // По умолчанию - неделя

        [ObservableProperty]
        private decimal _periodIncome;

        [ObservableProperty]
        private decimal _periodExpense;

        private readonly SKColor _barChartColor = SKColor.Parse("#2ECC71"); // Зеленый из макета

        private Random _random = new Random();

        public MainViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Мой кошелек";
            Transactions = new ObservableCollection<TransactionDisplayViewModel>();
            SpendingBarChart = new BarChart { Entries = new List<ChartEntry>() };
            // Загружаем данные при инициализации, если нужно
            // Task.Run(async () => await LoadTransactionsAsync());
        }

        [RelayCommand]
        private async Task SetPeriodAsync(string period)
        {
            TimePeriod newPeriod;
            switch (period?.ToLower()) // Используем ToLower для надежности
            {
                // --- ИЗМЕНЕНО: Добавляем русские варианты ---
                case "today":
                case "сегодня":
                    newPeriod = TimePeriod.Today; break;
                case "week":
                case "неделя":
                    newPeriod = TimePeriod.Week; break;
                case "month":
                case "месяц":
                    newPeriod = TimePeriod.Month; break;
                case "year":
                case "год":
                    newPeriod = TimePeriod.Year; break;
                case "all":
                case "все":
                    newPeriod = TimePeriod.All; break;
                // TODO: Добавить обработку "custom" / "Выбрать..."
                default: newPeriod = TimePeriod.Week; break; // По умолчанию - неделя
            }


            if (SelectedPeriod != newPeriod)
            {
                SelectedPeriod = newPeriod;
                await LoadTransactionsAsync();
            }
        }

        [RelayCommand]
        public async Task LoadTransactionsAsync() // Сделал public
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                var transactionsFromDb = await _databaseService.GetTransactionsAsync();
                var categoriesFromDb = await _databaseService.GetCategoriesAsync();
                var categoriesDict = categoriesFromDb.ToDictionary(c => c.Id, c => c.Name);

                DateTime startDate = DateTime.MinValue;
                DateTime endDate = DateTime.MaxValue;
                var now = DateTime.Now;

                // Используем текущую культуру приложения (установлена в App.xaml.cs)
                var currentCulture = CultureInfo.CurrentCulture;
                var firstDayOfWeek = currentCulture.DateTimeFormat.FirstDayOfWeek; // В et-EE это Понедельник

                switch (SelectedPeriod)
                {
                    case TimePeriod.Today:
                        startDate = now.Date;
                        endDate = startDate.AddDays(1);
                        break;
                    case TimePeriod.Week:
                        // Рассчитываем начало недели относительно первого дня в культуре
                        int diff = (7 + (int)now.DayOfWeek - (int)firstDayOfWeek) % 7;
                        startDate = now.AddDays(-diff).Date;
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


                var filteredTransactions = transactionsFromDb
                    .Where(t => t.Date >= startDate && t.Date < endDate)
                    .OrderByDescending(x => x.Date)
                    .ToList();

                Transactions.Clear();
                foreach (var t in filteredTransactions)
                {
                    Transactions.Add(new TransactionDisplayViewModel
                    {
                        Transaction = t,
                        CategoryName = categoriesDict.TryGetValue(t.CategoryId, out var name) ? name : "Без категории"
                    });
                }

                CalculateBalanceAndTotals(transactionsFromDb); // Общий баланс
                CalculatePeriodTotals(filteredTransactions); // Суммы за период
                UpdateBarChart(filteredTransactions, startDate); // Обновляем график
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void CalculateBalanceAndTotals(List<Transaction> allTransactions)
        {
            decimal totalIncome = allTransactions.Where(t => t.IsIncome).Sum(t => t.Amount);
            decimal totalExpense = allTransactions.Where(t => !t.IsIncome).Sum(t => t.Amount);
            Balance = totalIncome - totalExpense;
        }

        private void CalculatePeriodTotals(List<Transaction> periodTransactions)
        {
            PeriodIncome = periodTransactions.Where(t => t.IsIncome).Sum(t => t.Amount);
            PeriodExpense = periodTransactions.Where(t => !t.IsIncome).Sum(t => t.Amount);
        }

        private void UpdateBarChart(List<Transaction> transactions, DateTime periodStartDate)
        {
            var expensesByDayOfWeek = transactions
                .Where(t => !t.IsIncome)
                .GroupBy(t => t.Date.DayOfWeek)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

            var chartEntries = new List<ChartEntry>();
            // --- ИЗМЕНЕНО: Русские сокращения дней недели ---
            string[] dayLabels = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
            DayOfWeek[] daysOrder = { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };


            for (int i = 0; i < daysOrder.Length; i++)
            {
                DayOfWeek currentDay = daysOrder[i];
                decimal totalAmount = expensesByDayOfWeek.TryGetValue(currentDay, out var amount) ? amount : 0;

                chartEntries.Add(new ChartEntry((float)totalAmount)
                {
                    Label = dayLabels[i], // "Пн", "Вт", ...
                    ValueLabel = totalAmount > 0 ? totalAmount.ToString("N0") : "0",
                    Color = _barChartColor
                });
            }

            SpendingBarChart = new BarChart
            {
                Entries = chartEntries,
                LabelTextSize = 12f,
                ValueLabelTextSize = 12f,
                BackgroundColor = SKColors.Transparent,
                LabelOrientation = Orientation.Horizontal,
                ValueLabelOrientation = Orientation.Horizontal,
                IsAnimated = false,
                Margin = 5,
                PointMode = PointMode.None,
                PointSize = 0,
            };
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
        private async Task GoToAddExpenseAsync()
        {
            await GoToAddTransactionAsync(false);
        }


        [RelayCommand]
        private async Task DeleteTransactionAsync(TransactionDisplayViewModel transactionVM)
        {
            if (transactionVM is null) return;

            // --- ИЗМЕНЕНО: Русский текст в подтверждении ---
            bool answer = await Shell.Current.DisplayAlert("Подтверждение", $"Удалить транзакцию '{transactionVM.Transaction.Description}'?", "Да", "Нет");
            if (!answer) return;

            await _databaseService.DeleteTransactionAsync(transactionVM.Transaction);
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

        [RelayCommand]
        private async Task GoToCategoriesAsync()
        {
            await Shell.Current.GoToAsync(nameof(CategoriesPage));
        }
    }
}