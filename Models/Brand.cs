using System;
using System.Collections.Generic;

namespace CosmeticStoreManagement.Models;

public partial class Brand : ICloneable
{
    public int BrandId { get; set; }

    public string BrandName { get; set; } = null!;

    public string? Country { get; set; }

    public bool? Status { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public object Clone()
    {
        return new Brand
        {
            BrandId = this.BrandId,
            BrandName = this.BrandName,
            Country = this.Country,
            Status = this.Status
        };
    }
}
