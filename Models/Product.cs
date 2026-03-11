using System;
using System.Collections.Generic;

namespace CosmeticStoreManagement.Models;

public partial class Product : ICloneable
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public int BrandId { get; set; }

    public int CategoryId { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public virtual Brand Brand { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    public object Clone()
    {
        return new Product
        {
            ProductId = this.ProductId,
            ProductName = this.ProductName,
            BrandId = this.BrandId,
            CategoryId = this.CategoryId,
            Description = this.Description,
            IsActive = this.IsActive
        };
    }
}
