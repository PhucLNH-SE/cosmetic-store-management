using System.Collections.Generic;

namespace CosmeticStoreManagement.Services;

public interface IOrderCheckoutService
{
    CheckoutResult Checkout(CheckoutRequest request);
}

public sealed class CheckoutRequest
{
    public int UserId { get; init; }

    public string CustomerPhone { get; init; } = string.Empty;

    public string CustomerName { get; init; } = string.Empty;

    public IReadOnlyList<CheckoutLineRequest> Lines { get; init; } = new List<CheckoutLineRequest>();
}

public sealed class CheckoutLineRequest
{
    public int VariantId { get; init; }

    public int Quantity { get; init; }

    public decimal UnitPrice { get; init; }
}

public sealed class CheckoutResult
{
    private CheckoutResult(bool isSuccess, string message, int orderId)
    {
        IsSuccess = isSuccess;
        Message = message;
        OrderId = orderId;
    }

    public bool IsSuccess { get; }

    public string Message { get; }

    public int OrderId { get; }

    public static CheckoutResult Success(int orderId)
    {
        return new CheckoutResult(true, string.Empty, orderId);
    }

    public static CheckoutResult Fail(string message)
    {
        return new CheckoutResult(false, message, 0);
    }
}
