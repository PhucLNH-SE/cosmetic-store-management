using System;
using System.Collections.Generic;

namespace CosmeticStoreManagement.Models;

public partial class Voucher : ICloneable
{
    public int VoucherId { get; set; }

    public string VoucherCode { get; set; } = null!;

    public string? DiscountType { get; set; }

    public decimal? DiscountValue { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? Quantity { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public object Clone()
    {
        return new Voucher
        {
            VoucherId = this.VoucherId,
            VoucherCode = this.VoucherCode,
            DiscountType = this.DiscountType,
            DiscountValue = this.DiscountValue,
            StartDate = this.StartDate,
            EndDate = this.EndDate,
            Quantity = this.Quantity,
            IsActive = this.IsActive
        };
    }
}
