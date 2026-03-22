using System.Windows.Controls;
using CosmeticStoreManagement.ViewModels.Staff;

namespace CosmeticStoreManagement.Views.Staff;

public partial class StaffSalesPage : Page
{
    public StaffSalesPage()
    {
        InitializeComponent();
        DataContext = new StaffSalesPageVM();
    }

    public void RefreshData()
    {
        if (DataContext is StaffSalesPageVM vm)
        {
            vm.LoadData();
        }
    }

    private void ClearFilters_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is StaffSalesPageVM vm)
        {
            vm.ClearFilters();
        }
    }
}
