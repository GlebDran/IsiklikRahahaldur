using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IsiklikRahahaldur.Models;
using IsiklikRahahaldur.Services;

namespace IsiklikRahahaldur.ViewModels
{
    public partial class CategoriesViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private ObservableCollection<Category> _categories;

        [ObservableProperty]
        private string _newCategoryName;

        public CategoriesViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Categories = new ObservableCollection<Category>();
            Title = "Управление категориями";
        }

        [RelayCommand]
        private async Task LoadCategoriesAsync()
        {
            var cats = await _databaseService.GetCategoriesAsync();
            Categories.Clear();
            foreach (var cat in cats)
            {
                // Не показываем "Без категории" (Id=0), если она есть
                if (cat.Id != 0)
                    Categories.Add(cat);
            }
        }

        [RelayCommand]
        private async Task AddCategoryAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName))
            {
                await Shell.Current.DisplayAlert("Ошибка", "Введите название категории", "OK");
                return;
            }

            var newCategory = new Category { Name = NewCategoryName };
            await _databaseService.SaveCategoryAsync(newCategory);

            NewCategoryName = string.Empty; // Очищаем поле ввода
            await LoadCategoriesAsync(); // Обновляем список
        }

        [RelayCommand]
        private async Task DeleteCategoryAsync(Category category)
        {
            if (category == null) return;

            bool answer = await Shell.Current.DisplayAlert("Подтверждение",
                $"Удалить категорию '{category.Name}'? \n\n(Транзакции, связанные с ней, получат статус 'Без категории')",
                "Удалить", "Отмена");

            if (answer)
            {
                // 1. Обновляем транзакции
                await _databaseService.UpdateTransactionsCategoryToDefaultAsync(category.Id);
                // 2. Удаляем категорию
                await _databaseService.DeleteCategoryAsync(category);
                // 3. Обновляем список на экране
                await LoadCategoriesAsync();
            }
        }

        [RelayCommand]
        private async Task EditCategoryAsync(Category category)
        {
            if (category == null) return;

            string newName = await Shell.Current.DisplayPromptAsync("Редактировать",
                $"Введите новое имя для '{category.Name}'", "Сохранить", "Отмена",
                initialValue: category.Name);

            if (!string.IsNullOrWhiteSpace(newName) && newName != category.Name)
            {
                category.Name = newName;
                await _databaseService.SaveCategoryAsync(category);
                await LoadCategoriesAsync();
            }
        }
    }
}