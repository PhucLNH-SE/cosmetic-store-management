using CosmeticStoreManagement.ViewModels;
using CosmeticStoreManagement.ViewModels.admin;
using System.Windows.Controls;

namespace CosmeticStoreManagement.Views
{
    public partial class ManageProductPage : Page
    {
        public ManageProductPage()
        {
            InitializeComponent();
            this.DataContext = new ManageProductPageVM();
        }
    }
}