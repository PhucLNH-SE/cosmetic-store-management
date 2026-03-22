using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CosmeticStoreManagement.Data;
using CosmeticStoreManagement.Helpers;
using Microsoft.EntityFrameworkCore;

namespace CosmeticStoreManagement.ViewModels.Staff;

public class StaffOrderLineItem
{
    public int OrderDetailId { get; set; }

    public int VariantId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string Volume { get; set; } = "-";

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Subtotal { get; set; }

    public string ImagePath { get; set; } = string.Empty;
}

public class StaffOrderItem : BaseViewModel
{
    private string _status = string.Empty;

    public int OrderId { get; set; }

    public DateTime? OrderDate { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerPhone { get; set; } = string.Empty;

    public string VoucherCode { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public decimal FinalAmount { get; set; }

    public int ItemCount { get; set; }

    public string Status
    {
        get => _status;
        set
        {
            if (_status == value)
            {
                return;
            }

            _status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusBadge));
        }
    }

    public int? Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public ObservableCollection<StaffOrderLineItem> Lines { get; set; } = new();

    public string StatusBadge => string.IsNullOrWhiteSpace(Status) ? "Unknown" : Status;
}

public class StaffOrdersPageVM : BaseViewModel
{
    public ObservableCollection<StaffOrderItem> Orders { get; } = new();

    public ICollectionView OrdersView { get; }

    public List<string> StatusOptions { get; } = new()
    {
        "All statuses",
        "Pending",
        "Completed",
        "Cancelled"
    };

    public ICommand RefreshCommand { get; }

    public ICommand ClearFiltersCommand { get; }

    public ICommand CompleteOrderCommand { get; }

    public ICommand CancelOrderCommand { get; }

    private StaffOrderItem? _selectedOrder;
    public StaffOrderItem? SelectedOrder
    {
        get => _selectedOrder;
        set
        {
            if (_selectedOrder == value)
            {
                return;
            }

            _selectedOrder = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelectedOrder));
            OnPropertyChanged(nameof(SelectedOrderLines));
            OnPropertyChanged(nameof(SelectedOrderSummary));
            CommandManager.InvalidateRequerySuggested();
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

    private string _selectedStatus = "All statuses";
    public string SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            if (_selectedStatus == value)
            {
                return;
            }

            _selectedStatus = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

    public bool HasSelectedOrder => SelectedOrder != null;

    public IEnumerable<StaffOrderLineItem>? SelectedOrderLines => SelectedOrder?.Lines;

    public string SelectedOrderSummary
    {
        get
        {
            if (SelectedOrder == null)
            {
                return "Choose an order to inspect its details.";
            }

            return $"Order #{SelectedOrder.OrderId} • {SelectedOrder.ItemCount} line(s) • {SelectedOrder.FinalAmount:N0} VND";
        }
    }

    public int TotalOrders => Orders.Count;

    public int PendingOrders => Orders.Count(order => order.Status == "Pending");

    public int CompletedOrders => Orders.Count(order => order.Status == "Completed");

    public int CancelledOrders => Orders.Count(order => order.Status == "Cancelled");

    public StaffOrdersPageVM()
    {
        OrdersView = CollectionViewSource.GetDefaultView(Orders);
        OrdersView.Filter = FilterPredicate;

        RefreshCommand = new RelayCommand(_ => LoadOrders());
        ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
        CompleteOrderCommand = new RelayCommand(_ => UpdateOrderStatus("Completed"), _ => CanChangeStatus("Completed"));
        CancelOrderCommand = new RelayCommand(_ => UpdateOrderStatus("Cancelled"), _ => CanChangeStatus("Cancelled"));

        LoadOrders();
    }

