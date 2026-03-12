using CosmeticStoreManagement.Models;

namespace CosmeticStoreManagement.Helpers;

public static class UserSession
{
    public static User? CurrentUser { get; set; }

    public static bool IsManager => CurrentUser?.Role == "Manager";

    public static bool IsStaff => CurrentUser?.Role == "Staff";

    public static void Clear()
    {
        CurrentUser = null;
    }
}
