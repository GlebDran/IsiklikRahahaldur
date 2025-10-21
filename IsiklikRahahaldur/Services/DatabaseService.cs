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

        // Флаги для корректной работы с кодировкой и потоками
        private const SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;

        private async Task Init()
        {
            if (_database is not null)
                return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "MyFinance.db");
            _database = new SQLiteAsyncConnection(databasePath, Flags);

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
                return await _database.UpdateAsync(transaction);
            }
            else
            {
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

