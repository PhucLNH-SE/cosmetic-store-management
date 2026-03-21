using System.Windows.Controls;
using CosmeticStoreManagement.ViewModels;

namespace CosmeticStoreManagement.Views.Pages;

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
