using System.Windows.Controls;
using CosmeticStoreManagement.ViewModels.Staff;

namespace CosmeticStoreManagement.Views.Staff;

public partial class StaffCustomerPage : Page
{
    public StaffCustomerPage()
    {
        InitializeComponent();
        DataContext = new StaffCustomerPageVM();
    }
}
