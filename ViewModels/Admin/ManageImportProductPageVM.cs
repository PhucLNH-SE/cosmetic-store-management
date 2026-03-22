using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CosmeticStoreManagement.Data;
using CosmeticStoreManagement.Helpers;
using Microsoft.EntityFrameworkCore;

namespace CosmeticStoreManagement.ViewModels.Admin;

public class ImportVariantItem : BaseViewModel
{
    public int VariantId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Volume { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
}

public class ImportDraftLineItem : BaseViewModel
{
    private int _quantity;
    private decimal _importPrice;

    public int VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string Volume { get; set; } = string.Empty;
    public int CurrentStock { get; set; }

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity == value) return;
            _quantity = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LineTotal));
        }
    }

    public decimal ImportPrice
    {
        get => _importPrice;
        set
        {
            if (_importPrice == value) return;
            _importPrice = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LineTotal));
        }
    }

    public decimal LineTotal => Quantity * ImportPrice;
}

public class ImportHistoryItem
{
    public int ImportId { get; set; }
    public DateTime ImportDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public decimal TotalCost { get; set; }
    public List<ImportHistoryDetailItem> Details { get; set; } = new();
}

public class ImportHistoryDetailItem
{
    public string ProductName { get; set; } = string.Empty;
    public string Volume { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal ImportPrice { get; set; }
    public decimal LineTotal => Quantity * ImportPrice;
}

public class ManageImportProductPageVM : BaseViewModel
{
    public ObservableCollection<ImportVariantItem> AvailableVariants { get; } = new();
    public ObservableCollection<ImportDraftLineItem> DraftLines { get; } = new();
    public ObservableCollection<ImportHistoryItem> ImportHistory { get; } = new();

    public ICollectionView AvailableVariantsView { get; }

