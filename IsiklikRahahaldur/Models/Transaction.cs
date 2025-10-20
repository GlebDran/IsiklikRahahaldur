using System;

namespace IsiklikRahahaldur.Models
{
    /// <summary>
    /// Представляет одну финансовую транзакцию (доход или расход).
    /// </summary>
    public class Transaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public bool IsIncome { get; set; } // true = доход, false = расход
        public string Description { get; set; }
        public DateTime Date { get; set; }
    }
}
