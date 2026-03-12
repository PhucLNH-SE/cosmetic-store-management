using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CosmeticStoreManagement.Data;
using CosmeticStoreManagement.Helpers;
using CosmeticStoreManagement.Models;
using CosmeticStoreManagement.Views;

namespace CosmeticStoreManagement.ViewModels;

public class LoginWindowVM : BaseViewModel
{
    public ICommand LoginCommand { get; }

    public LoginWindowVM()
    {
        LoginCommand = new RelayCommand(Login);
    }

    private string _username = string.Empty;
    public string username
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged(nameof(username));
        }
    }

    private string _errormessage = string.Empty;
    public string errormessage
    {
        get => _errormessage;
        set
        {
            _errormessage = value;
            OnPropertyChanged(nameof(errormessage));
        }
    }

    private void Login(object obj)
    {
        var passwordBox = obj as PasswordBox;
        var password = passwordBox?.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username))
        {
            errormessage = "Username is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            errormessage = "Password is required.";
            return;
        }

        using var context = new AppDbContext();
        User? user = context.Users
            .FirstOrDefault(x => x.Username == username && x.Password == password);

        if (user == null)
        {
            errormessage = "Invalid username or password.";
            return;
        }

        if (user.Status == "Locked")
        {
            errormessage = "This account is locked.";
            return;
        }

        UserSession.CurrentUser = user;
        errormessage = string.Empty;

        Window? nextWindow = user.Role switch
        {
            "Manager" => new ManagerWindow(),
            "Staff" => new StaffWindow(),
            _ => null
        };

        if (nextWindow == null)
        {
            UserSession.Clear();
            errormessage = "Access denied.";
            return;
        }

        nextWindow.Show();

        Window? loginWindow = Application.Current.Windows
            .OfType<LoginWindow>()
            .FirstOrDefault();

        loginWindow?.Close();
    }
}
