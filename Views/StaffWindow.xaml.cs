using System.Windows;
using CosmeticStoreManagement.Helpers;
using CosmeticStoreManagement.Views.Pages;

namespace CosmeticStoreManagement.Views;

public partial class StaffWindow : Window
{
    private readonly StaffSalesPage _salesPage;
    private readonly StaffOrdersPage _ordersPage;

    public StaffWindow()
    {
        InitializeComponent();
        _salesPage = new StaffSalesPage();
        _ordersPage = new StaffOrdersPage();
        OpenSalesPage();
    }

    private void Products_Click(object sender, RoutedEventArgs e)
    {
        OpenSalesPage();
    }

    private void Cart_Click(object sender, RoutedEventArgs e)
    {
        OpenSalesPage();
    }

    private void Customer_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Customer page coming soon.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Voucher_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Voucher page coming soon.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Payment_Click(object sender, RoutedEventArgs e)
    {
        OpenSalesPage();
    }

    private void Order_Click(object sender, RoutedEventArgs e)
    {
        OpenOrdersPage();
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        UserSession.Clear();
        var loginWindow = new LoginWindow();
        WindowStateHelper.ApplyFrom(this, loginWindow);
        loginWindow.Show();
        Close();
    }

    private void OpenSalesPage()
    {
        if (!ReferenceEquals(mainFrame.Content, _salesPage))
        {
            mainFrame.Navigate(_salesPage);
        }
    }

    private void OpenOrdersPage()
    {
        _ordersPage.RefreshOrders();
        if (!ReferenceEquals(mainFrame.Content, _ordersPage))
        {
            mainFrame.Navigate(_ordersPage);
        }
    }
}
