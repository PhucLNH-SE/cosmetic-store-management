using System.Windows.Controls;
using CosmeticStoreManagement.ViewModels;

namespace CosmeticStoreManagement.Views.Admin;

public partial class ManageBrandPage : Page
{
    public ManageBrandPage()
    {
        InitializeComponent();
        DataContext = new ManageBrandPageVM();
    }

    private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {

    }
}
