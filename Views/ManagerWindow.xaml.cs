using System.Windows;
using CosmeticStoreManagement.Helpers;
using CosmeticStoreManagement.Views.Admin;

namespace CosmeticStoreManagement.Views;

public partial class ManagerWindow : Window
{
    public ManagerWindow()
    {
        InitializeComponent();
        frManager.Content = new ViewStatisticsPage();
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        UserSession.Clear();
        new LoginWindow().Show();
        Close();
    }

    private void ViewStatistics_Click(object sender, RoutedEventArgs e)
    {
        frManager.Content = new ViewStatisticsPage();
    }

    private void ManageBrand_Click(object sender, RoutedEventArgs e)
    {
        frManager.Content = new ManageBrandPage();
    }

    private void ManageCategory_Click(object sender, RoutedEventArgs e)
    {
        frManager.Content = new ManageCategoryPage();
    }

    private void ManageProduct_Click(object sender, RoutedEventArgs e)
    {
        frManager.Content = new ManageProductPage();
    }

    private void ManageVoucher_Click(object sender, RoutedEventArgs e)
    {
        frManager.Content = new ManageVoucherPage();
    }

    private void ManageImportProduct_Click(object sender, RoutedEventArgs e)
    {
        frManager.Content = new ManageImportProductPage();
    }

    private void ManageStaff_Click(object sender, RoutedEventArgs e)
    {
        frManager.Content = new ManageStaffPage();
    }
}
