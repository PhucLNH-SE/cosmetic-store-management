using System;
using System.Collections.Generic;

namespace CosmeticStoreManagement.Models;

public partial class ImportOrderDetail : ICloneable
{
    public int ImportDetailId { get; set; }

    public int ImportId { get; set; }

    public int VariantId { get; set; }

    public int? Quantity { get; set; }

    public decimal? ImportPrice { get; set; }

    public virtual ImportOrder Import { get; set; } = null!;

    public virtual ProductVariant Variant { get; set; } = null!;

    public object Clone()
    {
        return new ImportOrderDetail
        {
            ImportDetailId = this.ImportDetailId,
            ImportId = this.ImportId,
            VariantId = this.VariantId,
            Quantity = this.Quantity,
            ImportPrice = this.ImportPrice
        };
    }
}
