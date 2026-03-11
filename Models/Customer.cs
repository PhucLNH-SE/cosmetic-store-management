using System;
using System.Collections.Generic;

namespace CosmeticStoreManagement.Models;

public partial class Customer : ICloneable
{
    public int CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public object Clone()
    {
        return new Customer
        {
            CustomerId = this.CustomerId,
            CustomerName = this.CustomerName,
            Phone = this.Phone,
            Email = this.Email,
            Address = this.Address
        };
    }
}
