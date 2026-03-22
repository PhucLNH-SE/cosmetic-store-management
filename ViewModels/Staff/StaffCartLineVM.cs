using CosmeticStoreManagement.Models;

namespace CosmeticStoreManagement.ViewModels.Staff;

public class StaffCartLineVM : BaseViewModel
{
    private int _quantity;

    public StaffCartLineVM(ProductVariant variant, int quantity = 1)
    {
        Variant = variant;
        _quantity = quantity < 1 ? 1 : quantity;
    }

    public ProductVariant Variant { get; }

    public int VariantId => Variant.VariantId;

    public string ProductName => Variant.Product?.ProductName ?? "Unknown product";

    public string Volume => string.IsNullOrWhiteSpace(Variant.Volume) ? "-" : Variant.Volume;

    public string BrandName => Variant.Product?.Brand?.BrandName ?? "Unknown brand";

    public string ImagePath => Variant.ImagePath ?? string.Empty;

    public decimal UnitPrice => Variant.Price ?? 0m;

    public int AvailableStock => Variant.StockQuantity ?? 0;

    public int Quantity
    {
        get => _quantity;
        set
        {
            var normalized = value < 1 ? 1 : value;
            if (_quantity == normalized)
            {
                return;
            }

            _quantity = normalized;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LineTotal));
        }
    }

    public decimal LineTotal => UnitPrice * Quantity;

    public void UpdateAvailableStock(int stock)
    {
        Variant.StockQuantity = stock;
        OnPropertyChanged(nameof(AvailableStock));
    }
}
