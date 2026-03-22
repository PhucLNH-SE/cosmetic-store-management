using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CosmeticStoreManagement.Data;
using CosmeticStoreManagement.Helpers;
using Microsoft.EntityFrameworkCore;

namespace CosmeticStoreManagement.ViewModels.Admin;

public class StatisticsSummaryCard
{
    public string Title { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string Note { get; set; } = string.Empty;

    public string AccentColor { get; set; } = "#FF8A5A44";

    public string BackgroundColor { get; set; } = "#FFF8EEE7";
}

public class StatisticsTrendPoint
{
    public string Label { get; set; } = string.Empty;

    public decimal Revenue { get; set; }

    public decimal Cost { get; set; }

    public double RevenueRatio { get; set; }

    public double CostRatio { get; set; }
}

public class StatisticsProductItem
{
    public string ProductName { get; set; } = string.Empty;

    public string VariantSummary { get; set; } = string.Empty;

    public int QuantitySold { get; set; }

    public decimal Revenue { get; set; }
}

public class StatisticsStaffItem
{
    public string StaffName { get; set; } = string.Empty;

    public int CompletedOrders { get; set; }

    public decimal Revenue { get; set; }

    public decimal AverageOrderValue { get; set; }
}

public class ViewStatisticsPageVM : BaseViewModel
{
    public ObservableCollection<StatisticsSummaryCard> SummaryCards { get; } = new();

    public ObservableCollection<StatisticsTrendPoint> DailyRevenueItems { get; } = new();

    public ObservableCollection<StatisticsProductItem> TopProducts { get; } = new();

    public ObservableCollection<StatisticsStaffItem> StaffPerformance { get; } = new();

    public List<string> PeriodOptions { get; } = new()
    {
        "Current month",
        "Last 30 days",
        "All time"
    };

    public ICommand RefreshCommand { get; }

    private string _selectedPeriod = "Current month";
    public string SelectedPeriod
    {
        get => _selectedPeriod;
        set
        {
            if (_selectedPeriod == value)
            {
                return;
            }

            _selectedPeriod = value;
            OnPropertyChanged();
            LoadStatistics();
        }
    }

    private string _periodDescription = string.Empty;
    public string PeriodDescription
    {
        get => _periodDescription;
        set
        {
            _periodDescription = value;
            OnPropertyChanged();
        }
    }

