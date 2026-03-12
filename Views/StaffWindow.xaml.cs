using System.Windows;
using CosmeticStoreManagement.Helpers;
using CosmeticStoreManagement.Views.Pages;

namespace CosmeticStoreManagement.Views;

public partial class StaffWindow : Window
{
    public StaffWindow()
    {
        InitializeComponent();
        // Load Products page by default
        mainFrame.Navigate(new ProductPage());
    }

    private void Products_Click(object sender, RoutedEventArgs e)
    {
        mainFrame.Navigate(new ProductPage());
    }

    private void Cart_Click(object sender, RoutedEventArgs e)
    {
        // Placeholder - will implement later
        MessageBox.Show("Cart page coming soon!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Customer_Click(object sender, RoutedEventArgs e)
    {
        // Placeholder - will implement later
        MessageBox.Show("Customer page coming soon!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Voucher_Click(object sender, RoutedEventArgs e)
    {
        // Placeholder - will implement later
        MessageBox.Show("Voucher page coming soon!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Payment_Click(object sender, RoutedEventArgs e)
    {
        // Placeholder - will implement later
        MessageBox.Show("Payment page coming soon!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Order_Click(object sender, RoutedEventArgs e)
    {
        // Placeholder - will implement later
        MessageBox.Show("Order page coming soon!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        UserSession.Clear();
        var loginWindow = new LoginWindow();
        WindowStateHelper.ApplyFrom(this, loginWindow);
        loginWindow.Show();
        Close();
    }
}
