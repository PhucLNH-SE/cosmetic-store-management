using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using CosmeticStoreManagement.Data;
using CosmeticStoreManagement.Helpers;
using CosmeticStoreManagement.Models;

namespace CosmeticStoreManagement.ViewModels.Staff;

public class StaffCustomerPageVM : BaseViewModel
{
    private ICollectionView _customerView;
    private Customer _textboxitem;
    private Customer? _selecteditem;
    private string _searchtext = string.Empty;
    private string _statusmessage = string.Empty;

    public StaffCustomerPageVM()
    {
        customers = new ObservableCollection<Customer>();
        _customerView = CollectionViewSource.GetDefaultView(customers);
        _textboxitem = new Customer();

        AddCommand = new RelayCommand(Add);
        UpdateCommand = new RelayCommand(Update);
        ClearCommand = new RelayCommand(ClearForm);
        SearchCommand = new RelayCommand(Search);

        Load();
    }

    public ObservableCollection<Customer> customers { get; set; }

    public ICollectionView CustomerView
    {
        get => _customerView;
        set
        {
            _customerView = value;
            OnPropertyChanged(nameof(CustomerView));
        }
    }

    public Customer textboxitem
    {
        get => _textboxitem;
        set
        {
            _textboxitem = value;
            OnPropertyChanged(nameof(textboxitem));
        }
    }

    public Customer? selecteditem
    {
        get => _selecteditem;
        set
        {
            _selecteditem = value;
            OnPropertyChanged(nameof(selecteditem));

            if (_selecteditem != null)
            {
                textboxitem = (Customer)_selecteditem.Clone();
            }
        }
    }

    public string searchtext
    {
        get => _searchtext;
        set
        {
            _searchtext = value;
            OnPropertyChanged(nameof(searchtext));
        }
    }

    public string statusmessage
    {
        get => _statusmessage;
        set
        {
            _statusmessage = value;
            OnPropertyChanged(nameof(statusmessage));
        }
    }

    public ICommand AddCommand { get; }
    public ICommand UpdateCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand SearchCommand { get; }

    private void Load()
    {
        using var context = new AppDbContext();
        var list = context.Customers.OrderBy(customer => customer.CustomerId).ToList();

        customers = new ObservableCollection<Customer>(list);
        OnPropertyChanged(nameof(customers));

        CustomerView = CollectionViewSource.GetDefaultView(customers);
        ApplyFilter();
    }

    private void Add(object? parameter)
    {
        if (!ValidateCustomerInput())
        {
            return;
        }

        var customerName = textboxitem.CustomerName!.Trim();
        var phone = textboxitem.Phone!.Trim();

        using var context = new AppDbContext();
        var duplicated = context.Customers.Any(customer =>
            customer.Phone != null &&
            customer.Phone.ToLower() == phone.ToLower());

        if (duplicated)
        {
            statusmessage = "Phone number already exists.";
            return;
        }

        var newCustomer = new Customer
        {
            CustomerName = customerName,
            Phone = phone,
            Email = NormalizeString(textboxitem.Email),
            Address = NormalizeString(textboxitem.Address)
        };

        context.Customers.Add(newCustomer);
        context.SaveChanges();

        customers.Add(newCustomer);
        ApplyFilter();
        ResetForm();
        statusmessage = "Add customer successfully.";
    }

    private void Update(object? parameter)
    {
        if (selecteditem == null)
        {
            statusmessage = "Select one customer to update.";
            return;
        }

        if (!ValidateCustomerInput())
        {
            return;
        }

        var customerName = textboxitem.CustomerName!.Trim();
        var phone = textboxitem.Phone!.Trim();

        using var context = new AppDbContext();
        var customerInDb = context.Customers.FirstOrDefault(customer => customer.CustomerId == selecteditem.CustomerId);
        if (customerInDb == null)
        {
            statusmessage = "Customer not found.";
            return;
        }

        var duplicated = context.Customers.Any(customer =>
            customer.CustomerId != customerInDb.CustomerId &&
            customer.Phone != null &&
            customer.Phone.ToLower() == phone.ToLower());

        if (duplicated)
        {
            statusmessage = "Phone number already exists.";
            return;
        }

        customerInDb.CustomerName = customerName;
        customerInDb.Phone = phone;
        customerInDb.Email = NormalizeString(textboxitem.Email);
        customerInDb.Address = NormalizeString(textboxitem.Address);

        context.SaveChanges();

        var oldItem = customers.FirstOrDefault(customer => customer.CustomerId == customerInDb.CustomerId);
        if (oldItem != null)
        {
            var index = customers.IndexOf(oldItem);
            customers[index] = (Customer)customerInDb.Clone();
        }

        ApplyFilter();
        ResetForm();
        statusmessage = "Update customer successfully.";
    }

    private void Search(object? parameter)
    {
        ApplyFilter();
    }

    private void ClearForm(object? parameter)
    {
        ResetForm();
        statusmessage = string.Empty;
    }

    private void ApplyFilter()
    {
        var keyword = searchtext?.Trim() ?? string.Empty;

        CustomerView.Filter = item =>
        {
            if (item is not Customer customer)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(keyword))
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(customer.CustomerName)
                   && customer.CustomerName.Contains(keyword, StringComparison.OrdinalIgnoreCase);
        };

        CustomerView.Refresh();
    }

    private bool ValidateCustomerInput()
    {
        if (string.IsNullOrWhiteSpace(textboxitem.CustomerName))
        {
            statusmessage = "Customer name is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(textboxitem.Phone))
        {
            statusmessage = "Phone number is required.";
            return false;
        }

        return true;
    }

    private void ResetForm()
    {
        selecteditem = null;
        textboxitem = new Customer();
    }

    private static string? NormalizeString(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
