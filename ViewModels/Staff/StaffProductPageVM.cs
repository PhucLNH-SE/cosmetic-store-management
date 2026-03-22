using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CosmeticStoreManagement.Data;
using CosmeticStoreManagement.Helpers;
using CosmeticStoreManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CosmeticStoreManagement.ViewModels.Staff;

public class StaffProductItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int TotalVariants { get; set; }
    public int TotalStock { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public string PriceRangeDisplay { get; set; } = "N/A";
    public List<ProductVariant> Variants { get; set; } = new();
}

public class StaffProductPageVM : BaseViewModel
{
    public ObservableCollection<StaffProductItem> Products { get; } = new();
    public ObservableCollection<Brand> Brands { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<ProductVariant> SelectedProductVariants { get; } = new();

    public ICollectionView ProductsView { get; }

    public ICommand RefreshCommand { get; }
    public ICommand ClearFiltersCommand { get; }

    public List<string> SortOptions { get; } = new()
    {
        "ID: Low to High",
        "ID: High to Low",
        "Price: Low to High",
        "Price: High to Low",
        "Name: A to Z",
        "Name: Z to A"
    };

    private Brand? _selectedBrand;
    public Brand? SelectedBrand
    {
        get => _selectedBrand;
        set
        {
            if (_selectedBrand == value)
            {
                return;
            }

            _selectedBrand = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

    private Category? _selectedCategory;
    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (_selectedCategory == value)
            {
                return;
            }

            _selectedCategory = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value)
            {
                return;
            }

            _searchText = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

    private StaffProductItem? _selectedProduct;
    public StaffProductItem? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (_selectedProduct == value)
            {
                return;
            }

            _selectedProduct = value;
            OnPropertyChanged();
            UpdateSelectedProductDetails();
        }
    }

    public bool HasSelectedProduct => SelectedProduct != null;
    public string SelectedProductName => SelectedProduct?.ProductName ?? "No product selected";
    public string SelectedProductBrand => SelectedProduct?.BrandName ?? "N/A";
    public string SelectedProductCategory => SelectedProduct?.CategoryName ?? "N/A";
    public string SelectedProductDescription =>
        string.IsNullOrWhiteSpace(SelectedProduct?.Description) ? "No description." : SelectedProduct!.Description;
    public string SelectedProductPriceRange => SelectedProduct?.PriceRangeDisplay ?? "N/A";
    public int SelectedProductTotalStock => SelectedProduct?.TotalStock ?? 0;
    public int SelectedProductTotalVariants => SelectedProduct?.TotalVariants ?? 0;
    public string SelectedProductImagePath => SelectedProduct?.ImagePath ?? string.Empty;

    private string? _selectedSort;
    public string? SelectedSort
    {
        get => _selectedSort;
        set
        {
            if (_selectedSort == value)
            {
                return;
            }

            _selectedSort = value;
            OnPropertyChanged();
            ApplySort();
        }
    }

    public StaffProductPageVM()
    {
        ProductsView = CollectionViewSource.GetDefaultView(Products);
        ProductsView.Filter = FilterPredicate;

        RefreshCommand = new RelayCommand(_ =>
        {
            ClearFilters();
            LoadData();
        });
        ClearFiltersCommand = new RelayCommand(_ => ClearFilters());

        LoadData();
    }

