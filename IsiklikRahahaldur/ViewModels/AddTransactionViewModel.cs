using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using IsiklikRahahaldur.Messages;
using IsiklikRahahaldur.Models;
using IsiklikRahahaldur.Services;

namespace IsiklikRahahaldur.ViewModels;

[QueryProperty(nameof(IsIncome), "IsIncome")]
public partial class AddTransactionViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private Transaction _transaction;

    [ObservableProperty]
    private bool _isIncome;

    public AddTransactionViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        Transaction = new Transaction();
    }

    // Этот метод будет вызван, когда мы перейдем на эту страницу
    partial void OnIsIncomeChanged(bool value)
    {
        Transaction.IsIncome = value;
        Title = value ? "Добавить доход" : "Добавить расход";
    }

    [RelayCommand]
    private async Task SaveTransactionAsync()
    {
        if (Transaction.Amount <= 0 || string.IsNullOrWhiteSpace(Transaction.Description))
        {
            await Shell.Current.DisplayAlert("Ошибка", "Пожалуйста, введите описание и сумму.", "OK");
            return;
        }

        Transaction.Date = DateTime.Now;

        // Сохраняем транзакцию в базу данных
        await _databaseService.SaveTransactionAsync(Transaction);

        // Отправляем сообщение всем, кто на него подписан
        WeakReferenceMessenger.Default.Send(new TransactionAddedMessage(Transaction));

        // Возвращаемся на главный экран
        await Shell.Current.GoToAsync("..");
    }
}