    private decimal _totalRevenue;
    public decimal TotalRevenue
    {
        get => _totalRevenue;
        set
        {
            _totalRevenue = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TotalRevenueDisplay));
        }
    }

    private decimal _totalImportCost;
    public decimal TotalImportCost
    {
        get => _totalImportCost;
        set
        {
            _totalImportCost = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TotalImportCostDisplay));
        }
    }

    private decimal _estimatedProfit;
    public decimal EstimatedProfit
    {
        get => _estimatedProfit;
        set
        {
            _estimatedProfit = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(EstimatedProfitDisplay));
        }
    }

    private int _totalOrders;
    public int TotalOrders
    {
        get => _totalOrders;
        set
        {
            _totalOrders = value;
            OnPropertyChanged();
        }
    }

    private int _completedOrders;
    public int CompletedOrders
    {
        get => _completedOrders;
        set
        {
            _completedOrders = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CompletedRateDisplay));
        }
    }

    private int _pendingOrders;
    public int PendingOrders
    {
        get => _pendingOrders;
        set
        {
            _pendingOrders = value;
            OnPropertyChanged();
        }
    }

    private int _cancelledOrders;
    public int CancelledOrders
    {
        get => _cancelledOrders;
        set
        {
            _cancelledOrders = value;
            OnPropertyChanged();
        }
    }

    private int _customersServed;
    public int CustomersServed
    {
        get => _customersServed;
        set
        {
            _customersServed = value;
            OnPropertyChanged();
        }
    }

    private int _voucherUsageCount;
    public int VoucherUsageCount
    {
        get => _voucherUsageCount;
        set
        {
            _voucherUsageCount = value;
            OnPropertyChanged();
        }
    }

    private int _totalItemsSold;
    public int TotalItemsSold
    {
        get => _totalItemsSold;
        set
        {
            _totalItemsSold = value;
            OnPropertyChanged();
        }
    }

    private decimal _averageOrderValue;
    public decimal AverageOrderValue
    {
        get => _averageOrderValue;
        set
        {
            _averageOrderValue = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AverageOrderValueDisplay));
        }
    }

    private double _averageRating;
    public double AverageRating
    {
        get => _averageRating;
        set
        {
            _averageRating = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AverageRatingDisplay));
        }
    }

    public string TotalRevenueDisplay => $"{TotalRevenue:N0} VND";

    public string TotalImportCostDisplay => $"{TotalImportCost:N0} VND";

    public string EstimatedProfitDisplay => $"{EstimatedProfit:N0} VND";

    public string AverageOrderValueDisplay => $"{AverageOrderValue:N0} VND";

    public string AverageRatingDisplay => AverageRating <= 0 ? "No rating yet" : $"{AverageRating:N1}/5";

    public string CompletedRateDisplay =>
        TotalOrders == 0 ? "0%" : $"{(double)CompletedOrders / TotalOrders:P0}";

    public ViewStatisticsPageVM()
    {
        RefreshCommand = new RelayCommand(_ => LoadStatistics());
        LoadStatistics();
    }

    public async void LoadStatistics()
    {
        try
        {
            var startDate = ResolveStartDate();
            var today = DateTime.Today;
            PeriodDescription = BuildPeriodDescription(startDate, today);

            using var context = new AppDbContext();

            var ordersQuery = context.Orders
                .AsNoTracking()
                .Include(order => order.User)
                .Include(order => order.OrderDetails)
                    .ThenInclude(detail => detail.Variant)
                    .ThenInclude(variant => variant.Product)
                .AsQueryable();

            var importsQuery = context.ImportOrders
                .AsNoTracking()
                .Include(importOrder => importOrder.ImportOrderDetails)
                .AsQueryable();

            if (startDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(order => order.OrderDate.HasValue && order.OrderDate.Value.Date >= startDate.Value.Date);
                importsQuery = importsQuery.Where(importOrder => importOrder.ImportDate.HasValue && importOrder.ImportDate.Value.Date >= startDate.Value.Date);
            }

            var orders = await ordersQuery.ToListAsync();
            var imports = await importsQuery.ToListAsync();

            var completedOrders = orders
                .Where(order => string.Equals(order.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var ratedOrders = completedOrders.Where(order => order.Rating.HasValue && order.Rating.Value > 0).ToList();

            TotalOrders = orders.Count;
            CompletedOrders = completedOrders.Count;
            PendingOrders = orders.Count(order => string.Equals(order.Status, "Pending", StringComparison.OrdinalIgnoreCase));
            CancelledOrders = orders.Count(order => string.Equals(order.Status, "Cancelled", StringComparison.OrdinalIgnoreCase));
            CustomersServed = completedOrders.Select(order => order.CustomerId).Distinct().Count();
            VoucherUsageCount = completedOrders.Count(order => order.VoucherId.HasValue);
            TotalItemsSold = completedOrders.SelectMany(order => order.OrderDetails).Sum(detail => detail.Quantity ?? 0);
            TotalRevenue = completedOrders.Sum(order => order.FinalAmount ?? 0m);
            TotalImportCost = imports.Sum(importOrder => importOrder.TotalCost ?? 0m);
            AverageOrderValue = CompletedOrders == 0 ? 0m : TotalRevenue / CompletedOrders;
            AverageRating = ratedOrders.Count == 0 ? 0 : ratedOrders.Average(order => order.Rating ?? 0);
            EstimatedProfit = completedOrders
                .SelectMany(order => order.OrderDetails)
                .Sum(detail => (detail.Subtotal ?? 0m) - ((detail.ImportPrice ?? 0m) * (detail.Quantity ?? 0)));

            RebuildSummaryCards();
            RebuildDailyRevenue(completedOrders, imports);
            RebuildTopProducts(completedOrders);
            RebuildStaffPerformance(completedOrders);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Unable to load statistics.\n{ex.Message}",
                "Statistics load failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void RebuildSummaryCards()
    {
        SummaryCards.Clear();
        SummaryCards.Add(new StatisticsSummaryCard
        {
            Title = "Revenue",
            Value = TotalRevenueDisplay,
            Note = $"{CompletedOrders} completed order(s)",
            AccentColor = "#FF8A4F32",
            BackgroundColor = "#FFF8ECE4"
        });
        SummaryCards.Add(new StatisticsSummaryCard
        {
            Title = "Estimated Profit",
            Value = EstimatedProfitDisplay,
            Note = "Based on order detail cost and sale price",
            AccentColor = "#FF3F6E54",
            BackgroundColor = "#FFEAF4ED"
        });
        SummaryCards.Add(new StatisticsSummaryCard
        {
            Title = "Import Cost",
            Value = TotalImportCostDisplay,
            Note = "Total import spending in selected period",
            AccentColor = "#FFB2742A",
            BackgroundColor = "#FFFDF1E1"
        });
        SummaryCards.Add(new StatisticsSummaryCard
        {
            Title = "Customers Served",
            Value = CustomersServed.ToString(),
            Note = $"Voucher used in {VoucherUsageCount} completed order(s)",
            AccentColor = "#FF7A4E73",
            BackgroundColor = "#FFF8EDF4"
        });
    }

    private void RebuildDailyRevenue(List<Models.Order> completedOrders, List<Models.ImportOrder> imports)
    {
        DailyRevenueItems.Clear();

        var revenueByDate = completedOrders
            .Where(order => order.OrderDate.HasValue)
            .GroupBy(order => order.OrderDate!.Value.Date)
            .ToDictionary(group => group.Key, group => group.Sum(order => order.FinalAmount ?? 0m));

        var costByDate = imports
            .Where(importOrder => importOrder.ImportDate.HasValue)
            .GroupBy(importOrder => importOrder.ImportDate!.Value.Date)
            .ToDictionary(group => group.Key, group => group.Sum(importOrder => importOrder.TotalCost ?? 0m));

        var dates = revenueByDate.Keys
            .Union(costByDate.Keys)
            .OrderByDescending(date => date)
            .Take(7)
            .OrderBy(date => date)
            .ToList();

        if (dates.Count == 0)
        {
            return;
        }

        var peakValue = dates
            .Select(date => Math.Max(revenueByDate.GetValueOrDefault(date), costByDate.GetValueOrDefault(date)))
            .DefaultIfEmpty(0m)
            .Max();

        var divisor = peakValue <= 0 ? 1m : peakValue;
        foreach (var date in dates)
        {
            var revenue = revenueByDate.GetValueOrDefault(date);
            var cost = costByDate.GetValueOrDefault(date);

            DailyRevenueItems.Add(new StatisticsTrendPoint
            {
                Label = date.ToString("dd/MM"),
                Revenue = revenue,
                Cost = cost,
                RevenueRatio = (double)(revenue / divisor * 100m),
                CostRatio = (double)(cost / divisor * 100m)
            });
        }
    }

    private void RebuildTopProducts(List<Models.Order> completedOrders)
    {
        TopProducts.Clear();

        var topProducts = completedOrders
            .SelectMany(order => order.OrderDetails)
            .GroupBy(detail => new
            {
                ProductName = detail.Variant?.Product?.ProductName ?? "Unknown product",
                Volume = detail.Variant?.Volume ?? "-"
            })
            .Select(group => new StatisticsProductItem
            {
                ProductName = group.Key.ProductName,
                VariantSummary = $"Variant: {group.Key.Volume}",
                QuantitySold = group.Sum(detail => detail.Quantity ?? 0),
                Revenue = group.Sum(detail => detail.Subtotal ?? 0m)
            })
            .OrderByDescending(item => item.QuantitySold)
            .ThenByDescending(item => item.Revenue)
            .Take(5)
            .ToList();

        foreach (var item in topProducts)
        {
            TopProducts.Add(item);
        }
    }

    private void RebuildStaffPerformance(List<Models.Order> completedOrders)
    {
        StaffPerformance.Clear();

        var staffItems = completedOrders
            .GroupBy(order => order.User?.FullName ?? order.User?.Username ?? $"User #{order.UserId}")
            .Select(group => new StatisticsStaffItem
            {
                StaffName = group.Key,
                CompletedOrders = group.Count(),
                Revenue = group.Sum(order => order.FinalAmount ?? 0m),
                AverageOrderValue = group.Any() ? group.Average(order => order.FinalAmount ?? 0m) : 0m
            })
            .OrderByDescending(item => item.Revenue)
            .ThenByDescending(item => item.CompletedOrders)
            .Take(5)
            .ToList();

        foreach (var item in staffItems)
        {
            StaffPerformance.Add(item);
        }
    }

    private DateTime? ResolveStartDate()
    {
        var today = DateTime.Today;
        return SelectedPeriod switch
        {
            "Current month" => new DateTime(today.Year, today.Month, 1),
            "Last 30 days" => today.AddDays(-29),
            _ => null
        };
    }

    private string BuildPeriodDescription(DateTime? startDate, DateTime today)
    {
        if (!startDate.HasValue)
        {
            return "Showing all recorded business data in the system.";
        }

        return $"Showing data from {startDate.Value:dd/MM/yyyy} to {today:dd/MM/yyyy}.";
    }
}
