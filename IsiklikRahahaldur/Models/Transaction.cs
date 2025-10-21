using SQLite;
using System;

namespace IsiklikRahahaldur.Models
{
    public class Transaction
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Description { get; set; }
        public decimal Amount { get; set; }
        public bool IsIncome { get; set; } // true - доход, false - расход
        public DateTime Date { get; set; }

        // Новое свойство для связи с категорией
        public int CategoryId { get; set; }
    }
}

