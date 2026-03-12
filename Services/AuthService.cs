using System;
using System.Linq;
using CosmeticStoreManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace CosmeticStoreManagement.Services;

public class AuthService : IAuthService
{
    public LoginResult LoginAdmin(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return LoginResult.Fail("Username and password are required.");
        }

        using var context = new AppDbContext();
        var user = context.Users
            .AsNoTracking()
            .FirstOrDefault(u => u.Username == username);

        if (user == null || !string.Equals(user.Password, password, StringComparison.Ordinal))
        {
            return LoginResult.Fail("Invalid username or password.");
        }

        if (string.Equals(user.Status, "Locked", StringComparison.OrdinalIgnoreCase))
        {
            return LoginResult.Fail("Your account is locked.");
        }

        if (!IsAdminRole(user.Role))
        {
            return LoginResult.Fail("This account has no admin permission.");
        }

        return LoginResult.Success(user);
    }

    private static bool IsAdminRole(string? role)
    {
        return string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
    }
}
