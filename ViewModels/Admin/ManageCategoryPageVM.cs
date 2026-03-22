using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using CosmeticStoreManagement.Data;
using CosmeticStoreManagement.Helpers;
using CosmeticStoreManagement.Models;

namespace CosmeticStoreManagement.ViewModels.Admin;

public class ManageCategoryPageVM : BaseViewModel
{
    private ICollectionView _categoryView;
    private Category _textboxitem;
    private Category? _selecteditem;
    private string _searchtext = string.Empty;
    private string _statusmessage = string.Empty;

    public ManageCategoryPageVM()
    {
        categories = new ObservableCollection<Category>();
        _categoryView = CollectionViewSource.GetDefaultView(categories);
        _textboxitem = new Category { Status = true };

        AddCommand = new RelayCommand(Add);
        UpdateCommand = new RelayCommand(Update);
        DeleteCommand = new RelayCommand(Delete);
        SearchCommand = new RelayCommand(Search);
        ClearCommand = new RelayCommand(ClearForm);

        Load();
    }

    public ObservableCollection<Category> categories { get; set; }

    public ICollectionView CategoryView
    {
        get => _categoryView;
        set
        {
            _categoryView = value;
            OnPropertyChanged(nameof(CategoryView));
        }
    }

    public Category textboxitem
    {
        get => _textboxitem;
        set
        {
            _textboxitem = value;
            OnPropertyChanged(nameof(textboxitem));
        }
    }

    public Category? selecteditem
    {
        get => _selecteditem;
        set
        {
            _selecteditem = value;
            OnPropertyChanged(nameof(selecteditem));

            if (_selecteditem != null)
            {
                textboxitem = (Category)_selecteditem.Clone();
            }
        }
    }

    public string searchtext
    {
        get => _searchtext;
        set
        {
            _searchtext = value;
            OnPropertyChanged(nameof(searchtext));
        }
    }

    public string statusmessage
    {
        get => _statusmessage;
        set
        {
            _statusmessage = value;
            OnPropertyChanged(nameof(statusmessage));
        }
    }

    public ICommand AddCommand { get; }
    public ICommand UpdateCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand ClearCommand { get; }

    private void Load()
    {
        using var context = new AppDbContext();
        var list = context.Categories.OrderBy(x => x.CategoryId).ToList();

        categories = new ObservableCollection<Category>(list);
        OnPropertyChanged(nameof(categories));

        CategoryView = CollectionViewSource.GetDefaultView(categories);
        ApplyFilter();
    }

    private void Add(object? parameter)
    {
        if (string.IsNullOrWhiteSpace(textboxitem.CategoryName))
        {
            statusmessage = "Category name is required.";
            return;
        }

        var categoryName = textboxitem.CategoryName.Trim();
        var status = textboxitem.Status ?? true;

        using var context = new AppDbContext();
        var duplicated = context.Categories.Any(x => x.CategoryName.ToLower() == categoryName.ToLower());
        if (duplicated)
        {
            statusmessage = "Category name already exists.";
            return;
        }

        var newCategory = new Category
        {
            CategoryName = categoryName,
            Status = status
        };

        context.Categories.Add(newCategory);
        context.SaveChanges();

        categories.Add(newCategory);
        ApplyFilter();

        textboxitem = new Category { Status = true };
        statusmessage = "Create category successfully.";
    }

    private void Update(object? parameter)
    {
        if (selecteditem == null)
        {
            statusmessage = "Select one category to update.";
            return;
        }

        if (string.IsNullOrWhiteSpace(textboxitem.CategoryName))
        {
            statusmessage = "Category name is required.";
            return;
        }

        using var context = new AppDbContext();
        var categoryInDb = context.Categories.FirstOrDefault(x => x.CategoryId == selecteditem.CategoryId);
        if (categoryInDb == null)
        {
            statusmessage = "Category not found.";
            return;
        }

        var categoryName = textboxitem.CategoryName.Trim();
        var duplicated = context.Categories.Any(x => x.CategoryId != categoryInDb.CategoryId && x.CategoryName.ToLower() == categoryName.ToLower());
        if (duplicated)
        {
            statusmessage = "Category name already exists.";
            return;
        }

        categoryInDb.CategoryName = categoryName;
        categoryInDb.Status = textboxitem.Status ?? true;

        context.SaveChanges();

        var oldItem = categories.FirstOrDefault(x => x.CategoryId == categoryInDb.CategoryId);
        if (oldItem != null)
        {
            var index = categories.IndexOf(oldItem);
            categories[index] = (Category)categoryInDb.Clone();
        }

        ApplyFilter();
        statusmessage = "Update category successfully.";
    }

    private void Delete(object? parameter)
    {
        if (selecteditem == null)
        {
            statusmessage = "Select one category to delete.";
            return;
        }

        using var context = new AppDbContext();
        var categoryInDb = context.Categories.FirstOrDefault(x => x.CategoryId == selecteditem.CategoryId);
        if (categoryInDb == null)
        {
            statusmessage = "Category not found.";
            return;
        }

        categoryInDb.Status = false;
        context.SaveChanges();

        var localCategory = categories.FirstOrDefault(x => x.CategoryId == categoryInDb.CategoryId);
        if (localCategory != null)
        {
            localCategory.Status = false;
        }

        selecteditem = null;
        textboxitem = new Category { Status = true };

        ApplyFilter();
        statusmessage = "Delete category successfully.";
    }

    private void Search(object? parameter)
    {
        ApplyFilter();
    }

    private void ClearForm(object? parameter)
    {
        selecteditem = null;
        textboxitem = new Category { Status = true };
        statusmessage = string.Empty;
    }

    private void ApplyFilter()
    {
        var keyword = searchtext?.Trim() ?? string.Empty;

        CategoryView.Filter = item =>
        {
            if (item is not Category category)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(keyword))
            {
                return true;
            }

            return category.CategoryId.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase)
                   || (!string.IsNullOrWhiteSpace(category.CategoryName)
                       && category.CategoryName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        };

        CategoryView.Refresh();
    }
}

