using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CosmeticStoreManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace CosmeticStoreManagement.Services;

public class AdminCatalogService : IAdminCatalogService
{
    private const string DefaultImageFolder = @"D:\PRN121\hinhPRN";

    public IReadOnlyList<CatalogVariantDto> GetActiveCatalog()
    {
        using var context = new AppDbContext();

        var rows = (from variant in context.ProductVariants.AsNoTracking()
                    join product in context.Products.AsNoTracking() on variant.ProductId equals product.ProductId
                    join brand in context.Brands.AsNoTracking() on product.BrandId equals brand.BrandId
                    join category in context.Categories.AsNoTracking() on product.CategoryId equals category.CategoryId
                    where (variant.IsActive ?? true)
                        && (product.IsActive ?? true)
                        && (brand.Status ?? true)
                        && (category.Status ?? true)
                    select new
                    {
                        variant.VariantId,
                        variant.ProductId,
                        product.ProductName,
                        BrandName = brand.BrandName,
                        CategoryName = category.CategoryName,
                        variant.Volume,
                        variant.Sku,
                        variant.Price,
                        variant.StockQuantity,
                        variant.ImagePath
                    })
                    .ToList();

        return rows
            .Select(row => new CatalogVariantDto
            {
                VariantId = row.VariantId,
                ProductId = row.ProductId,
                ProductName = row.ProductName,
                BrandName = row.BrandName,
                CategoryName = row.CategoryName,
                Volume = row.Volume,
                Sku = row.Sku,
                Price = row.Price ?? 0m,
                StockQuantity = row.StockQuantity ?? 0,
                ImagePath = row.ImagePath,
                ImageAbsolutePath = ResolveImagePath(row.ImagePath)
            })
            .OrderBy(item => item.ProductName)
            .ToList();
    }

    public DashboardStatsDto GetDashboardStats()
    {
        using var context = new AppDbContext();
        return new DashboardStatsDto
        {
            Products = context.Products.Count(),
            Categories = context.Categories.Count(),
            Brands = context.Brands.Count(),
            Customers = context.Customers.Count(),
            Orders = context.Orders.Count(),
            ImportOrders = context.ImportOrders.Count(),
            Vouchers = context.Vouchers.Count(),
            Users = context.Users.Count()
        };
    }

    private static string ResolveImagePath(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return string.Empty;
        }

        if (Path.IsPathRooted(imagePath) && File.Exists(imagePath))
        {
            return imagePath;
        }

        var fileName = imagePath.Trim();
        var rootCandidates = new[]
        {
            DefaultImageFolder,
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "ProductImages"),
            Path.Combine(Directory.GetCurrentDirectory(), "Resources", "ProductImages"),
            Path.Combine(Directory.GetCurrentDirectory(), "Resources")
        };

        foreach (var root in rootCandidates)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                continue;
            }

            var candidate = Path.Combine(root, fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return string.Empty;
    }
}
