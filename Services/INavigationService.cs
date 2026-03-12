using System;
using CosmeticStoreManagement.Models;
using CosmeticStoreManagement.ViewModels;

namespace CosmeticStoreManagement.Services;

public interface INavigationService
{
    BaseViewModel? CurrentViewModel { get; }

    event Action<BaseViewModel>? CurrentViewModelChanged;

    void NavigateToLogin();

    void NavigateToAdmin(User user);
}
