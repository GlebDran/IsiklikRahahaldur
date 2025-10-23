using IsiklikRahahaldur.Models;

namespace IsiklikRahahaldur.ViewModels
{
    // Этот класс будет использоваться для отображения в списке
    public class TransactionDisplayViewModel
    {
        public Transaction Transaction { get; set; }
        public string CategoryName { get; set; }

        public string CategoryIcon { get; set; }
    }
}
