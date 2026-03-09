using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CosmeticStoreManagement.Helpers

{
    class BaseViewModel
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
