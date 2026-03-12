using CosmeticStoreManagement.ViewModels;
using CosmeticStoreManagement.ViewModels.admin;
using System.Windows.Controls;

namespace CosmeticStoreManagement.Views.Admin;

public partial class ManageProductPage : Page
{
    public ManageProductPage()
    {
        InitializeComponent();

        // Dòng này c?c k? quan tr?ng: N?i giao di?n v?i kh?i óc x? lý (ViewModel)
        this.DataContext = new ManageProductPageVM();
    }
}