    private ImportVariantItem? _selectedVariant;
    public ImportVariantItem? SelectedVariant
    {
        get => _selectedVariant;
        set
        {
            if (_selectedVariant == value) return;
            _selectedVariant = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedVariantSummary));
            UpdateDraftGuidance();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private ImportDraftLineItem? _selectedDraftLine;
    public ImportDraftLineItem? SelectedDraftLine
    {
        get => _selectedDraftLine;
        set
        {
            if (_selectedDraftLine == value) return;
            _selectedDraftLine = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private ImportHistoryItem? _selectedHistoryItem;
    public ImportHistoryItem? SelectedHistoryItem
    {
        get => _selectedHistoryItem;
        set
        {
            if (_selectedHistoryItem == value) return;
            _selectedHistoryItem = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedHistoryDetails));
            OnPropertyChanged(nameof(SelectedHistorySummary));
        }
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value) return;
            _searchText = value;
            OnPropertyChanged();
            AvailableVariantsView.Refresh();
        }
    }

    private int _importQuantity = 1;
    public int ImportQuantity
    {
        get => _importQuantity;
        set
        {
            if (_importQuantity == value) return;
            _importQuantity = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PreviewLineTotal));
            UpdateDraftGuidance();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private decimal _importPrice;
    public decimal ImportPrice
    {
        get => _importPrice;
        set
        {
            if (_importPrice == value) return;
            _importPrice = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PreviewLineTotal));
            UpdateDraftGuidance();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private string _statusMessage = "Choose a product variant, set quantity and import price, then add it to the draft.";
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public ICommand AddToDraftCommand { get; }
    public ICommand RemoveDraftLineCommand { get; }
    public ICommand ClearDraftCommand { get; }
    public ICommand SaveImportCommand { get; }
    public ICommand RefreshCommand { get; }

    public int TotalVariants => AvailableVariants.Count;
    public int DraftItemCount => DraftLines.Sum(item => item.Quantity);
    public decimal DraftTotalCost => DraftLines.Sum(item => item.LineTotal);
    public decimal PreviewLineTotal => ImportQuantity * ImportPrice;
    public bool HasDraftLines => DraftLines.Count > 0;

    public string SelectedVariantSummary =>
        SelectedVariant == null
            ? "Select one variant from the list to prepare an import line."
            : $"{SelectedVariant.ProductName} - {SelectedVariant.Volume} - Current stock: {SelectedVariant.StockQuantity}";

    public string ImportFlowNote =>
        "Add Line only creates a draft row. Stock and import history update after you click Save Import Order.";

    public IEnumerable<ImportHistoryDetailItem>? SelectedHistoryDetails => SelectedHistoryItem?.Details;

    public string SelectedHistorySummary =>
        SelectedHistoryItem == null
            ? "Select one import order to inspect its detail lines."
            : $"Import #{SelectedHistoryItem.ImportId} - {SelectedHistoryItem.TotalItems} item(s) - {SelectedHistoryItem.TotalCost:N0} VND";

    public ManageImportProductPageVM()
    {
        AvailableVariantsView = CollectionViewSource.GetDefaultView(AvailableVariants);
        AvailableVariantsView.Filter = FilterVariant;

        AddToDraftCommand = new RelayCommand(_ => AddToDraft(), _ => CanAddToDraft());
        RemoveDraftLineCommand = new RelayCommand(_ => RemoveDraftLine(), _ => SelectedDraftLine != null);
        ClearDraftCommand = new RelayCommand(_ => ClearDraft(), _ => DraftLines.Count > 0);
        SaveImportCommand = new RelayCommand(_ => SaveImport(), _ => DraftLines.Count > 0);
        RefreshCommand = new RelayCommand(_ => LoadData());

        DraftLines.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(DraftItemCount));
            OnPropertyChanged(nameof(DraftTotalCost));
            OnPropertyChanged(nameof(HasDraftLines));
            CommandManager.InvalidateRequerySuggested();
        };

        LoadData();
        UpdateDraftGuidance();
    }

    public async void LoadData()
    {
        var selectedVariantId = SelectedVariant?.VariantId;
        var selectedImportId = SelectedHistoryItem?.ImportId;

        try
        {
            using var context = new AppDbContext();

            var variants = await context.ProductVariants
                .AsNoTracking()
                .Include(variant => variant.Product)
                    .ThenInclude(product => product!.Brand)
                .Include(variant => variant.Product)
                    .ThenInclude(product => product!.Category)
                .Where(variant => variant.Product != null && variant.Product.IsActive == true && variant.IsActive == true)
                .OrderBy(variant => variant.Product!.ProductName)
                .ThenBy(variant => variant.Volume)
                .ToListAsync();

            var importOrders = await context.ImportOrders
                .AsNoTracking()
                .Include(importOrder => importOrder.User)
                .Include(importOrder => importOrder.ImportOrderDetails)
                    .ThenInclude(detail => detail.Variant)
                    .ThenInclude(variant => variant.Product)
                .OrderByDescending(importOrder => importOrder.ImportDate)
                .ThenByDescending(importOrder => importOrder.ImportId)
                .Take(20)
                .ToListAsync();

            AvailableVariants.Clear();
            foreach (var variant in variants)
            {
                AvailableVariants.Add(new ImportVariantItem
                {
                    VariantId = variant.VariantId,
                    ProductId = variant.ProductId,
                    ProductName = variant.Product?.ProductName ?? "Unknown product",
                    BrandName = variant.Product?.Brand?.BrandName ?? "-",
                    CategoryName = variant.Product?.Category?.CategoryName ?? "-",
                    Volume = variant.Volume ?? "-",
                    ImagePath = variant.ImagePath ?? string.Empty,
                    StockQuantity = variant.StockQuantity ?? 0,
                    IsActive = variant.IsActive == true
                });
            }

            ImportHistory.Clear();
            foreach (var importOrder in importOrders)
            {
                var details = importOrder.ImportOrderDetails
                    .OrderBy(detail => detail.ImportDetailId)
                    .Select(detail => new ImportHistoryDetailItem
                    {
                        ProductName = detail.Variant?.Product?.ProductName ?? "Unknown product",
                        Volume = detail.Variant?.Volume ?? "-",
                        Quantity = detail.Quantity ?? 0,
                        ImportPrice = detail.ImportPrice ?? 0m
                    })
                    .ToList();

                ImportHistory.Add(new ImportHistoryItem
                {
                    ImportId = importOrder.ImportId,
                    ImportDate = importOrder.ImportDate ?? DateTime.Now,
                    CreatedBy = importOrder.User?.FullName ?? importOrder.User?.Username ?? $"User #{importOrder.UserId}",
                    TotalItems = details.Sum(detail => detail.Quantity),
                    TotalCost = importOrder.TotalCost ?? 0m,
                    Details = details
                });
            }

            SelectedVariant = selectedVariantId.HasValue
                ? AvailableVariants.FirstOrDefault(item => item.VariantId == selectedVariantId.Value)
                : AvailableVariants.FirstOrDefault();
            SelectedHistoryItem = selectedImportId.HasValue
                ? ImportHistory.FirstOrDefault(item => item.ImportId == selectedImportId.Value)
                : ImportHistory.FirstOrDefault();

            OnPropertyChanged(nameof(TotalVariants));
            UpdateDraftGuidance();
            AvailableVariantsView.Refresh();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unable to load import data.\n{ex.Message}", "Load failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool FilterVariant(object obj)
    {
        if (obj is not ImportVariantItem item)
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
            || item.CategoryName.ToLowerInvariant().Contains(keyword)
            || item.Volume.ToLowerInvariant().Contains(keyword);
    }

    private bool CanAddToDraft() => SelectedVariant != null && ImportQuantity > 0 && ImportPrice > 0;

    private void AddToDraft()
    {
        if (SelectedVariant == null)
        {
            StatusMessage = "Select one product variant before adding to the import draft.";
            return;
        }

        if (ImportQuantity <= 0)
        {
            StatusMessage = "Import quantity must be greater than zero.";
            return;
        }

        if (ImportPrice <= 0)
        {
            StatusMessage = "Import price must be greater than zero.";
            return;
        }

        var existingLine = DraftLines.FirstOrDefault(item => item.VariantId == SelectedVariant.VariantId);
        if (existingLine != null)
        {
            existingLine.Quantity += ImportQuantity;
            existingLine.ImportPrice = ImportPrice;
        }
        else
        {
            DraftLines.Add(new ImportDraftLineItem
            {
                VariantId = SelectedVariant.VariantId,
                ProductName = SelectedVariant.ProductName,
                BrandName = SelectedVariant.BrandName,
                Volume = SelectedVariant.Volume,
                CurrentStock = SelectedVariant.StockQuantity,
                Quantity = ImportQuantity,
                ImportPrice = ImportPrice
            });
        }

        ResetEntryFields();
        StatusMessage = "The variant was added to the current import draft.";
    }

    private void RemoveDraftLine()
    {
        if (SelectedDraftLine == null) return;
        DraftLines.Remove(SelectedDraftLine);
        SelectedDraftLine = null;
        StatusMessage = "The selected draft line was removed.";
    }

    private void ClearDraft()
    {
        DraftLines.Clear();
        SelectedDraftLine = null;
        ResetEntryFields();
        StatusMessage = "The current import draft was cleared.";
    }

    private void SaveImport()
    {
        var currentUser = UserSession.CurrentUser;
        if (currentUser == null)
        {
            MessageBox.Show("The manager session is not available. Please log in again.", "Session expired", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (DraftLines.Count == 0)
        {
            StatusMessage = "Add at least one line before saving the import order.";
            return;
        }

        try
        {
            using var context = new AppDbContext();
            using var transaction = context.Database.BeginTransaction();

            var variantIds = DraftLines.Select(item => item.VariantId).ToList();
            var variants = context.ProductVariants.Where(variant => variantIds.Contains(variant.VariantId)).ToDictionary(variant => variant.VariantId);

            foreach (var line in DraftLines)
            {
                if (!variants.ContainsKey(line.VariantId))
                {
                    transaction.Rollback();
                    MessageBox.Show($"Variant for {line.ProductName} {line.Volume} could not be found.", "Import blocked", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            var importOrder = new Models.ImportOrder
            {
                UserId = currentUser.UserId,
                ImportDate = DateTime.Now,
                TotalCost = DraftLines.Sum(item => item.LineTotal)
            };

            context.ImportOrders.Add(importOrder);
            context.SaveChanges();

            foreach (var line in DraftLines)
            {
                var variant = variants[line.VariantId];
                variant.StockQuantity = (variant.StockQuantity ?? 0) + line.Quantity;

                context.ImportOrderDetails.Add(new Models.ImportOrderDetail
                {
                    ImportId = importOrder.ImportId,
                    VariantId = line.VariantId,
                    Quantity = line.Quantity,
                    ImportPrice = line.ImportPrice
                });
            }

            context.SaveChanges();
            transaction.Commit();

            DraftLines.Clear();
            SelectedDraftLine = null;
            ResetEntryFields();
        StatusMessage = $"Import order #{importOrder.ImportId} was created successfully.";
        LoadData();
    }
        catch (Exception ex)
        {
            MessageBox.Show($"Unable to save the import order.\n{ex.Message}", "Save failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ResetEntryFields()
    {
        ImportQuantity = 1;
        ImportPrice = 0m;
    }

    private void UpdateDraftGuidance()
    {
        if (SelectedVariant == null)
        {
            StatusMessage = "Select one product variant, then enter quantity and import price.";
            return;
        }

        if (ImportQuantity <= 0)
        {
            StatusMessage = "Import quantity must be greater than zero before you can add a draft line.";
            return;
        }

        if (ImportPrice <= 0)
        {
            StatusMessage = "Import price must be greater than zero. After Add Line, click Save Import Order to update stock and history.";
            return;
        }

        StatusMessage = "Ready to add this line into the draft. Stock and history will update after Save Import Order.";
    }
}
