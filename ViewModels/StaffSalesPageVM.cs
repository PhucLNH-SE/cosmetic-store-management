using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CosmeticStoreManagement.Data;
using CosmeticStoreManagement.Helpers;
using CosmeticStoreManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CosmeticStoreManagement.ViewModels;

public class StaffSalesPageVM : BaseViewModel
{
    private const string LegacyWalkInCustomerPhone = "STAFF-WALKIN";

    public ObservableCollection<ProductVariant> ProductVariants { get; } = new();
    public ObservableCollection<Brand> Brands { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<Customer> Customers { get; } = new();
    public ObservableCollection<StaffCartLineVM> CartItems { get; } = new();

    public ICollectionView ProductVariantsView { get; }
    public ICollectionView CustomerOptionsView { get; }

    public List<string> SortOptions { get; } = new()
    {
        "Newest",
        "Price: High to Low",
        "Price: Low to High",
        "Name: A to Z",
        "Name: Z to A"
    };

    public ICommand AddToCartCommand { get; }
    public ICommand IncreaseQuantityCommand { get; }
    public ICommand DecreaseQuantityCommand { get; }
    public ICommand RemoveFromCartCommand { get; }
    public ICommand CheckoutCommand { get; }

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

    private string _customerSearchText = string.Empty;
    public string CustomerSearchText
    {
        get => _customerSearchText;
        set
        {
            if (_customerSearchText == value)
            {
                return;
            }

            _customerSearchText = value;
            OnPropertyChanged();

            if (!_isSyncingCustomerSearch && _selectedCustomer != null)
            {
                SetSelectedCustomer(null, syncSearchText: false, closeSuggestions: false);
            }

            RefreshCustomerFilter();
            ShowCustomerSuggestions = !string.IsNullOrWhiteSpace(_customerSearchText);
        }
    }

    private Customer? _selectedCustomer;
    public Customer? SelectedCustomer
    {
        get => _selectedCustomer;
        set => SetSelectedCustomer(value, syncSearchText: true, closeSuggestions: value != null);
    }

    private bool _showCustomerSuggestions;
    public bool ShowCustomerSuggestions
    {
        get => _showCustomerSuggestions;
        set
        {
            if (_showCustomerSuggestions == value)
            {
                return;
            }

            _showCustomerSuggestions = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowCustomerNoResults));
        }
    }

    public bool HasCustomerResults => CustomerOptionsView.Cast<object>().Any();

    public bool ShowCustomerNoResults =>
        ShowCustomerSuggestions &&
        !HasCustomerResults &&
        !string.IsNullOrWhiteSpace(CustomerSearchText);

    public int TotalQuantity => CartItems.Sum(item => item.Quantity);

    public decimal CartTotal => CartItems.Sum(item => item.LineTotal);

    public bool HasCartItems => CartItems.Count > 0;

    public bool HasSelectedCustomer => SelectedCustomer != null;

    public bool CanCheckout => HasCartItems && HasSelectedCustomer;

    public string SelectedCustomerName =>
        string.IsNullOrWhiteSpace(SelectedCustomer?.CustomerName)
            ? "No customer selected"
            : SelectedCustomer.CustomerName!;

    public string SelectedCustomerPhone =>
        string.IsNullOrWhiteSpace(SelectedCustomer?.Phone)
            ? "No phone number"
            : SelectedCustomer.Phone!;

    public string SelectedCustomerAddress =>
        string.IsNullOrWhiteSpace(SelectedCustomer?.Address)
            ? "No address"
            : SelectedCustomer.Address!;

    private bool _isSyncingCustomerSearch;

    public StaffSalesPageVM()
    {
        ProductVariantsView = CollectionViewSource.GetDefaultView(ProductVariants);
        ProductVariantsView.Filter = FilterPredicate;

        CustomerOptionsView = CollectionViewSource.GetDefaultView(Customers);
        CustomerOptionsView.Filter = CustomerFilterPredicate;

        AddToCartCommand = new RelayCommand(AddToCart);
        IncreaseQuantityCommand = new RelayCommand(IncreaseQuantity);
        DecreaseQuantityCommand = new RelayCommand(DecreaseQuantity);
        RemoveFromCartCommand = new RelayCommand(RemoveFromCart);
        CheckoutCommand = new RelayCommand(_ => Checkout(), _ => CanCheckout);

        CartItems.CollectionChanged += CartItems_CollectionChanged;

        LoadData();
    }

    public async void LoadData()
    {
        var currentBrandId = SelectedBrand?.BrandId ?? 0;
        var currentCategoryId = SelectedCategory?.CategoryId ?? 0;
        var currentSort = SelectedSort;
        var currentSearch = SearchText;
        var currentCustomerSearch = CustomerSearchText;
        var currentCustomerId = SelectedCustomer?.CustomerId;

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

            var customers = await context.Customers
                .AsNoTracking()
                .Where(customer => customer.Phone != LegacyWalkInCustomerPhone)
                .OrderBy(customer => customer.CustomerName)
                .ThenBy(customer => customer.Phone)
                .ToListAsync();

            var variants = await context.ProductVariants
                .AsNoTracking()
                .Include(variant => variant.Product)
                    .ThenInclude(product => product!.Brand)
                .Include(variant => variant.Product)
                    .ThenInclude(product => product!.Category)
                .Where(variant =>
                    variant.IsActive == true &&
                    (variant.StockQuantity ?? 0) > 0 &&
                    variant.Product != null &&
                    variant.Product.IsActive == true &&
                    variant.Product.Brand.Status == true &&
                    variant.Product.Category.Status == true)
                .OrderByDescending(variant => variant.VariantId)
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

            Customers.Clear();
            foreach (var customer in customers)
            {
                Customers.Add(customer);
            }

            ProductVariants.Clear();
            foreach (var variant in variants)
            {
                ProductVariants.Add(variant);
            }

            SelectedBrand = Brands.FirstOrDefault(brand => brand.BrandId == currentBrandId) ?? Brands.FirstOrDefault();
            SelectedCategory = Categories.FirstOrDefault(category => category.CategoryId == currentCategoryId) ?? Categories.FirstOrDefault();
            SelectedSort = SortOptions.Contains(currentSort ?? string.Empty) ? currentSort : SortOptions.FirstOrDefault();
            SearchText = currentSearch;
            CustomerSearchText = currentCustomerSearch;
            SelectedCustomer = Customers.FirstOrDefault(customer => customer.CustomerId == currentCustomerId);

            RefreshCartAvailability(variants);
            RefreshCustomerFilter();
            ApplySort();
            ApplyFilter();
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

    public void ApplyFilter()
    {
        ProductVariantsView.Refresh();
    }

    public void ApplySort()
    {
        ProductVariantsView.SortDescriptions.Clear();

        switch (SelectedSort)
        {
            case "Price: High to Low":
                ProductVariantsView.SortDescriptions.Add(new SortDescription(nameof(ProductVariant.Price), ListSortDirection.Descending));
                ProductVariantsView.SortDescriptions.Add(new SortDescription(nameof(ProductVariant.VariantId), ListSortDirection.Descending));
                break;
            case "Price: Low to High":
                ProductVariantsView.SortDescriptions.Add(new SortDescription(nameof(ProductVariant.Price), ListSortDirection.Ascending));
                ProductVariantsView.SortDescriptions.Add(new SortDescription(nameof(ProductVariant.VariantId), ListSortDirection.Descending));
                break;
            case "Name: A to Z":
                ProductVariantsView.SortDescriptions.Add(new SortDescription("Product.ProductName", ListSortDirection.Ascending));
                ProductVariantsView.SortDescriptions.Add(new SortDescription(nameof(ProductVariant.Volume), ListSortDirection.Ascending));
                break;
            case "Name: Z to A":
                ProductVariantsView.SortDescriptions.Add(new SortDescription("Product.ProductName", ListSortDirection.Descending));
                ProductVariantsView.SortDescriptions.Add(new SortDescription(nameof(ProductVariant.Volume), ListSortDirection.Descending));
                break;
            case "Newest":
            default:
                ProductVariantsView.SortDescriptions.Add(new SortDescription(nameof(ProductVariant.VariantId), ListSortDirection.Descending));
                break;
        }
    }

    private bool FilterPredicate(object obj)
    {
        if (obj is not ProductVariant variant)
        {
            return false;
        }

        if (variant.IsActive != true || (variant.StockQuantity ?? 0) <= 0 || variant.Product?.IsActive != true)
        {
            return false;
        }

        if (SelectedBrand != null && SelectedBrand.BrandId != 0 && variant.Product?.BrandId != SelectedBrand.BrandId)
        {
            return false;
        }

        if (SelectedCategory != null && SelectedCategory.CategoryId != 0 && variant.Product?.CategoryId != SelectedCategory.CategoryId)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        var keyword = SearchText.Trim().ToLowerInvariant();
        var productName = variant.Product?.ProductName?.ToLowerInvariant() ?? string.Empty;
        var brandName = variant.Product?.Brand?.BrandName?.ToLowerInvariant() ?? string.Empty;
        var categoryName = variant.Product?.Category?.CategoryName?.ToLowerInvariant() ?? string.Empty;
        var volume = variant.Volume?.ToLowerInvariant() ?? string.Empty;

        return productName.Contains(keyword)
            || brandName.Contains(keyword)
            || categoryName.Contains(keyword)
            || volume.Contains(keyword);
    }

    private bool CustomerFilterPredicate(object obj)
    {
        if (obj is not Customer customer)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(CustomerSearchText))
        {
            return true;
        }

        var keyword = CustomerSearchText.Trim();
        return (!string.IsNullOrWhiteSpace(customer.CustomerName)
                && customer.CustomerName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
               || (!string.IsNullOrWhiteSpace(customer.Phone)
                   && customer.Phone.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private void RefreshCustomerFilter()
    {
        CustomerOptionsView.Refresh();
        OnPropertyChanged(nameof(HasCustomerResults));
        OnPropertyChanged(nameof(ShowCustomerNoResults));
    }

    private void SetSelectedCustomer(Customer? customer, bool syncSearchText, bool closeSuggestions)
    {
        if (_selectedCustomer == customer)
        {
            if (closeSuggestions)
            {
                ShowCustomerSuggestions = false;
            }

            return;
        }

        _selectedCustomer = customer;
        OnPropertyChanged(nameof(SelectedCustomer));
        OnPropertyChanged(nameof(HasSelectedCustomer));
        OnPropertyChanged(nameof(SelectedCustomerName));
        OnPropertyChanged(nameof(SelectedCustomerPhone));
        OnPropertyChanged(nameof(SelectedCustomerAddress));
        OnPropertyChanged(nameof(CanCheckout));

        if (syncSearchText)
        {
            _isSyncingCustomerSearch = true;
            CustomerSearchText = customer?.CustomerName ?? string.Empty;
            _isSyncingCustomerSearch = false;
        }

        ShowCustomerSuggestions = closeSuggestions ? false : !string.IsNullOrWhiteSpace(CustomerSearchText);
        CommandManager.InvalidateRequerySuggested();
    }

    private void AddToCart(object? parameter)
    {
        if (parameter is not ProductVariant variant)
        {
            return;
        }

        var availableStock = variant.StockQuantity ?? 0;
        if (availableStock <= 0)
        {
            MessageBox.Show(
                "This variant is out of stock.",
                "Out of stock",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var existingLine = CartItems.FirstOrDefault(item => item.VariantId == variant.VariantId);
        if (existingLine != null)
        {
            if (existingLine.Quantity >= availableStock)
            {
                MessageBox.Show(
                    $"Only {availableStock} item(s) are available for this variant.",
                    "Stock limit",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            existingLine.Quantity += 1;
            return;
        }

        CartItems.Add(new StaffCartLineVM(variant));
    }

    private void IncreaseQuantity(object? parameter)
    {
        if (parameter is not StaffCartLineVM line)
        {
            return;
        }

        if (line.Quantity >= line.AvailableStock)
        {
            MessageBox.Show(
                $"Only {line.AvailableStock} item(s) are available for {line.ProductName} {line.Volume}.",
                "Stock limit",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        line.Quantity += 1;
    }

    private void DecreaseQuantity(object? parameter)
    {
        if (parameter is not StaffCartLineVM line)
        {
            return;
        }

        if (line.Quantity <= 1)
        {
            CartItems.Remove(line);
            return;
        }

        line.Quantity -= 1;
    }

    private void RemoveFromCart(object? parameter)
    {
        if (parameter is StaffCartLineVM line)
        {
            CartItems.Remove(line);
        }
    }

    private void Checkout()
    {
        if (!HasCartItems)
        {
            MessageBox.Show(
                "Add at least one product before checkout.",
                "Cart is empty",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        if (SelectedCustomer == null)
        {
            MessageBox.Show(
                "Select a customer before checkout.",
                "Customer required",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var currentUser = UserSession.CurrentUser;
        if (currentUser == null)
        {
            MessageBox.Show(
                "The staff session is no longer available. Please log in again.",
                "Session expired",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var context = new AppDbContext();
            using var transaction = context.Database.BeginTransaction();

            var selectedCustomerId = SelectedCustomer.CustomerId;
            var selectedCustomerName = SelectedCustomerName;

            var customerExists = context.Customers.Any(customer => customer.CustomerId == selectedCustomerId);
            if (!customerExists)
            {
                transaction.Rollback();
                LoadData();
                MessageBox.Show(
                    "The selected customer no longer exists. Please choose again.",
                    "Customer not found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var variantIds = CartItems.Select(item => item.VariantId).ToList();
            var stockMap = context.ProductVariants
                .Where(variant => variantIds.Contains(variant.VariantId))
                .ToDictionary(variant => variant.VariantId);

            var unavailableLines = new List<string>();
            foreach (var line in CartItems)
            {
                if (!stockMap.TryGetValue(line.VariantId, out var currentVariant) || currentVariant.IsActive != true)
                {
                    line.UpdateAvailableStock(0);
                    unavailableLines.Add($"{line.ProductName} {line.Volume} is no longer available.");
                    continue;
                }

                var currentStock = currentVariant.StockQuantity ?? 0;
                line.UpdateAvailableStock(currentStock);
                if (currentStock < line.Quantity)
                {
                    unavailableLines.Add($"{line.ProductName} {line.Volume} only has {currentStock} item(s) left.");
                }
            }

            if (unavailableLines.Count > 0)
            {
                transaction.Rollback();
                LoadData();
                MessageBox.Show(
                    "Unable to complete checkout because stock changed:\n\n" + string.Join("\n", unavailableLines),
                    "Checkout blocked",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var totalAmount = CartTotal;
            var order = new Order
            {
                CustomerId = selectedCustomerId,
                UserId = currentUser.UserId,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                FinalAmount = totalAmount,
                Status = "Completed"
            };

            context.Orders.Add(order);
            context.SaveChanges();

            foreach (var line in CartItems)
            {
                var currentVariant = stockMap[line.VariantId];
                currentVariant.StockQuantity = (currentVariant.StockQuantity ?? 0) - line.Quantity;

                context.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.OrderId,
                    VariantId = line.VariantId,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    ImportPrice = line.UnitPrice,
                    Subtotal = line.LineTotal
                });
            }

            context.SaveChanges();
            transaction.Commit();

            CartItems.Clear();
            SelectedCustomer = null;
            CustomerSearchText = string.Empty;
            LoadData();

            MessageBox.Show(
                $"Order #{order.OrderId} for {selectedCustomerName} was created successfully.\nTotal: {totalAmount:N0} VND",
                "Checkout complete",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Unable to complete checkout.\n{ex.Message}",
                "Checkout failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void RefreshCartAvailability(IEnumerable<ProductVariant> latestVariants)
    {
        var stockMap = latestVariants.ToDictionary(variant => variant.VariantId, variant => variant.StockQuantity ?? 0);
        foreach (var line in CartItems)
        {
            line.UpdateAvailableStock(stockMap.TryGetValue(line.VariantId, out var stock) ? stock : 0);
        }
    }

    private void CartItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (StaffCartLineVM item in e.OldItems)
            {
                item.PropertyChanged -= CartLine_PropertyChanged;
            }
        }

        if (e.NewItems != null)
        {
            foreach (StaffCartLineVM item in e.NewItems)
            {
                item.PropertyChanged += CartLine_PropertyChanged;
            }
        }

        UpdateCartState();
    }

    private void CartLine_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(StaffCartLineVM.Quantity) || e.PropertyName == nameof(StaffCartLineVM.LineTotal))
        {
            UpdateCartState();
        }
    }

    private void UpdateCartState()
    {
        OnPropertyChanged(nameof(TotalQuantity));
        OnPropertyChanged(nameof(CartTotal));
        OnPropertyChanged(nameof(HasCartItems));
        OnPropertyChanged(nameof(CanCheckout));
        CommandManager.InvalidateRequerySuggested();
    }
}
