using CommunityToolkit.Mvvm.Messaging.Messages;
using IsiklikRahahaldur.Models;

// Это простой класс-сообщение, который будет "переносить"
// новую транзакцию между ViewModel'ами.
namespace IsiklikRahahaldur.Messages;

public class TransactionAddedMessage : ValueChangedMessage<Transaction>
{
    public TransactionAddedMessage(Transaction value) : base(value)
    {
    }
}
