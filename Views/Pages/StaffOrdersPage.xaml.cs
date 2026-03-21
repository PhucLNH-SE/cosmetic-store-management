using System.Windows.Controls;
using CosmeticStoreManagement.ViewModels;

namespace CosmeticStoreManagement.Views.Pages;

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
