using System.Windows.Controls;
using CosmeticStoreManagement.ViewModels.Admin;

namespace CosmeticStoreManagement.Views.Admin;

public partial class ManageImportProductPage : Page
{
    public ManageImportProductPage()
    {
        InitializeComponent();
        DataContext = new ManageImportProductPageVM();
    }
}
