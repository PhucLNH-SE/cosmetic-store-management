using CosmeticStoreManagement.ViewModels.Admin;
using System.Windows.Controls;

namespace CosmeticStoreManagement.Views.Admin
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
