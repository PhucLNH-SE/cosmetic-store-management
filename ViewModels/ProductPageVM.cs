using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using CosmeticStoreManagement.Data;
using CosmeticStoreManagement.Helpers;
using CosmeticStoreManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CosmeticStoreManagement.ViewModels;

public class ProductPageVM : BaseViewModel
{
    private readonly AppDbContext _context;

    // Collections
    public ObservableCollection<ProductVariant> ProductVariants { get; } = new();
    public ObservableCollection<Brand> Brands { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public ICollectionView ProductVariantsView { get; }

    // Sort Options
    public List<string> SortOptions { get; } = new()
    {
        "Mới nhất",
        "Giá: Cao → Thấp",
        "Giá: Thấp → Cao",
        "Tên: A-Z",
        "Tên: Z-A"
    };

    // Filter Properties
    private Brand? _selectedBrand;
    public Brand? SelectedBrand
    {
        get => _selectedBrand;
        set
        {
            if (_selectedBrand != value)
            {
                _selectedBrand = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }
    }

    private Category? _selectedCategory;
    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (_selectedCategory != value)
            {
                _selectedCategory = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }
    }

    private string? _selectedSort;
    public string? SelectedSort
    {
        get => _selectedSort;
        set
        {
            if (_selectedSort != value)
            {
                _selectedSort = value;
                OnPropertyChanged();
                ApplySort();
            }
        }
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }
    }

    // Selected Product for Detail
    private ProductVariant? _selectedProductVariant;
    public ProductVariant? SelectedProductVariant
    {
        get => _selectedProductVariant;
        set
        {
            if (_selectedProductVariant != value)
            {
                _selectedProductVariant = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedProduct));
                OnPropertyChanged(nameof(SelectedProductName));
                OnPropertyChanged(nameof(SelectedProductBrand));
                OnPropertyChanged(nameof(SelectedProductCategory));
                OnPropertyChanged(nameof(SelectedProductDescription));
                OnPropertyChanged(nameof(SelectedProductVariants));
                OnPropertyChanged(nameof(SelectedProductImage));
            }
        }
    }

    // Helper properties for Detail Panel
    public bool HasSelectedProduct => SelectedProductVariant != null;
    public string? SelectedProductName => SelectedProductVariant?.Product?.ProductName;
    public string? SelectedProductBrand => SelectedProductVariant?.Product?.Brand?.BrandName;
    public string? SelectedProductCategory => SelectedProductVariant?.Product?.Category?.CategoryName;
    public string? SelectedProductDescription => SelectedProductVariant?.Product?.Description;
    public string? SelectedProductImage => SelectedProductVariant?.ImagePath;

    public IEnumerable<ProductVariant>? SelectedProductVariants
    {
        get
        {
            if (SelectedProductVariant?.Product == null) return null;
            return SelectedProductVariant.Product.ProductVariants
                .Where(v => v.IsActive == true && v.StockQuantity > 0)
                .OrderBy(v => v.Volume);
        }
    }

    // Constructor
    public ProductPageVM()
    {
        _context = new AppDbContext();
        ProductVariantsView = CollectionViewSource.GetDefaultView(ProductVariants);
        ProductVariantsView.Filter = FilterPredicate;

        LoadData();
    }

    // Load data from database
    public async void LoadData()
    {
        try
        {
            // Load Brands
            var brands = await _context.Brands
                .Where(b => b.Status == true)
                .OrderBy(b => b.BrandName)
                .ToListAsync();
            Brands.Clear();
            Brands.Add(new Brand { BrandId = 0, BrandName = "Tất cả" });
            foreach (var brand in brands)
            {
                Brands.Add(brand);
            }
            SelectedBrand = Brands.FirstOrDefault();

            // Load Categories
            var categories = await _context.Categories
                .Where(c => c.Status == true)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
            Categories.Clear();
            Categories.Add(new Category { CategoryId = 0, CategoryName = "Tất cả" });
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
            SelectedCategory = Categories.FirstOrDefault();

            // Load ProductVariants (active and in stock)
            var variants = await _context.ProductVariants
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Brand)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Category)
                .Where(v => v.IsActive == true && v.StockQuantity > 0)
                .OrderBy(v => v.Product!.ProductName)
                .ThenBy(v => v.Volume)
                .ToListAsync();

            ProductVariants.Clear();
            foreach (var variant in variants)
            {
                ProductVariants.Add(variant);
            }

            // Default sort
            SelectedSort = SortOptions.FirstOrDefault();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    // Filter predicate
    private bool FilterPredicate(object obj)
    {
        if (obj is not ProductVariant variant) return false;

        // Filter by Brand
        if (SelectedBrand != null && SelectedBrand.BrandId != 0)
        {
            if (variant.Product?.BrandId != SelectedBrand.BrandId)
                return false;
        }

        // Filter by Category
        if (SelectedCategory != null && SelectedCategory.CategoryId != 0)
        {
            if (variant.Product?.CategoryId != SelectedCategory.CategoryId)
                return false;
        }

        // Filter by Search Text
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower().Trim();
            var productName = variant.Product?.ProductName?.ToLower() ?? "";
            var brandName = variant.Product?.Brand?.BrandName?.ToLower() ?? "";
            var categoryName = variant.Product?.Category?.CategoryName?.ToLower() ?? "";
            var volume = variant.Volume?.ToLower() ?? "";

            if (!productName.Contains(searchLower) &&
                !brandName.Contains(searchLower) &&
                !categoryName.Contains(searchLower) &&
                !volume.Contains(searchLower))
            {
                return false;
            }
        }

        return true;
    }

    // Apply filter
    public void ApplyFilter()
    {
        ProductVariantsView.Refresh();
    }

    // Apply sort
    public void ApplySort()
    {
        ProductVariantsView.SortDescriptions.Clear();

        if (string.IsNullOrEmpty(SelectedSort)) return;

        switch (SelectedSort)
        {
            case "Giá: Cao → Thấp":
                ProductVariantsView.SortDescriptions.Add(new SortDescription("Price", ListSortDirection.Descending));
                break;
            case "Giá: Thấp → Cao":
                ProductVariantsView.SortDescriptions.Add(new SortDescription("Price", ListSortDirection.Ascending));
                break;
            case "Tên: A-Z":
                ProductVariantsView.SortDescriptions.Add(new SortDescription("Product.ProductName", ListSortDirection.Ascending));
                break;
            case "Tên: Z-A":
                ProductVariantsView.SortDescriptions.Add(new SortDescription("Product.ProductName", ListSortDirection.Descending));
                break;
            case "Mới nhất":
            default:
                // Keep default order or add CreatedDate if available
                ProductVariantsView.SortDescriptions.Add(new SortDescription("Product.ProductName", ListSortDirection.Ascending));
                break;
        }
    }

    // Search command
    public void Search()
    {
        ApplyFilter();
    }

    // Clear filters
    public void ClearFilters()
    {
        SelectedBrand = Brands.FirstOrDefault();
        SelectedCategory = Categories.FirstOrDefault();
        SearchText = string.Empty;
        SelectedSort = SortOptions.FirstOrDefault();
    }
}
