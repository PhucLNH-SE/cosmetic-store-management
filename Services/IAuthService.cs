using CosmeticStoreManagement.Models;

namespace CosmeticStoreManagement.Services;

public interface IAuthService
{
    LoginResult LoginAdmin(string username, string password);
}

public sealed class LoginResult
{
    private LoginResult(bool isSuccess, string message, User? user)
    {
        IsSuccess = isSuccess;
        Message = message;
        User = user;
    }

    public bool IsSuccess { get; }

    public string Message { get; }

    public User? User { get; }

    public static LoginResult Success(User user)
    {
        return new LoginResult(true, string.Empty, user);
    }

    public static LoginResult Fail(string message)
    {
        return new LoginResult(false, message, null);
    }
}
