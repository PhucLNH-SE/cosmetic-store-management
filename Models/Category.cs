using System;
using System.Collections.Generic;

namespace CosmeticStoreManagement.Models;

public partial class Category : ICloneable
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public bool? Status { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public object Clone()
    {
        return new Category
        {
            CategoryId = this.CategoryId,
            CategoryName = this.CategoryName,
            Status = this.Status
        };
    }
}
