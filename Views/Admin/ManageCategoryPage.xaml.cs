using System.Windows.Controls;
using CosmeticStoreManagement.ViewModels;

namespace CosmeticStoreManagement.Views.Admin;

public partial class ManageCategoryPage : Page
{
    public ManageCategoryPage()
    {
        InitializeComponent();
        DataContext = new ManageCategoryPageVM();
    }

    private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {

    }
}