    public async void LoadOrders()
    {
        var currentUser = UserSession.CurrentUser;
        if (currentUser == null)
        {
            MessageBox.Show(
                "The staff session is not available. Please log in again.",
                "Session expired",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var previouslySelectedOrderId = SelectedOrder?.OrderId;

        try
        {
            using var context = new AppDbContext();

            var orders = await context.Orders
                .AsNoTracking()
                .Include(order => order.Customer)
                .Include(order => order.Voucher)
                .Include(order => order.OrderDetails)
                    .ThenInclude(detail => detail.Variant)
                    .ThenInclude(variant => variant.Product)
                .Where(order => order.UserId == currentUser.UserId)
                .OrderByDescending(order => order.OrderDate)
                .ThenByDescending(order => order.OrderId)
                .ToListAsync();

            Orders.Clear();
            foreach (var order in orders)
            {
                var lines = order.OrderDetails
                    .OrderBy(detail => detail.OrderDetailId)
                    .Select(detail => new StaffOrderLineItem
                    {
                        OrderDetailId = detail.OrderDetailId,
                        VariantId = detail.VariantId,
                        ProductName = detail.Variant?.Product?.ProductName ?? "Unknown product",
                        Volume = string.IsNullOrWhiteSpace(detail.Variant?.Volume) ? "-" : detail.Variant.Volume!,
                        Quantity = detail.Quantity ?? 0,
                        UnitPrice = detail.UnitPrice ?? 0m,
                        Subtotal = detail.Subtotal ?? 0m,
                        ImagePath = detail.Variant?.ImagePath ?? string.Empty
                    })
                    .ToList();

                Orders.Add(new StaffOrderItem
                {
                    OrderId = order.OrderId,
                    OrderDate = order.OrderDate,
                    CustomerName = order.Customer?.CustomerName ?? "Unknown customer",
                    CustomerPhone = order.Customer?.Phone ?? string.Empty,
                    VoucherCode = order.Voucher?.VoucherCode ?? string.Empty,
                    TotalAmount = order.TotalAmount ?? 0m,
                    FinalAmount = order.FinalAmount ?? 0m,
                    Status = order.Status ?? string.Empty,
                    Rating = order.Rating,
                    Comment = order.Comment ?? string.Empty,
                    ItemCount = lines.Sum(line => line.Quantity),
                    Lines = new ObservableCollection<StaffOrderLineItem>(lines)
                });
            }

            SelectedOrder = previouslySelectedOrderId.HasValue
                ? Orders.FirstOrDefault(order => order.OrderId == previouslySelectedOrderId.Value)
                : Orders.FirstOrDefault();

            OnPropertyChanged(nameof(TotalOrders));
            OnPropertyChanged(nameof(PendingOrders));
            OnPropertyChanged(nameof(CompletedOrders));
            OnPropertyChanged(nameof(CancelledOrders));

            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Unable to load orders.\n{ex.Message}",
                "Load failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    public void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedStatus = StatusOptions.First();
    }

    public void ApplyFilter()
    {
        OrdersView.Refresh();
    }

    private bool FilterPredicate(object obj)
    {
        if (obj is not StaffOrderItem order)
        {
            return false;
        }

        if (!string.Equals(SelectedStatus, "All statuses", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(order.Status, SelectedStatus, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        var keyword = SearchText.Trim().ToLowerInvariant();
        return order.OrderId.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || order.CustomerName.ToLowerInvariant().Contains(keyword)
            || order.CustomerPhone.ToLowerInvariant().Contains(keyword)
            || order.Status.ToLowerInvariant().Contains(keyword)
            || order.VoucherCode.ToLowerInvariant().Contains(keyword);
    }

    private bool CanChangeStatus(string nextStatus)
    {
        if (SelectedOrder == null)
        {
            return false;
        }

        return string.Equals(SelectedOrder.Status, "Pending", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(SelectedOrder.Status, nextStatus, StringComparison.OrdinalIgnoreCase);
    }

    private void UpdateOrderStatus(string nextStatus)
    {
        if (SelectedOrder == null)
        {
            return;
        }

        var currentUser = UserSession.CurrentUser;
        if (currentUser == null)
        {
            MessageBox.Show(
                "The staff session is not available. Please log in again.",
                "Session expired",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var context = new AppDbContext();
            var order = context.Orders.FirstOrDefault(item => item.OrderId == SelectedOrder.OrderId && item.UserId == currentUser.UserId);
            if (order == null)
            {
                MessageBox.Show(
                    "The selected order could not be found.",
                    "Order missing",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (!string.Equals(order.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "Only pending orders can be updated from this page.",
                    "Status locked",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            order.Status = nextStatus;
            context.SaveChanges();

            SelectedOrder.Status = nextStatus;
            OrdersView.Refresh();
            OnPropertyChanged(nameof(PendingOrders));
            OnPropertyChanged(nameof(CompletedOrders));
            OnPropertyChanged(nameof(CancelledOrders));
            CommandManager.InvalidateRequerySuggested();

            MessageBox.Show(
                $"Order #{order.OrderId} was updated to {nextStatus}.",
                "Order updated",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Unable to update the order.\n{ex.Message}",
                "Update failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
