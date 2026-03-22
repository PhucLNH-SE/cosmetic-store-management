using System.Windows.Controls;
using CosmeticStoreManagement.ViewModels.Staff;

namespace CosmeticStoreManagement.Views.Staff;

public partial class StaffOrdersPage : Page
{
    public StaffOrdersPage()
    {
        InitializeComponent();
        DataContext = new StaffOrdersPageVM();
    }

    public void RefreshOrders()
    {
        if (DataContext is StaffOrdersPageVM vm)
        {
            vm.LoadOrders();
        }
    }
}
