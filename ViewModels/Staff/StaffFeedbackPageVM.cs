using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CosmeticStoreManagement.Data;
using CosmeticStoreManagement.Helpers;
using CosmeticStoreManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CosmeticStoreManagement.ViewModels.Staff;

public class StaffFeedbackOrderItem : BaseViewModel
{
    public int OrderId { get; set; }

    public DateTime? OrderDate { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerPhone { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public decimal FinalAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    private int? _rating;
    public int? Rating
    {
        get => _rating;
        set
        {
            if (_rating == value)
            {
                return;
            }

            _rating = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RatingDisplay));
        }
    }

    private string _comment = string.Empty;
    public string Comment
    {
        get => _comment;
        set
        {
            if (_comment == value)
            {
                return;
            }

            _comment = value;
            OnPropertyChanged();
        }
    }

    public string RatingDisplay => Rating.HasValue ? $"{Rating}/5" : "No feedback";
}

public class StaffFeedbackPageVM : BaseViewModel
{
    private readonly IConfiguration _configuration;
    private readonly EmailService _emailService;

    public ObservableCollection<StaffFeedbackOrderItem> Orders { get; } = new();

    public ObservableCollection<int> RatingOptions { get; } = new() { 1, 2, 3, 4, 5 };

    public ICollectionView OrdersView { get; }

    public ICommand RefreshCommand { get; }

    public ICommand SaveFeedbackCommand { get; }

    public ICommand ResetCommand { get; }

