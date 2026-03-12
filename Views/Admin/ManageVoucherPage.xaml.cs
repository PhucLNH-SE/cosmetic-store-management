using CosmeticStoreManagement.ViewModels;
using System.Windows.Controls;

namespace CosmeticStoreManagement.Views.Admin;

public partial class ManageVoucherPage : Page
{
    public ManageVoucherPage()
    {
        InitializeComponent();
        DataContext = new ManageVoucherVM();
    }
}
