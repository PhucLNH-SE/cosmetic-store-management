using System;
using System.Collections.Generic;

namespace CosmeticStoreManagement.Models;

public partial class User : ICloneable
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Role { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<ImportOrder> ImportOrders { get; set; } = new List<ImportOrder>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public object Clone()
    {
        return new User
        {
            UserId = this.UserId,
            Username = this.Username,
            Password = this.Password,
            FullName = this.FullName,
            Role = this.Role,
            Status = this.Status,
            CreatedDate = this.CreatedDate
        };
    }
}
