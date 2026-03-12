using System;
using CosmeticStoreManagement.Models;
using CosmeticStoreManagement.ViewModels;

namespace CosmeticStoreManagement.Services;

public class NavigationService : INavigationService
{
    private readonly Func<LoginViewModel> _createLoginViewModel;
    private readonly Func<User, AdminHomeViewModel> _createAdminViewModel;

    public NavigationService(
        Func<LoginViewModel> createLoginViewModel,
        Func<User, AdminHomeViewModel> createAdminViewModel)
    {
        _createLoginViewModel = createLoginViewModel;
        _createAdminViewModel = createAdminViewModel;
    }

    public BaseViewModel? CurrentViewModel { get; private set; }

    public event Action<BaseViewModel>? CurrentViewModelChanged;

    public void NavigateToLogin()
    {
        SetCurrentViewModel(_createLoginViewModel());
    }

    public void NavigateToAdmin(User user)
    {
        SetCurrentViewModel(_createAdminViewModel(user));
    }

    private void SetCurrentViewModel(BaseViewModel viewModel)
    {
        CurrentViewModel = viewModel;
        CurrentViewModelChanged?.Invoke(viewModel);
    }
}
