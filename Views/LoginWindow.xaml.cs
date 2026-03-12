using System.Windows;
using CosmeticStoreManagement.ViewModels;

namespace CosmeticStoreManagement.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        DataContext = new LoginWindowVM();
    }
}
