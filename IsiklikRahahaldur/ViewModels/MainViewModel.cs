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
using System.Globalization;

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
        private Chart _spendingBarChart;

        [ObservableProperty]
        private TimePeriod _selectedPeriod = TimePeriod.Week;

        [ObservableProperty]
        private decimal _periodIncome;

        [ObservableProperty]
        private decimal _periodExpense;

        // --- ИЗМЕНЕНО: Добавлены два цвета ---
        private readonly SKColor _incomeChartColor = SKColor.Parse("#2ECC71"); // Зеленый
        private readonly SKColor _expenseChartColor = SKColor.Parse("#E74C3C"); // Красный

        [RelayCommand]
        private async Task GoToSettingsAsync()
        {
            await Shell.Current.GoToAsync(nameof(SettingsPage));
        }
        public MainViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Мой кошелек";
            Transactions = new ObservableCollection<TransactionDisplayViewModel>();

            // --- ИСПРАВЛЕНИЕ КОНСТРУКТОРА ---
            // (Логика та же, просто используем новые переменные цвета)
            var initialEntries = new List<ChartEntry>();
            string[] dayLabels = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };

            for (int i = 0; i < dayLabels.Length; i++)
            {
                // Добавляем "заглушку" для дохода
                initialEntries.Add(new ChartEntry(0)
                {
                    Label = dayLabels[i],
                    ValueLabel = "0",
                    Color = _incomeChartColor
                });
                // Добавляем "заглушку" для расхода
                initialEntries.Add(new ChartEntry(0)
                {
                    Label = dayLabels[i],
                    ValueLabel = "",
                    Color = _expenseChartColor
                });
            }

            SpendingBarChart = new BarChart
            {
                Entries = initialEntries,
                LabelTextSize = 12f,
                ValueLabelTextSize = 12f,
                BackgroundColor = SKColors.Transparent,
                LabelOrientation = Orientation.Horizontal,
                ValueLabelOrientation = Orientation.Horizontal,
                IsAnimated = false,
                Margin = 5,
            };
        }

        [RelayCommand]
        private async Task SetPeriodAsync(string period)
        {
            TimePeriod newPeriod;
            switch (period?.ToLower())
            {
                case "today": case "сегодня": newPeriod = TimePeriod.Today; break;
                case "week": case "неделя": newPeriod = TimePeriod.Week; break;
                case "month": case "месяц": newPeriod = TimePeriod.Month; break;
                case "year": case "год": newPeriod = TimePeriod.Year; break;
                case "all": case "все": newPeriod = TimePeriod.All; break;
                default: newPeriod = TimePeriod.Week; break;
            }

            if (SelectedPeriod != newPeriod)
            {
                SelectedPeriod = newPeriod;
                await LoadTransactionsAsync();
            }
        }

        [RelayCommand]
        public async Task LoadTransactionsAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                var transactionsFromDb = await _databaseService.GetTransactionsAsync();
                var categoriesFromDb = await _databaseService.GetCategoriesAsync();
                var categoriesDict = categoriesFromDb.ToDictionary(c => c.Id, c => c.Name);

                if (!categoriesDict.ContainsKey(0))
                {
                    categoriesDict.Add(0, "Без категории");
                }

                DateTime startDate = DateTime.MinValue;
                DateTime endDate = DateTime.MaxValue;
                var now = DateTime.Now;
                var currentCulture = CultureInfo.CurrentCulture;
                var firstDayOfWeek = currentCulture.DateTimeFormat.FirstDayOfWeek;

                switch (SelectedPeriod)
                {
                    case TimePeriod.Today:
                        startDate = now.Date;
                        endDate = startDate.AddDays(1);
                        break;
                    case TimePeriod.Week:
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
                    case TimePeriod.All: break;
                }

                var filteredTransactions = transactionsFromDb
                    .Where(t => t.Date >= startDate && t.Date < endDate)
                    .OrderByDescending(x => x.Date)
                    .ToList();

                Transactions.Clear();
                foreach (var t in filteredTransactions)
                {
                    string categoryName = categoriesDict.TryGetValue(t.CategoryId, out var name) ? name : "Без категории";
                    string categoryIcon = GetIconForCategory(categoryName);

                    Transactions.Add(new TransactionDisplayViewModel
                    {
                        Transaction = t,
                        CategoryName = categoryName,
                        CategoryIcon = categoryIcon
                    });
                }

                CalculateBalanceAndTotals(transactionsFromDb);
                CalculatePeriodTotals(filteredTransactions);

                // --- ИЗМЕНЕНО: Передаем отфильтрованные транзакции в график ---
                UpdateBarChart(filteredTransactions, startDate);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private string GetIconForCategory(string categoryName)
        {
            switch (categoryName)
            {
                case "Продукты": return "food_icon.png";
                case "Транспорт": return "transport_icon.png";
                case "Дом": return "house_icon.png";
                case "Кафе и рестораны": return "restaurants_icon.png";
                case "Здоровье": return "health_icon.png";
                case "Подарки": return "gift_icon.png";
                case "Зарплата": return "salary_icon.png";
                default: return "other_icon.png";
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

        // --- ИЗМЕНЕНО: Полностью обновленный метод UpdateBarChart ---
        private void UpdateBarChart(List<Transaction> transactions, DateTime periodStartDate)
        {
            // 1. Группируем расходы по дням недели
            var expensesByDayOfWeek = transactions
                .Where(t => !t.IsIncome)
                .GroupBy(t => t.Date.DayOfWeek)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

            // 2. Группируем доходы по дням недели
            var incomesByDayOfWeek = transactions
                .Where(t => t.IsIncome)
                .GroupBy(t => t.Date.DayOfWeek)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

            var chartEntries = new List<ChartEntry>();
            string[] dayLabels = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
            DayOfWeek[] daysOrder = { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };
            var culture = new CultureInfo("ru-RU"); // Для корректного отображения чисел

            // Находим максимальные значения для корректной шкалы
            decimal maxIncome = 0;
            decimal maxExpense = 0;

            if (incomesByDayOfWeek.Any())
                maxIncome = incomesByDayOfWeek.Values.Max();

            if (expensesByDayOfWeek.Any())
                maxExpense = expensesByDayOfWeek.Values.Max();

            var maxChartValue = (float)Math.Max(maxIncome, maxExpense) * 1.2f; // +20% запаса
            if (maxChartValue == 0) maxChartValue = 100; // Минимальная высота, если данных нет

            for (int i = 0; i < daysOrder.Length; i++)
            {
                DayOfWeek currentDay = daysOrder[i];

                // Получаем доход
                decimal totalIncome = incomesByDayOfWeek.TryGetValue(currentDay, out var income) ? income : 0;

                // Получаем расход
                decimal totalExpense = expensesByDayOfWeek.TryGetValue(currentDay, out var expense) ? expense : 0;

                // Добавляем запись ДОХОДА (зеленый, положительный)
                chartEntries.Add(new ChartEntry((float)totalIncome)
                {
                    Label = dayLabels[i],
                    ValueLabel = totalIncome > 0 ? totalIncome.ToString("N0", culture) : (totalExpense > 0 ? "" : "0"), // Показываем 0, только если обе суммы 0
                    Color = _incomeChartColor,
                    ValueLabelColor = _incomeChartColor
                });

                // Добавляем запись РАСХОДА (красный, отрицательный)
                chartEntries.Add(new ChartEntry(totalExpense > 0 ? (float)totalExpense * -1 : 0) // Умножаем на -1, чтобы график шел вниз
                {
                    Label = dayLabels[i],
                    ValueLabel = totalExpense > 0 ? totalExpense.ToString("N0", culture) : "", // Не показываем 0 для расходов, если есть доход
                    Color = _expenseChartColor,
                    ValueLabelColor = _expenseChartColor
                });
            }

            if (SpendingBarChart is BarChart barChart)
            {
                barChart.Entries = chartEntries;

                // Устанавливаем MinValue/MaxValue, чтобы отрицательные значения отображались
                barChart.MinValue = (float)maxExpense * -1.2f; // Минимальное значение (отрицательное)
                barChart.MaxValue = maxChartValue; // Максимальное значение (положительное)

                OnPropertyChanged(nameof(SpendingBarChart));
            }
        }

        [RelayCommand]
        private async Task GoToAddIncomeAsync()
        {
            await GoToAddTransactionAsync(true);
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