    private StaffFeedbackOrderItem? _selectedOrder;
    public StaffFeedbackOrderItem? SelectedOrder
    {
        get => _selectedOrder;
        set
        {
            if (_selectedOrder == value)
            {
                return;
            }

            _selectedOrder = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelectedOrder));
            LoadEditorFromSelection();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value)
            {
                return;
            }

            _searchText = value;
            OnPropertyChanged();
            OrdersView.Refresh();
        }
    }

    private int? _selectedRating;
    public int? SelectedRating
    {
        get => _selectedRating;
        set
        {
            if (_selectedRating == value)
            {
                return;
            }

            _selectedRating = value;
            OnPropertyChanged();
        }
    }

    private string _feedbackComment = string.Empty;
    public string FeedbackComment
    {
        get => _feedbackComment;
        set
        {
            if (_feedbackComment == value)
            {
                return;
            }

            _feedbackComment = value;
            OnPropertyChanged();
        }
    }

    private string _statusMessage = "Select a completed order to submit customer feedback.";
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage == value)
            {
                return;
            }

            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public bool HasSelectedOrder => SelectedOrder != null;

    private bool _isSavingFeedback;
    public bool IsSavingFeedback
    {
        get => _isSavingFeedback;
        set
        {
            if (_isSavingFeedback == value)
            {
                return;
            }

            _isSavingFeedback = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public StaffFeedbackPageVM()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        _emailService = new EmailService(_configuration);

        OrdersView = CollectionViewSource.GetDefaultView(Orders);
        OrdersView.Filter = FilterPredicate;

        RefreshCommand = new RelayCommand(_ => LoadOrders());
        SaveFeedbackCommand = new RelayCommand(async _ => await SaveFeedbackAsync(), _ => SelectedOrder != null && !IsSavingFeedback);
        ResetCommand = new RelayCommand(_ => LoadEditorFromSelection(), _ => SelectedOrder != null && !IsSavingFeedback);

        LoadOrders();
    }

    public async void LoadOrders()
    {
        var currentUser = UserSession.CurrentUser;
        if (currentUser == null)
        {
            MessageBox.Show(
                "The staff session is not available. Please log in again.",
                "Session expired",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var previouslySelectedOrderId = SelectedOrder?.OrderId;

        try
        {
            using var context = new AppDbContext();

            var orders = await context.Orders
                .AsNoTracking()
                .Include(order => order.Customer)
                .Where(order =>
                    order.UserId == currentUser.UserId &&
                    order.Status == "Completed")
                .OrderByDescending(order => order.OrderDate)
                .ThenByDescending(order => order.OrderId)
                .ToListAsync();

            Orders.Clear();
            foreach (var order in orders)
            {
                Orders.Add(new StaffFeedbackOrderItem
                {
                    OrderId = order.OrderId,
                    OrderDate = order.OrderDate,
                    CustomerName = order.Customer?.CustomerName ?? "Unknown customer",
                    CustomerPhone = order.Customer?.Phone ?? string.Empty,
                    CustomerEmail = order.Customer?.Email ?? string.Empty,
                    FinalAmount = order.FinalAmount ?? 0m,
                    Status = order.Status ?? string.Empty,
                    Rating = order.Rating,
                    Comment = order.Comment ?? string.Empty
                });
            }

            SelectedOrder = previouslySelectedOrderId.HasValue
                ? Orders.FirstOrDefault(order => order.OrderId == previouslySelectedOrderId.Value)
                : Orders.FirstOrDefault();

            OrdersView.Refresh();

            if (Orders.Count == 0)
            {
                StatusMessage = "No completed orders found for this staff account yet.";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Unable to load feedback orders.\n{ex.Message}",
                "Load failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private bool FilterPredicate(object obj)
    {
        if (obj is not StaffFeedbackOrderItem order)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        var keyword = SearchText.Trim().ToLowerInvariant();
        return order.OrderId.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase)
            || order.CustomerName.ToLowerInvariant().Contains(keyword)
            || order.CustomerPhone.ToLowerInvariant().Contains(keyword)
            || order.RatingDisplay.ToLowerInvariant().Contains(keyword);
    }

    private void LoadEditorFromSelection()
    {
        SelectedRating = SelectedOrder?.Rating;
        FeedbackComment = SelectedOrder?.Comment ?? string.Empty;
        StatusMessage = SelectedOrder == null
            ? "Select a completed order to submit customer feedback."
            : $"Ready to save feedback for order #{SelectedOrder.OrderId}.";
    }

    private async Task SaveFeedbackAsync()
    {
        if (SelectedOrder == null)
        {
            StatusMessage = "Select an order first.";
            return;
        }

        if (SelectedRating == null || SelectedRating < 1 || SelectedRating > 5)
        {
            StatusMessage = "Choose a rating from 1 to 5 stars.";
            return;
        }

        if (string.IsNullOrWhiteSpace(FeedbackComment))
        {
            StatusMessage = "Enter the customer's comment before saving.";
            return;
        }

        var currentUser = UserSession.CurrentUser;
        if (currentUser == null)
        {
            StatusMessage = "The staff session is not available. Please log in again.";
            return;
        }

        try
        {
            IsSavingFeedback = true;
            using var context = new AppDbContext();
            var order = context.Orders
                .Include(order => order.Customer)
                .FirstOrDefault(order =>
                order.OrderId == SelectedOrder.OrderId &&
                order.UserId == currentUser.UserId &&
                order.Status == "Completed");

            if (order == null)
            {
                StatusMessage = "The selected order could not be found.";
                return;
            }

            order.Rating = SelectedRating;
            order.Comment = FeedbackComment.Trim();
            context.SaveChanges();

            SelectedOrder.Rating = order.Rating;
            SelectedOrder.Comment = order.Comment ?? string.Empty;

            if (string.IsNullOrWhiteSpace(order.Customer?.Email))
            {
                StatusMessage = $"Feedback for order #{order.OrderId} was saved, but the customer does not have an email address.";
                return;
            }

            if (!_emailService.IsConfigured)
            {
                StatusMessage = $"Feedback for order #{order.OrderId} was saved, but email settings are missing.";
                return;
            }

            try
            {
                StatusMessage = $"Feedback for order #{order.OrderId} was saved. Sending thank-you email...";

                await _emailService.SendThankYouEmailAsync(
                    order.Customer.Email,
                    order.Customer.CustomerName ?? "Customer",
                    order.OrderId,
                    order.Rating ?? 0,
                    order.Comment ?? string.Empty);

                StatusMessage = $"Feedback for order #{order.OrderId} was saved and a thank-you email was sent.";
            }
            catch (Exception emailEx)
            {
                StatusMessage = $"Feedback for order #{order.OrderId} was saved, but the thank-you email failed: {emailEx.Message}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Unable to save feedback: {ex.Message}";
        }
        finally
        {
            IsSavingFeedback = false;
        }
    }
}