    public async void LoadData()
    {
        var currentBrandId = SelectedBrand?.BrandId ?? 0;
        var currentCategoryId = SelectedCategory?.CategoryId ?? 0;
        var currentSearch = SearchText;
        var currentSelectedProductId = SelectedProduct?.ProductId ?? 0;
        var currentSort = SelectedSort;

        try
        {
            using var context = new AppDbContext();

            var brands = await context.Brands
                .AsNoTracking()
                .Where(brand => brand.Status == true)
                .OrderBy(brand => brand.BrandName)
                .ToListAsync();

            var categories = await context.Categories
                .AsNoTracking()
                .Where(category => category.Status == true)
                .OrderBy(category => category.CategoryName)
                .ToListAsync();

            var products = await context.Products
                .AsNoTracking()
                .Include(product => product.Brand)
                .Include(product => product.Category)
                .Include(product => product.ProductVariants)
                .Where(product =>
                    product.IsActive == true &&
                    product.Brand.Status == true &&
                    product.Category.Status == true)
                .OrderBy(product => product.ProductId)
                .ToListAsync();

            Brands.Clear();
            Brands.Add(new Brand { BrandId = 0, BrandName = "All brands" });
            foreach (var brand in brands)
            {
                Brands.Add(brand);
            }

            Categories.Clear();
            Categories.Add(new Category { CategoryId = 0, CategoryName = "All categories" });
            foreach (var category in categories)
            {
                Categories.Add(category);
            }

            Products.Clear();
            foreach (var product in products)
            {
                var activeVariants = product.ProductVariants
                    .Where(variant => variant.IsActive == true)
                    .OrderBy(variant => variant.Volume)
                    .ToList();

                var totalStock = activeVariants.Sum(variant => variant.StockQuantity ?? 0);
                var priceValues = activeVariants
                    .Select(variant => variant.Price ?? 0m)
                    .ToList();

                var minPrice = priceValues.Count > 0 ? priceValues.Min() : 0m;
                var maxPrice = priceValues.Count > 0 ? priceValues.Max() : 0m;
                var priceRangeDisplay = priceValues.Count == 0
                    ? "N/A"
                    : minPrice == maxPrice
                        ? $"{minPrice:N0} VND"
                        : $"{minPrice:N0} - {maxPrice:N0} VND";

                Products.Add(new StaffProductItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    BrandId = product.BrandId,
                    BrandName = product.Brand.BrandName ?? string.Empty,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category.CategoryName ?? string.Empty,
                    Description = product.Description ?? string.Empty,
                    ImagePath = NormalizeImagePath(product.ImagePath, product.ProductId),
                    IsActive = product.IsActive == true,
                    TotalVariants = activeVariants.Count,
                    TotalStock = totalStock,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    PriceRangeDisplay = priceRangeDisplay,
                    Variants = activeVariants
                });
            }

            SelectedBrand = Brands.FirstOrDefault(brand => brand.BrandId == currentBrandId) ?? Brands.FirstOrDefault();
            SelectedCategory = Categories.FirstOrDefault(category => category.CategoryId == currentCategoryId) ?? Categories.FirstOrDefault();
            SearchText = currentSearch;
            SelectedSort = SortOptions.Contains(currentSort ?? string.Empty) ? currentSort : SortOptions.FirstOrDefault();

            ApplySort();
            ApplyFilter();

            SelectedProduct = Products.FirstOrDefault(product => product.ProductId == currentSelectedProductId);
            if (SelectedProduct == null && Products.Count > 0)
            {
                SelectedProduct = Products.First();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Unable to load products.\n{ex.Message}",
                "Load failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    public void ClearFilters()
    {
        SelectedBrand = Brands.FirstOrDefault();
        SelectedCategory = Categories.FirstOrDefault();
        SearchText = string.Empty;
        SelectedSort = SortOptions.FirstOrDefault();
    }

    private void ApplyFilter()
    {
        ProductsView.Refresh();
    }

    private void ApplySort()
    {
        ProductsView.SortDescriptions.Clear();

        switch (SelectedSort)
        {
            case "ID: High to Low":
                ProductsView.SortDescriptions.Add(new SortDescription(nameof(StaffProductItem.ProductId), ListSortDirection.Descending));
                break;
            case "Price: Low to High":
                ProductsView.SortDescriptions.Add(new SortDescription(nameof(StaffProductItem.MinPrice), ListSortDirection.Ascending));
                ProductsView.SortDescriptions.Add(new SortDescription(nameof(StaffProductItem.ProductId), ListSortDirection.Ascending));
                break;
            case "Price: High to Low":
                ProductsView.SortDescriptions.Add(new SortDescription(nameof(StaffProductItem.MaxPrice), ListSortDirection.Descending));
                ProductsView.SortDescriptions.Add(new SortDescription(nameof(StaffProductItem.ProductId), ListSortDirection.Descending));
                break;
            case "Name: A to Z":
                ProductsView.SortDescriptions.Add(new SortDescription(nameof(StaffProductItem.ProductName), ListSortDirection.Ascending));
                ProductsView.SortDescriptions.Add(new SortDescription(nameof(StaffProductItem.ProductId), ListSortDirection.Ascending));
                break;
            case "Name: Z to A":
                ProductsView.SortDescriptions.Add(new SortDescription(nameof(StaffProductItem.ProductName), ListSortDirection.Descending));
                ProductsView.SortDescriptions.Add(new SortDescription(nameof(StaffProductItem.ProductId), ListSortDirection.Descending));
                break;
            case "ID: Low to High":
            default:
                ProductsView.SortDescriptions.Add(new SortDescription(nameof(StaffProductItem.ProductId), ListSortDirection.Ascending));
                break;
        }
    }

    private bool FilterPredicate(object obj)
    {
        if (obj is not StaffProductItem item)
        {
            return false;
        }

        if (SelectedBrand != null && SelectedBrand.BrandId != 0 && item.BrandId != SelectedBrand.BrandId)
        {
            return false;
        }

        if (SelectedCategory != null && SelectedCategory.CategoryId != 0 && item.CategoryId != SelectedCategory.CategoryId)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        var keyword = SearchText.Trim().ToLowerInvariant();
        return item.ProductName.ToLowerInvariant().Contains(keyword)
            || item.BrandName.ToLowerInvariant().Contains(keyword)
            || item.CategoryName.ToLowerInvariant().Contains(keyword);
    }

    private void UpdateSelectedProductDetails()
    {
        SelectedProductVariants.Clear();
        if (SelectedProduct?.Variants != null)
        {
            foreach (var variant in SelectedProduct.Variants)
            {
                SelectedProductVariants.Add(variant);
            }
        }

        OnPropertyChanged(nameof(HasSelectedProduct));
        OnPropertyChanged(nameof(SelectedProductName));
        OnPropertyChanged(nameof(SelectedProductBrand));
        OnPropertyChanged(nameof(SelectedProductCategory));
        OnPropertyChanged(nameof(SelectedProductDescription));
        OnPropertyChanged(nameof(SelectedProductPriceRange));
        OnPropertyChanged(nameof(SelectedProductTotalStock));
        OnPropertyChanged(nameof(SelectedProductTotalVariants));
        OnPropertyChanged(nameof(SelectedProductImagePath));
    }

    private static string NormalizeImagePath(string? rawPath, int productId)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        if (!string.IsNullOrWhiteSpace(rawPath))
        {
            var normalized = Path.IsPathRooted(rawPath) ? Path.GetFileName(rawPath) : rawPath;
            var candidatePath = Path.Combine(baseDirectory, "Images", "Products", normalized);
            if (File.Exists(candidatePath))
            {
                return normalized;
            }
        }

        var fallback = $"product_{productId}.png";
        var fallbackPath = Path.Combine(baseDirectory, "Images", "Products", fallback);
        return File.Exists(fallbackPath) ? fallback : string.Empty;
    }
}
