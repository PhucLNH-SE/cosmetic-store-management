using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CosmeticStoreManagement.Helpers;
using CosmeticStoreManagement.Models;
using CosmeticStoreManagement.Services;

namespace CosmeticStoreManagement.ViewModels;

public class AdminHomeViewModel : BaseViewModel
{
    private readonly User _currentUser;
    private readonly IAdminCatalogService _catalogService;
    private readonly IOrderCheckoutService _checkoutService;
    private readonly INavigationService _navigationService;

    private readonly ObservableCollection<ProductCardItem> _products;
    private readonly ObservableCollection<OrderLineItem> _orderItems;

    private SidebarModuleItem? _selectedModuleItem;
    private string _searchText = string.Empty;
    private string _selectedSortOption = "A - Z";
    private string _customerPhone = string.Empty;
    private string _customerName = string.Empty;
    private string _statusMessage = string.Empty;
    private Brush _statusBrush = Brushes.SeaGreen;
    private int _filteredProductCount;

    public AdminHomeViewModel(
        User user,
        IAdminCatalogService catalogService,
        IOrderCheckoutService checkoutService,
        INavigationService navigationService)
    {
        _currentUser = user;
        _catalogService = catalogService;
        _checkoutService = checkoutService;
        _navigationService = navigationService;

        _products = new ObservableCollection<ProductCardItem>();
        _orderItems = new ObservableCollection<OrderLineItem>();

        ModuleItems = new ObservableCollection<SidebarModuleItem>();
        OrderItems = _orderItems;

        SortOptions = new ObservableCollection<string>
        {
            "A - Z",
            "Z - A",
            "Price Low - High",
            "Price High - Low"
        };

        ProductView = CollectionViewSource.GetDefaultView(_products);
        ProductView.Filter = FilterProducts;

        AddToOrderCommand = new RelayCommand(ExecuteAddToOrder, CanExecuteAddToOrder);
        RemoveOrderItemCommand = new RelayCommand(ExecuteRemoveOrderItem);
        ClearOrderCommand = new RelayCommand(ExecuteClearOrder, _ => OrderItems.Count > 0);
        CheckoutCommand = new RelayCommand(ExecuteCheckout, _ => OrderItems.Count > 0);
        RefreshCatalogCommand = new RelayCommand(_ => LoadCatalogData());
        SelectModuleCommand = new RelayCommand(ExecuteSelectModule);
        LogoutCommand = new RelayCommand(_ => _navigationService.NavigateToLogin());

        _orderItems.CollectionChanged += (_, _) => RefreshOrderSummary();

        WelcomeText = string.IsNullOrWhiteSpace(user.FullName)
            ? user.Username
            : user.FullName;

        HeaderDateText = DateTime.Now.ToString("dddd, dd/MM/yyyy");
        RoleText = user.Role ?? "Unknown";

        LoadModules();
        LoadCatalogData();
    }

    public ObservableCollection<SidebarModuleItem> ModuleItems { get; }

    public ObservableCollection<OrderLineItem> OrderItems { get; }

    public ObservableCollection<string> SortOptions { get; }

    public ICollectionView ProductView { get; }

    public ICommand AddToOrderCommand { get; }

    public ICommand RemoveOrderItemCommand { get; }

    public ICommand ClearOrderCommand { get; }

    public ICommand CheckoutCommand { get; }

    public ICommand RefreshCatalogCommand { get; }

    public ICommand SelectModuleCommand { get; }

    public ICommand LogoutCommand { get; }

    public SidebarModuleItem? SelectedModuleItem
    {
        get => _selectedModuleItem;
        set
        {
            _selectedModuleItem = value;
            OnPropertyChanged();
            UpdateModuleSelection();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            ApplySortAndFilter();
        }
    }

    public string SelectedSortOption
    {
        get => _selectedSortOption;
        set
        {
            _selectedSortOption = value;
            OnPropertyChanged();
            ApplySortAndFilter();
        }
    }

    public string CustomerPhone
    {
        get => _customerPhone;
        set
        {
            _customerPhone = value;
            OnPropertyChanged();
        }
    }

