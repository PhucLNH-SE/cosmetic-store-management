using CosmeticStoreManagement.Services;

namespace CosmeticStoreManagement.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private BaseViewModel? _currentViewModel;

    public MainWindowViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        _navigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;
        _navigationService.NavigateToLogin();
    }

    public BaseViewModel? CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            _currentViewModel = value;
            OnPropertyChanged();
        }
    }

    private void OnCurrentViewModelChanged(BaseViewModel viewModel)
    {
        CurrentViewModel = viewModel;
    }
}
