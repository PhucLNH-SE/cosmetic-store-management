using System.Windows.Controls;
using CosmeticStoreManagement.ViewModels;

namespace CosmeticStoreManagement.Views.Pages;

public partial class StaffCustomerPage : Page
{
    public StaffCustomerPage()
    {
        InitializeComponent();
        DataContext = new StaffCustomerPageVM();
    }
}
