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

        private readonly SKColor _barChartColor = SKColor.Parse("#2ECC71");

        public MainViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Мой кошелек";
            Transactions = new ObservableCollection<TransactionDisplayViewModel>();

            // --- НАЧАЛО ИСПРАВЛЕНИЯ ---
            // Создаем начальный список с нулями, чтобы график не был пустым
            var initialEntries = new List<ChartEntry>();
            string[] dayLabels = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };

            for (int i = 0; i < dayLabels.Length; i++)
            {
                initialEntries.Add(new ChartEntry(0)
                {
                    Label = dayLabels[i],
                    ValueLabel = "0",
                    Color = _barChartColor
                });
            }

            SpendingBarChart = new BarChart
            {
                Entries = initialEntries, // <-- ТЕПЕРЬ СПИСОК НЕ ПУСТОЙ
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

                // Добавляем "Без категории" в словарь для корректного отображения, если CategoryId = 0
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
                    // --- ИЗМЕНЕНО: Определяем имя категории и иконку ---
                    string categoryName = categoriesDict.TryGetValue(t.CategoryId, out var name) ? name : "Без категории";
                    string categoryIcon = GetIconForCategory(categoryName); // Вызываем новый метод

                    Transactions.Add(new TransactionDisplayViewModel
                    {
                        Transaction = t,
                        CategoryName = categoryName,
                        CategoryIcon = categoryIcon // Присваиваем имя файла иконки
                    });
                    // --- КОНЕЦ ИЗМЕНЕНИЯ ---
                }

                CalculateBalanceAndTotals(transactionsFromDb);
                CalculatePeriodTotals(filteredTransactions);
                UpdateBarChart(filteredTransactions, startDate);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // --- НОВЫЙ МЕТОД: Возвращает имя файла иконки по имени категории ---
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
                // Добавьте другие предопределенные категории здесь
                default: return "other_icon.png"; // Иконка по умолчанию
            }
        }
        // --- КОНЕЦ НОВОГО МЕТОДА ---


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
            string[] dayLabels = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
            DayOfWeek[] daysOrder = { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };

            for (int i = 0; i < daysOrder.Length; i++)
            {
                DayOfWeek currentDay = daysOrder[i];
                decimal totalAmount = expensesByDayOfWeek.TryGetValue(currentDay, out var amount) ? amount : 0;

                chartEntries.Add(new ChartEntry((float)totalAmount)
                {
                    Label = dayLabels[i],
                    ValueLabel = totalAmount > 0 ? totalAmount.ToString("N0") : "0",
                    Color = _barChartColor
                });
            }

            if (SpendingBarChart is BarChart barChart)
            {
                barChart.Entries = chartEntries;
                OnPropertyChanged(nameof(SpendingBarChart));
            }
        }

        [RelayCommand]
        private async Task GoToAddIncomeAsync()
        {
            // Просто вызываем существующий метод с нужным параметром
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