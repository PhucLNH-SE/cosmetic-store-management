using System;
using System.Collections.Generic;

namespace CosmeticStoreManagement.Models;

public partial class Order : ICloneable
{
    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public int UserId { get; set; }

    public int? VoucherId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? FinalAmount { get; set; }

    public string? Status { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual User User { get; set; } = null!;

    public virtual Voucher? Voucher { get; set; }

    public object Clone()
    {
        return new Order
        {
            OrderId = this.OrderId,
            CustomerId = this.CustomerId,
            UserId = this.UserId,
            VoucherId = this.VoucherId,
            OrderDate = this.OrderDate,
            TotalAmount = this.TotalAmount,
            FinalAmount = this.FinalAmount,
            Status = this.Status,
            Rating = this.Rating,
            Comment = this.Comment
        };
    }
}
