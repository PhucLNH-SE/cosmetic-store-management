using System.Collections.Generic;

namespace CosmeticStoreManagement.Services;

public interface IAdminCatalogService
{
    IReadOnlyList<CatalogVariantDto> GetActiveCatalog();

    DashboardStatsDto GetDashboardStats();
}

public sealed class CatalogVariantDto
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

    public string? ImagePath { get; init; }

    public string ImageAbsolutePath { get; init; } = string.Empty;
}

public sealed class DashboardStatsDto
{
    public int Products { get; init; }

    public int Categories { get; init; }

    public int Brands { get; init; }

    public int Customers { get; init; }

    public int Orders { get; init; }

    public int ImportOrders { get; init; }

    public int Vouchers { get; init; }

    public int Users { get; init; }
}
