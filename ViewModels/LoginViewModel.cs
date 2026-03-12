using System.Windows.Controls;
using System.Windows.Input;
using CosmeticStoreManagement.Helpers;
using CosmeticStoreManagement.Services;

namespace CosmeticStoreManagement.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    private string _username = string.Empty;
    private string _errorMessage = string.Empty;

    public LoginViewModel(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
        LoginCommand = new RelayCommand(ExecuteLogin);
    }

    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoginCommand { get; }

    private void ExecuteLogin(object parameter)
    {
        if (parameter is not PasswordBox passwordBox)
        {
            ErrorMessage = "Password control is not available.";
            return;
        }

        var result = _authService.LoginAdmin(Username.Trim(), passwordBox.Password);
        if (!result.IsSuccess || result.User == null)
        {
            ErrorMessage = result.Message;
            return;
        }

        ErrorMessage = string.Empty;
        passwordBox.Clear();
        _navigationService.NavigateToAdmin(result.User);
    }
}
