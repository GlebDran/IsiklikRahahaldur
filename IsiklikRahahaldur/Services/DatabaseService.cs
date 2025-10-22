using IsiklikRahahaldur.Models;
using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace IsiklikRahahaldur.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;

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
            await _database.CreateTableAsync<Category>();

            await SeedDefaultCategoriesAsync();
        }

        // --- Методы для Транзакций ---
        public async Task<List<Transaction>> GetTransactionsAsync()
        {
            await Init();
            return await _database.Table<Transaction>().ToListAsync();
        }

        // --- НОВЫЙ МЕТОД ---
        // Получаем одну транзакцию по ее ID
        public async Task<Transaction> GetTransactionByIdAsync(int id)
        {
            await Init();
            return await _database.Table<Transaction>().Where(t => t.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveTransactionAsync(Transaction transaction)
        {
            await Init();
            return transaction.Id != 0
                ? await _database.UpdateAsync(transaction)
                : await _database.InsertAsync(transaction);
        }

        public async Task<int> DeleteTransactionAsync(Transaction transaction)
        {
            await Init();
            return await _database.DeleteAsync(transaction);
        }

        // --- Методы для Категорий ---
        public async Task<List<Category>> GetCategoriesAsync()
        {
            await Init();
            return await _database.Table<Category>().ToListAsync();
        }

        public async Task<int> SaveCategoryAsync(Category category)
        {
            await Init();
            return category.Id != 0
                ? await _database.UpdateAsync(category)
                : await _database.InsertAsync(category);
        }

        private async Task SeedDefaultCategoriesAsync()
        {
            var categories = await GetCategoriesAsync();
            if (!categories.Any())
            {
                var defaultCategories = new List<Category>
                {
                    new Category { Name = "Продукты" },
                    new Category { Name = "Транспорт" },
                    new Category { Name = "Дом" },
                    new Category { Name = "Кафе и рестораны" },
                    new Category { Name = "Здоровье" },
                    new Category { Name = "Подарки" },
                    new Category { Name = "Зарплата" },
                    new Category { Name = "Другой доход" }
                };

                foreach (var category in defaultCategories)
                {
                    await SaveCategoryAsync(category);
                }
            }
        }
    }
}