    public string CustomerName
    {
        get => _customerName;
        set
        {
            _customerName = value;
            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public Brush StatusBrush
    {
        get => _statusBrush;
        private set
        {
            _statusBrush = value;
            OnPropertyChanged();
        }
    }

    public string WelcomeText { get; }

    public string HeaderDateText { get; }

    public string RoleText { get; }

    public int FilteredProductCount
    {
        get => _filteredProductCount;
        private set
        {
            _filteredProductCount = value;
            OnPropertyChanged();
        }
    }

    public int TotalQuantity => OrderItems.Sum(item => item.Quantity);

    public decimal TotalAmount => OrderItems.Sum(item => item.LineTotal);

    private void LoadModules()
    {
        ModuleItems.Clear();

        var stats = _catalogService.GetDashboardStats();
        ModuleItems.Add(new SidebarModuleItem("Menu", stats.Products, true));
        ModuleItems.Add(new SidebarModuleItem("Brands", stats.Brands));
        ModuleItems.Add(new SidebarModuleItem("Categories", stats.Categories));
        ModuleItems.Add(new SidebarModuleItem("Customers", stats.Customers));
        ModuleItems.Add(new SidebarModuleItem("Orders", stats.Orders));
        ModuleItems.Add(new SidebarModuleItem("Import Orders", stats.ImportOrders));
        ModuleItems.Add(new SidebarModuleItem("Vouchers", stats.Vouchers));
        ModuleItems.Add(new SidebarModuleItem("Users", stats.Users));

        SelectedModuleItem = ModuleItems.FirstOrDefault();
    }

    private void LoadCatalogData()
    {
        try
        {
            _products.Clear();
            var catalog = _catalogService.GetActiveCatalog();

            foreach (var row in catalog)
            {
                _products.Add(new ProductCardItem
                {
                    VariantId = row.VariantId,
                    ProductId = row.ProductId,
                    ProductName = row.ProductName,
                    BrandName = row.BrandName,
                    CategoryName = row.CategoryName,
                    Volume = row.Volume,
                    Sku = row.Sku,
                    Price = row.Price,
                    StockQuantity = row.StockQuantity,
                    ImageAbsolutePath = row.ImageAbsolutePath
                });
            }

            ApplySortAndFilter();
            SetInfo($"Loaded {catalog.Count} active product variants.");
        }
        catch (Exception ex)
        {
            SetError($"Cannot load products: {ex.Message}");
        }
    }

    private bool FilterProducts(object candidate)
    {
        if (candidate is not ProductCardItem product)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        var keyword = SearchText.Trim();
        return product.ProductName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || product.BrandName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || product.CategoryName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || product.VolumeText.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || product.SkuText.Contains(keyword, StringComparison.OrdinalIgnoreCase);
    }

    private void ApplySortAndFilter()
    {
        ProductView.SortDescriptions.Clear();

        switch (SelectedSortOption)
        {
            case "Z - A":
                ProductView.SortDescriptions.Add(new SortDescription(nameof(ProductCardItem.ProductName), ListSortDirection.Descending));
                break;
            case "Price Low - High":
                ProductView.SortDescriptions.Add(new SortDescription(nameof(ProductCardItem.Price), ListSortDirection.Ascending));
                break;
            case "Price High - Low":
                ProductView.SortDescriptions.Add(new SortDescription(nameof(ProductCardItem.Price), ListSortDirection.Descending));
                break;
            default:
                ProductView.SortDescriptions.Add(new SortDescription(nameof(ProductCardItem.ProductName), ListSortDirection.Ascending));
                break;
        }

        ProductView.Refresh();
        FilteredProductCount = ProductView.Cast<object>().Count();
    }

    private bool CanExecuteAddToOrder(object parameter)
    {
        return parameter is ProductCardItem product && product.StockQuantity > 0;
    }

    private void ExecuteAddToOrder(object parameter)
    {
        if (parameter is not ProductCardItem product)
        {
            return;
        }

        if (product.StockQuantity <= 0)
        {
            SetError("Product is out of stock.");
            return;
        }

        var currentLine = OrderItems.FirstOrDefault(item => item.VariantId == product.VariantId);
        if (currentLine == null)
        {
            OrderItems.Add(new OrderLineItem
            {
                VariantId = product.VariantId,
                ProductName = product.ProductName,
                VolumeText = product.VolumeText,
                UnitPrice = product.Price,
                Quantity = 1
            });

            SetInfo($"Added {product.ProductName} to current order.");
            return;
        }

        if (currentLine.Quantity + 1 > product.StockQuantity)
        {
            SetError($"Stock is not enough for {product.ProductName}.");
            return;
        }

        currentLine.Quantity += 1;
        RefreshOrderSummary();
        SetInfo($"Updated quantity for {product.ProductName}.");
    }

    private void ExecuteRemoveOrderItem(object parameter)
    {
        if (parameter is not OrderLineItem line)
        {
            return;
        }

        OrderItems.Remove(line);
        SetInfo($"Removed {line.DisplayName} from order.");
    }

    private void ExecuteClearOrder(object parameter)
    {
        OrderItems.Clear();
        SetInfo("Order has been cleared.");
    }

    private void ExecuteCheckout(object parameter)
    {
        if (OrderItems.Count == 0)
        {
            SetError("No items in order.");
            return;
        }

        if (string.IsNullOrWhiteSpace(CustomerPhone))
        {
            SetError("Please enter customer phone.");
            return;
        }

        var request = new CheckoutRequest
        {
            UserId = _currentUser.UserId,
            CustomerPhone = CustomerPhone,
            CustomerName = CustomerName,
            Lines = OrderItems.Select(item => new CheckoutLineRequest
            {
                VariantId = item.VariantId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };

        var result = _checkoutService.Checkout(request);
        if (!result.IsSuccess)
        {
            SetError(result.Message);
            return;
        }

        OrderItems.Clear();
        CustomerPhone = string.Empty;
        CustomerName = string.Empty;

        LoadModules();
        LoadCatalogData();

        SetInfo($"Order #{result.OrderId} created successfully.");
    }

    private void ExecuteSelectModule(object parameter)
    {
        if (parameter is SidebarModuleItem item)
        {
            SelectedModuleItem = item;
        }
    }

    private void UpdateModuleSelection()
    {
        foreach (var module in ModuleItems)
        {
            module.IsActive = module == SelectedModuleItem;
        }

        if (SelectedModuleItem != null && !string.Equals(SelectedModuleItem.Title, "Menu", StringComparison.OrdinalIgnoreCase))
        {
            SetInfo($"Selected module: {SelectedModuleItem.Title}. You can extend this module next.");
        }
    }

    private void RefreshOrderSummary()
    {
        OnPropertyChanged(nameof(TotalQuantity));
        OnPropertyChanged(nameof(TotalAmount));
        CommandManager.InvalidateRequerySuggested();
    }

    private void SetInfo(string message)
    {
        StatusMessage = message;
        StatusBrush = Brushes.SeaGreen;
    }

    private void SetError(string message)
    {
        StatusMessage = message;
        StatusBrush = Brushes.IndianRed;
    }
}

public class SidebarModuleItem : BaseViewModel
{
    private bool _isActive;

    public SidebarModuleItem(string title, int count, bool isActive = false)
    {
        Title = title;
        Count = count;
        _isActive = isActive;
    }

    public string Title { get; }

    public int Count { get; }

    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            OnPropertyChanged();
        }
    }
}

public class ProductCardItem
{
    public int VariantId { get; init; }

    public int ProductId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public string BrandName { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public string? Volume { get; init; }

    public string? Sku { get; init; }

    public decimal Price { get; init; }

    public int StockQuantity { get; init; }

    public string ImageAbsolutePath { get; init; } = string.Empty;

    public string VolumeText => string.IsNullOrWhiteSpace(Volume) ? "Standard" : Volume;

    public string SkuText => string.IsNullOrWhiteSpace(Sku) ? "No SKU" : Sku;

    public string Subtitle => $"{BrandName} | {CategoryName} | {VolumeText}";

    public string StockText => StockQuantity > 0 ? $"Stock: {StockQuantity}" : "Out of stock";
}

public class OrderLineItem : BaseViewModel
{
    private int _quantity;

    public int VariantId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public string VolumeText { get; init; } = string.Empty;

    public decimal UnitPrice { get; init; }

    public int Quantity
    {
        get => _quantity;
        set
        {
            _quantity = value < 1 ? 1 : value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LineTotal));
        }
    }

    public decimal LineTotal => UnitPrice * Quantity;

    public string DisplayName => string.IsNullOrWhiteSpace(VolumeText)
        ? ProductName
        : $"{ProductName} ({VolumeText})";
}

