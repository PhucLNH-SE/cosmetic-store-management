using System;
using System.Collections.Generic;

namespace CosmeticStoreManagement.Models;

public partial class CartItem : ICloneable
{
    public int CartItemId { get; set; }

    public int CartId { get; set; }

    public int VariantId { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual ProductVariant Variant { get; set; } = null!;

    public object Clone()
    {
        return new CartItem
        {
            CartItemId = this.CartItemId,
            CartId = this.CartId,
            VariantId = this.VariantId,
            Quantity = this.Quantity,
            UnitPrice = this.UnitPrice
        };
    }
}
