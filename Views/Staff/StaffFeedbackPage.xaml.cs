using System.Windows.Controls;
using CosmeticStoreManagement.ViewModels.Staff;

namespace CosmeticStoreManagement.Views.Staff;

public partial class StaffFeedbackPage : Page
{
    public StaffFeedbackPage()
    {
        InitializeComponent();
        DataContext = new StaffFeedbackPageVM();
    }

    public void RefreshOrders()
    {
        if (DataContext is StaffFeedbackPageVM vm)
        {
            vm.LoadOrders();
        }
    }
}
