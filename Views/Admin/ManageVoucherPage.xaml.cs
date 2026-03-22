using CosmeticStoreManagement.ViewModels.Admin;
using System.Windows.Controls;

namespace CosmeticStoreManagement.Views.Admin;

public partial class ManageVoucherPage : Page
{
    public ManageVoucherPage()
    {
        InitializeComponent();
        DataContext = new ManageVoucherVM();
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }
}
