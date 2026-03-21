using System.Windows.Controls;
using CosmeticStoreManagement.ViewModels;

namespace CosmeticStoreManagement.Views.Pages;

public partial class StaffSalesPage : Page
{
    public StaffSalesPage()
    {
        InitializeComponent();
        DataContext = new StaffSalesPageVM();
    }

    private void ClearFilters_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is StaffSalesPageVM vm)
        {
            vm.ClearFilters();
        }
    }
}
