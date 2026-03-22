using System.Windows;
using CosmeticStoreManagement.Helpers;
using CosmeticStoreManagement.Views;


namespace CosmeticStoreManagement.Views.Staff;

public partial class StaffWindow : Window
{
    private StaffSalesPage? _salesPage;
    private StaffProductPage? _productPage;
    private StaffCustomerPage? _customerPage;
    private StaffOrdersPage? _ordersPage;
    private StaffFeedbackPage? _feedbackPage;
    private StaffVoucherPage? _voucherPage;

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
        OpenProductPage();
    }

    private void Feedback_Click(object sender, RoutedEventArgs e)
    {
        OpenFeedbackPage();
    }

    private void Customer_Click(object sender, RoutedEventArgs e)
    {
        OpenCustomerPage();
    }

    private void Voucher_Click(object sender, RoutedEventArgs e)
    {
        OpenVoucherPage();
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
            _salesPage.RefreshData();

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

    private void OpenProductPage()
    {
        try
        {
            _productPage ??= new StaffProductPage();
            _productPage.RefreshData();

            if (!ReferenceEquals(mainFrame.Content, _productPage))
            {
                mainFrame.Navigate(_productPage);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Unable to open the product page.\n{ex.Message}",
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
        _ordersPage ??= new StaffOrdersPage();
        _ordersPage.RefreshOrders();
        if (!ReferenceEquals(mainFrame.Content, _ordersPage))
        {
            mainFrame.Navigate(_ordersPage);
        }
    }

    private void OpenFeedbackPage()
    {
        try
        {
            _feedbackPage ??= new StaffFeedbackPage();
            _feedbackPage.RefreshOrders();

            if (!ReferenceEquals(mainFrame.Content, _feedbackPage))
            {
                mainFrame.Navigate(_feedbackPage);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Unable to open the feedback page.\n{ex.Message}",
                "Staff page error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OpenVoucherPage()
    {
        try
        {
            _voucherPage ??= new StaffVoucherPage();
            _voucherPage.RefreshData();

            if (!ReferenceEquals(mainFrame.Content, _voucherPage))
            {
                mainFrame.Navigate(_voucherPage);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Unable to open the voucher page.\n{ex.Message}",
                "Staff page error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
