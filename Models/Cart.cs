using System;
using System.Collections.Generic;

namespace CosmeticStoreManagement.Models;

public partial class Cart : ICloneable
{
    public int CartId { get; set; }

    public int UserId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual User User { get; set; } = null!;

    public object Clone()
    {
        return new Cart
        {
            CartId = this.CartId,
            UserId = this.UserId,
            CreatedDate = this.CreatedDate,
            Status = this.Status
        };
    }
}
