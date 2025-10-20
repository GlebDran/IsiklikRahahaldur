using IsiklikRahahaldur.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace IsiklikRahahaldur.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private decimal _balance;
        public decimal Balance
        {
            get => _balance;
            set
            {
                _balance = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Transaction> Transactions { get; }

        public ICommand AddIncomeCommand { get; }
        public ICommand AddExpenseCommand { get; }

        public MainViewModel()
        {
            Transactions = new ObservableCollection<Transaction>();
            LoadSampleData();
            CalculateBalance();

            // Пока команды ничего не делают, но они готовы к использованию.
            AddIncomeCommand = new Command(OnAddIncome);
            AddExpenseCommand = new Command(OnAddExpense);
        }

        private void LoadSampleData()
        {
            // Пример данных, чтобы макет не был пустым.
            Transactions.Add(new Transaction { Amount = 1200.00m, IsIncome = true, Description = "Зарплата", Date = DateTime.Now.AddDays(-5) });
            Transactions.Add(new Transaction { Amount = 45.50m, IsIncome = false, Description = "Продукты в магазине", Date = DateTime.Now.AddDays(-3) });
            Transactions.Add(new Transaction { Amount = 250.00m, IsIncome = false, Description = "Коммунальные платежи", Date = DateTime.Now.AddDays(-2) });
            Transactions.Add(new Transaction { Amount = 300.00m, IsIncome = true, Description = "Фриланс проект", Date = DateTime.Now.AddDays(-1) });
        }

        private void CalculateBalance()
        {
            decimal totalIncome = Transactions.Where(t => t.IsIncome).Sum(t => t.Amount);
            decimal totalExpense = Transactions.Where(t => !t.IsIncome).Sum(t => t.Amount);
            Balance = totalIncome - totalExpense;
        }

        private void OnAddIncome()
        {
            // Логика для добавления дохода будет здесь
            // Например, открытие новой страницы
        }

        private void OnAddExpense()
        {
            // Логика для добавления расхода будет здесь
        }
    }
}
