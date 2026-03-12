using System.Windows.Controls;
using CosmeticStoreManagement.ViewModels.Admin;

namespace CosmeticStoreManagement.Views.Admin;

public partial class ManageStaffPage : Page
{
    public ManageStaffPage()
    {
        InitializeComponent();
        DataContext = new ManageStaffPageVM();
    }
}
