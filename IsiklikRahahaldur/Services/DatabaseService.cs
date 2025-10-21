using IsiklikRahahaldur.Models;
using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;

namespace IsiklikRahahaldur.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;

        // Метод для инициализации базы данных
        private async Task Init()
        {
            if (_database is not null)
                return;

            // Путь к файлу базы данных
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "MyFinance.db");
            _database = new SQLiteAsyncConnection(databasePath);

            // Создаем таблицу Transaction, если ее еще нет
            await _database.CreateTableAsync<Transaction>();
        }

        public async Task<List<Transaction>> GetTransactionsAsync()
        {
            await Init();
            return await _database.Table<Transaction>().ToListAsync();
        }

        public async Task<int> SaveTransactionAsync(Transaction transaction)
        {
            await Init();
            if (transaction.Id != 0)
            {
                // Обновляем существующую запись
                return await _database.UpdateAsync(transaction);
            }
            else
            {
                // Добавляем новую запись
                return await _database.InsertAsync(transaction);
            }
        }

        public async Task<int> DeleteTransactionAsync(Transaction transaction)
        {
            await Init();
            return await _database.DeleteAsync(transaction);
        }
    }
}
