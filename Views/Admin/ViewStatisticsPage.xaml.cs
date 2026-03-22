using System.Windows.Controls;
using CosmeticStoreManagement.ViewModels.Admin;

namespace CosmeticStoreManagement.Views.Admin;

public partial class ViewStatisticsPage : Page
{
    public ViewStatisticsPage()
    {
        InitializeComponent();
        DataContext = new ViewStatisticsPageVM();
    }
}
