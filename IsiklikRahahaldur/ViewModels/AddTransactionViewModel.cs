using System.Windows.Input;
using IsiklikRahahaldur.Models;

namespace IsiklikRahahaldur.ViewModels
{
    [QueryProperty(nameof(IsIncome), "isIncome")]
    public class AddTransactionViewModel : BaseViewModel
    {
        private string _description;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        private bool _isIncome;
        public bool IsIncome
        {
            get => _isIncome;
            set
            {
                SetProperty(ref _isIncome, value);
                Title = value ? "Новый доход" : "Новый расход";
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddTransactionViewModel()
        {
            SaveCommand = new Command(async () => await SaveTransaction(), CanSave);
            CancelCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            // Чтобы кнопка "Сохранить" обновляла свое состояние
            this.PropertyChanged += (_, __) => (SaveCommand as Command).ChangeCanExecute();
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Description) && Amount > 0;
        }

        private async Task SaveTransaction()
        {
            var newTransaction = new Transaction
            {
                Description = this.Description,
                Amount = this.Amount,
                IsIncome = this.IsIncome,
                Date = DateTime.Now
            };

            // Создаем словарь для передачи данных обратно на главную страницу
            var navigationParameter = new Dictionary<string, object>
            {
                { "NewTransaction", newTransaction }
            };

            // Возвращаемся назад и передаем данные
            await Shell.Current.GoToAsync("..", navigationParameter);
        }
    }
}

