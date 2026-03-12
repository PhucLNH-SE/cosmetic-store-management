using System.Windows;

namespace CosmeticStoreManagement.Helpers;

public static class WindowStateHelper
{
    public static void ApplyFrom(Window sourceWindow, Window targetWindow)
    {
        if (sourceWindow.WindowState == WindowState.Maximized)
        {
            targetWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            targetWindow.WindowState = WindowState.Maximized;
            return;
        }

        targetWindow.WindowStartupLocation = WindowStartupLocation.Manual;
        targetWindow.Left = sourceWindow.Left;
        targetWindow.Top = sourceWindow.Top;
        targetWindow.Width = sourceWindow.Width;
        targetWindow.Height = sourceWindow.Height;
        targetWindow.WindowState = sourceWindow.WindowState;
    }
}
