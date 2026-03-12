using System.Windows.Controls;
using CosmeticStoreManagement.ViewModels;

namespace CosmeticStoreManagement.Views.Pages;

public partial class ProductPage : Page
{
    public ProductPage()
    {
        InitializeComponent();
        DataContext = new ProductPageVM();
    }

    private void ClearFilters_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ProductPageVM vm)
        {
            vm.ClearFilters();
        }
    }
}
