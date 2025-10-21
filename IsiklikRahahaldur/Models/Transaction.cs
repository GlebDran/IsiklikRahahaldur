using SQLite;
using System;

namespace IsiklikRahahaldur.Models
{
    public class Transaction
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; } // Уникальный идентификатор для базы данных
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public bool IsIncome { get; set; } // true - доход, false - расход
        public DateTime Date { get; set; }
    }
}

