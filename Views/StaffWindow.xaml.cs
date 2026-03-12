using System.Windows;
using CosmeticStoreManagement.Helpers;

namespace CosmeticStoreManagement.Views;

public partial class StaffWindow : Window
{
    public StaffWindow()
    {
        InitializeComponent();
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        UserSession.Clear();
        new LoginWindow().Show();
        Close();
    }
}
