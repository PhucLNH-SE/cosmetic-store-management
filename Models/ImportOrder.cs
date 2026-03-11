using System;
using System.Collections.Generic;

namespace CosmeticStoreManagement.Models;

public partial class ImportOrder : ICloneable
{
    public int ImportId { get; set; }

    public int UserId { get; set; }

    public DateTime? ImportDate { get; set; }

    public decimal? TotalCost { get; set; }

    public virtual ICollection<ImportOrderDetail> ImportOrderDetails { get; set; } = new List<ImportOrderDetail>();

    public virtual User User { get; set; } = null!;

    public object Clone()
    {
        return new ImportOrder
        {
            ImportId = this.ImportId,
            UserId = this.UserId,
            ImportDate = this.ImportDate,
            TotalCost = this.TotalCost
        };
    }
}
