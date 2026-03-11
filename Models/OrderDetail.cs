using System;
using System.Collections.Generic;

namespace CosmeticStoreManagement.Models;

public partial class OrderDetail : ICloneable
{
    public int OrderDetailId { get; set; }

    public int OrderId { get; set; }

    public int VariantId { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? ImportPrice { get; set; }

    public decimal? Subtotal { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ProductVariant Variant { get; set; } = null!;

    public object Clone()
    {
        return new OrderDetail
        {
            OrderDetailId = this.OrderDetailId,
            OrderId = this.OrderId,
            VariantId = this.VariantId,
            Quantity = this.Quantity,
            UnitPrice = this.UnitPrice,
            ImportPrice = this.ImportPrice,
            Subtotal = this.Subtotal
        };
    }
}
