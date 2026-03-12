using System;
using System.Linq;
using CosmeticStoreManagement.Data;
using CosmeticStoreManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CosmeticStoreManagement.Services;

public class OrderCheckoutService : IOrderCheckoutService
{
    public CheckoutResult Checkout(CheckoutRequest request)
    {
        if (request.UserId <= 0)
        {
            return CheckoutResult.Fail("Invalid user account.");
        }

        if (request.Lines == null || request.Lines.Count == 0)
        {
            return CheckoutResult.Fail("Order must contain at least one product.");
        }

        var phone = request.CustomerPhone.Trim();
        if (string.IsNullOrWhiteSpace(phone))
        {
            return CheckoutResult.Fail("Customer phone is required.");
        }

        using var context = new AppDbContext();
        using var transaction = context.Database.BeginTransaction();

        try
        {
            var user = context.Users.FirstOrDefault(u => u.UserId == request.UserId);
            if (user == null)
            {
                return CheckoutResult.Fail("Current user does not exist.");
            }

            var customer = context.Customers.FirstOrDefault(c => c.Phone == phone);
            if (customer == null)
            {
                customer = new Customer
                {
                    CustomerName = string.IsNullOrWhiteSpace(request.CustomerName) ? "Guest" : request.CustomerName.Trim(),
                    Phone = phone
                };
                context.Customers.Add(customer);
                context.SaveChanges();
            }
            else if (!string.IsNullOrWhiteSpace(request.CustomerName) && string.IsNullOrWhiteSpace(customer.CustomerName))
            {
                customer.CustomerName = request.CustomerName.Trim();
                context.SaveChanges();
            }

            var variantIds = request.Lines.Select(line => line.VariantId).Distinct().ToList();
            var variants = context.ProductVariants
                .Where(variant => variantIds.Contains(variant.VariantId))
                .ToDictionary(variant => variant.VariantId);

            foreach (var line in request.Lines)
            {
                if (!variants.TryGetValue(line.VariantId, out var variant))
                {
                    return CheckoutResult.Fail("A product variant no longer exists.");
                }

                var stock = variant.StockQuantity ?? 0;
                if (line.Quantity <= 0 || line.Quantity > stock)
                {
                    return CheckoutResult.Fail($"Insufficient stock for variant {variant.Sku ?? variant.VariantId.ToString()}.");
                }
            }

            var totalAmount = request.Lines.Sum(line => line.UnitPrice * line.Quantity);
            var order = new Order
            {
                CustomerId = customer.CustomerId,
                UserId = user.UserId,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                FinalAmount = totalAmount,
                Status = "Pending",
                Comment = "Created from admin dashboard"
            };

            context.Orders.Add(order);
            context.SaveChanges();

            foreach (var line in request.Lines)
            {
                var variant = variants[line.VariantId];
                var subtotal = line.UnitPrice * line.Quantity;

                context.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.OrderId,
                    VariantId = line.VariantId,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    ImportPrice = null,
                    Subtotal = subtotal
                });

                variant.StockQuantity = (variant.StockQuantity ?? 0) - line.Quantity;
            }

            context.SaveChanges();
            transaction.Commit();
            return CheckoutResult.Success(order.OrderId);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return CheckoutResult.Fail($"Checkout failed: {ex.Message}");
        }
    }
}
