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
}
