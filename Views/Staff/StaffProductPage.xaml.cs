using System.Windows.Controls;
using CosmeticStoreManagement.ViewModels.Staff;

namespace CosmeticStoreManagement.Views.Staff;

public partial class StaffProductPage : Page
{
    public StaffProductPage()
    {
        InitializeComponent();
        DataContext = new StaffProductPageVM();
    }

    public void RefreshData()
    {
        if (DataContext is StaffProductPageVM vm)
        {
            vm.LoadData();
        }
    }
}
