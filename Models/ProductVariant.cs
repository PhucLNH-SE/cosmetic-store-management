using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CosmeticStoreManagement.Models;

public partial class ProductVariant : ICloneable
{
    public int VariantId { get; set; }

    public int ProductId { get; set; }

    public string? Volume { get; set; }

    public decimal? Price { get; set; }

    public int? StockQuantity { get; set; }

    public string? Sku { get; set; }

    [NotMapped]
    public string? ImagePath
    {
        get => Product?.ImagePath;
        set
        {
            if (Product != null)
            {
                Product.ImagePath = value;
            }
        }
    }

    public bool? IsActive { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<ImportOrderDetail> ImportOrderDetails { get; set; } = new List<ImportOrderDetail>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Product Product { get; set; } = null!;

    public object Clone()
    {
        return new ProductVariant
        {
            VariantId = this.VariantId,
            ProductId = this.ProductId,
            Volume = this.Volume,
            Price = this.Price,
            StockQuantity = this.StockQuantity,
            Sku = this.Sku,
            IsActive = this.IsActive
        };
    }
}
