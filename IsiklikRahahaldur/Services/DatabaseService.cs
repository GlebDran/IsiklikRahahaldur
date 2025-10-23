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
        private bool _isInitialized = false; // Flag to prevent re-initialization/seeding

        private const SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;

        // --- ИЗМЕНЕНО: Более надежная инициализация ---
        private async Task Init()
        {
            if (_isInitialized) // Используем флаг
                return;

            try
            {
                var databasePath = Path.Combine(FileSystem.AppDataDirectory, "MyFinance.db");
                _database = new SQLiteAsyncConnection(databasePath, Flags);

                // Создаем таблицы *сначала*
                await _database.CreateTableAsync<Transaction>();
                await _database.CreateTableAsync<Category>();

                // Заполняем *после* создания таблиц и только один раз
                await SeedDefaultCategoriesAsync();

                _isInitialized = true; // Устанавливаем флаг *после* успешной инициализации
            }
            catch (Exception ex)
            {
                // TODO: Добавить логирование ошибки инициализации
                Console.WriteLine($"Database initialization failed: {ex.Message}");
                // Можно выбросить исключение дальше или обработать его
                throw;
            }
        }

        // --- Методы для Транзакций ---
        public async Task<List<Transaction>> GetTransactionsAsync()
        {
            await Init(); // Гарантирует, что база данных инициализирована
            return await _database.Table<Transaction>().ToListAsync();
        }

        public async Task<Transaction> GetTransactionByIdAsync(int id)
        {
            await Init(); // Гарантирует инициализацию
            return await _database.Table<Transaction>().Where(t => t.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveTransactionAsync(Transaction transaction)
        {
            await Init(); // Гарантирует инициализацию
            return transaction.Id != 0
                ? await _database.UpdateAsync(transaction)
                : await _database.InsertAsync(transaction);
        }

        public async Task<int> DeleteTransactionAsync(Transaction transaction)
        {
            await Init(); // Гарантирует инициализацию
            return await _database.DeleteAsync(transaction);
        }

        // --- Методы для Категорий ---
        public async Task<List<Category>> GetCategoriesAsync()
        {
            await Init(); // Гарантирует инициализацию
            // Эта строка теперь безопасна, Init выполняется полностью до нее
            return await _database.Table<Category>().ToListAsync();
        }

        public async Task<int> SaveCategoryAsync(Category category)
        {
            await Init(); // Гарантирует инициализацию
            // Не позволяем сохранять категорию с ID 0 (зарезервировано для "Без категории")
            if (category.Id == 0 && !string.IsNullOrEmpty(category.Name))
            {
                // Пытаемся вставить как новую, ID будет сгенерирован (>0)
                category.Id = 0; // Сбрасываем Id на всякий случай, чтобы SQLite сгенерировал новый
                return await _database.InsertAsync(category);
            }
            else if (category.Id != 0)
            {
                return await _database.UpdateAsync(category);
            }
            else
            {
                // Либо Id == 0 и Name пустой, либо какая-то другая ошибка - ничего не делаем
                return 0; // Или выбросить исключение?
            }
        }

        public async Task<int> DeleteCategoryAsync(Category category)
        {
            await Init(); // Гарантирует инициализацию
                          // Запрещаем удаление категории с ID 0
            if (category == null || category.Id == 0) return 0;
            return await _database.DeleteAsync(category);
        }

        public async Task UpdateTransactionsCategoryToDefaultAsync(int categoryId)
        {
            await Init(); // Гарантирует инициализацию
                          // ID 0 не должен передаваться сюда, но на всякий случай
            if (categoryId == 0) return;

            var transactionsToUpdate = await _database.Table<Transaction>()
                                                      .Where(t => t.CategoryId == categoryId)
                                                      .ToListAsync();

            if (transactionsToUpdate.Any())
            {
                foreach (var t in transactionsToUpdate)
                {
                    t.CategoryId = 0; // Устанавливаем ID "Без категории"
                }
                // Обновляем все транзакции одним вызовом
                await _database.UpdateAllAsync(transactionsToUpdate);
            }
        }

        // --- ИЗМЕНЕНО: Логика заполнения по умолчанию ---
        private async Task SeedDefaultCategoriesAsync()
        {
            // Проверяем количество категорий прямо в таблице
            var categoryCount = await _database.Table<Category>().CountAsync();
            if (categoryCount == 0) // Заполняем только если таблица пуста
            {
                var defaultCategories = new List<Category>
                {
                    // ID будут сгенерированы автоматически SQLite (начиная с 1)
                    new Category { Name = "Продукты" },
                    new Category { Name = "Транспорт" },
                    new Category { Name = "Дом" },
                    new Category { Name = "Кафе и рестораны" },
                    new Category { Name = "Здоровье" },
                    new Category { Name = "Подарки" },
                    new Category { Name = "Зарплата" },
                    new Category { Name = "Другой доход" }
                    // "Без категории" (ID 0) НЕ добавляем в базу данных.
                    // Она будет обрабатываться логикой ViewModel.
                };

                // Используем InsertAllAsync для эффективности
                await _database.InsertAllAsync(defaultCategories);
            }
        }
    }
}