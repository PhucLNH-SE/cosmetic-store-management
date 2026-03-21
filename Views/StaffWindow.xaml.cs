using System.Windows;
using CosmeticStoreManagement.Helpers;
using CosmeticStoreManagement.Views.Pages;

namespace CosmeticStoreManagement.Views;

public partial class StaffWindow : Window
{
    private StaffSalesPage? _salesPage;
    private StaffCustomerPage? _customerPage;

    public StaffWindow()
    {
        InitializeComponent();
        Loaded += StaffWindow_Loaded;
    }

    private void StaffWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= StaffWindow_Loaded;
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
        OpenCustomerPage();
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
        try
        {
            _salesPage ??= new StaffSalesPage();

            if (!ReferenceEquals(mainFrame.Content, _salesPage))
            {
                mainFrame.Navigate(_salesPage);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Unable to open the sales page.\n{ex.Message}",
                "Staff page error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OpenCustomerPage()
    {
        try
        {
            _customerPage ??= new StaffCustomerPage();

            if (!ReferenceEquals(mainFrame.Content, _customerPage))
            {
                mainFrame.Navigate(_customerPage);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Unable to open the customer page.\n{ex.Message}",
                "Staff page error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
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
