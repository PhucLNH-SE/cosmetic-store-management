using System.Windows.Controls;
using CosmeticStoreManagement.ViewModels.Staff;

namespace CosmeticStoreManagement.Views.Staff;

public partial class StaffVoucherPage : Page
{
    public StaffVoucherPage()
    {
        InitializeComponent();
        DataContext = new StaffVoucherPageVM();
    }

    public void RefreshData()
    {
        if (DataContext is StaffVoucherPageVM vm)
        {
            vm.LoadVouchers();
        }
    }
}
