using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CosmeticStoreManagement.Data;
using CosmeticStoreManagement.Helpers;
using Microsoft.EntityFrameworkCore;

namespace CosmeticStoreManagement.ViewModels.Staff;

public class StaffVoucherItem : BaseViewModel
{
    public int VoucherId { get; set; }

    public string VoucherCode { get; set; } = string.Empty;

    public string DiscountType { get; set; } = string.Empty;

    public decimal DiscountValue { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int Quantity { get; set; }

    public bool IsActive { get; set; }

    public string DiscountTypeDisplay =>
        string.Equals(DiscountType, "PERCENT", StringComparison.OrdinalIgnoreCase)
            ? "Percent"
            : "Fixed";

    public string DiscountDisplay =>
        string.Equals(DiscountType, "PERCENT", StringComparison.OrdinalIgnoreCase)
            ? $"{DiscountValue:N0}%"
            : $"{DiscountValue:N0} VND";

    public string ValidityDisplay
    {
        get
        {
            var startText = StartDate?.ToString("dd/MM/yyyy") ?? "-";
            var endText = EndDate?.ToString("dd/MM/yyyy") ?? "-";
            return $"{startText} - {endText}";
        }
    }

    public string StatusText
    {
        get
        {
            var today = DateTime.Today;

            if (!IsActive)
            {
                return "Inactive";
            }

            if (StartDate.HasValue && today < StartDate.Value.Date)
            {
                return "Upcoming";
            }

            if (EndDate.HasValue && today > EndDate.Value.Date)
            {
                return "Expired";
            }

            if (Quantity <= 0)
            {
                return "Out of stock";
            }

            return "Available";
        }
    }

    public string QuantityDisplay => $"{Math.Max(0, Quantity)} left";
}

public class StaffVoucherPageVM : BaseViewModel
{
    public ObservableCollection<StaffVoucherItem> Vouchers { get; } = new();

    public ICollectionView VouchersView { get; }

    public List<string> StatusOptions { get; } = new()
    {
        "All vouchers",
        "Available",
        "Out of stock",
        "Upcoming",
        "Expired",
        "Inactive"
    };

    public ICommand RefreshCommand { get; }

    public ICommand ClearFiltersCommand { get; }

    private StaffVoucherItem? _selectedVoucher;
    public StaffVoucherItem? SelectedVoucher
    {
        get => _selectedVoucher;
        set
        {
            if (_selectedVoucher == value)
            {
                return;
            }

            _selectedVoucher = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelectedVoucher));
            OnPropertyChanged(nameof(SelectedVoucherSummary));
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

    private string _selectedStatus = "All vouchers";
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

    public bool HasSelectedVoucher => SelectedVoucher != null;

    public string SelectedVoucherSummary =>
        SelectedVoucher == null
            ? "Choose one voucher to see its details and validity."
            : $"{SelectedVoucher.VoucherCode} - {SelectedVoucher.DiscountDisplay} - {SelectedVoucher.StatusText}";

    public int TotalVouchers => Vouchers.Count;

    public int AvailableVouchers => Vouchers.Count(voucher => voucher.StatusText == "Available");

    public int UpcomingVouchers => Vouchers.Count(voucher => voucher.StatusText == "Upcoming");

    public int ExpiredVouchers => Vouchers.Count(voucher => voucher.StatusText == "Expired");

    public StaffVoucherPageVM()
    {
        VouchersView = CollectionViewSource.GetDefaultView(Vouchers);
        VouchersView.Filter = FilterPredicate;

        RefreshCommand = new RelayCommand(_ => LoadVouchers());
        ClearFiltersCommand = new RelayCommand(_ => ClearFilters());

        LoadVouchers();
    }

    public async void LoadVouchers()
    {
        var previouslySelectedVoucherId = SelectedVoucher?.VoucherId;

        try
        {
            using var context = new AppDbContext();
            var vouchers = await context.Vouchers
                .AsNoTracking()
                .OrderBy(voucher => voucher.IsActive == true ? 0 : 1)
                .ThenBy(voucher => voucher.EndDate)
                .ThenBy(voucher => voucher.VoucherCode)
                .ToListAsync();

            Vouchers.Clear();
            foreach (var voucher in vouchers)
            {
                Vouchers.Add(new StaffVoucherItem
                {
                    VoucherId = voucher.VoucherId,
                    VoucherCode = voucher.VoucherCode,
                    DiscountType = voucher.DiscountType ?? string.Empty,
                    DiscountValue = voucher.DiscountValue ?? 0m,
                    StartDate = voucher.StartDate,
                    EndDate = voucher.EndDate,
                    Quantity = voucher.Quantity ?? 0,
                    IsActive = voucher.IsActive == true
                });
            }

            SelectedVoucher = previouslySelectedVoucherId.HasValue
                ? Vouchers.FirstOrDefault(voucher => voucher.VoucherId == previouslySelectedVoucherId.Value)
                : Vouchers.FirstOrDefault();

            OnPropertyChanged(nameof(TotalVouchers));
            OnPropertyChanged(nameof(AvailableVouchers));
            OnPropertyChanged(nameof(UpcomingVouchers));
            OnPropertyChanged(nameof(ExpiredVouchers));

            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Unable to load vouchers.\n{ex.Message}",
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
        VouchersView.Refresh();
    }

    private bool FilterPredicate(object obj)
    {
        if (obj is not StaffVoucherItem voucher)
        {
            return false;
        }

        if (!string.Equals(SelectedStatus, "All vouchers", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(voucher.StatusText, SelectedStatus, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        var keyword = SearchText.Trim().ToLowerInvariant();
        return voucher.VoucherCode.ToLowerInvariant().Contains(keyword)
            || voucher.DiscountType.ToLowerInvariant().Contains(keyword)
            || voucher.StatusText.ToLowerInvariant().Contains(keyword);
    }
}
