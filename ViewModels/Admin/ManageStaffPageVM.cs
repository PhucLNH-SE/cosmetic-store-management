using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using CosmeticStoreManagement.Data;
using CosmeticStoreManagement.Helpers;
using CosmeticStoreManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CosmeticStoreManagement.ViewModels.Admin;

public class ManageStaffPageVM : BaseViewModel
{
    public ObservableCollection<User> staffs { get; set; } = new();

    public ICommand AddCommand { get; }
    public ICommand UpdateCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand ClearCommand { get; }

    private ICollectionView _staffView;
    public ICollectionView StaffView => _staffView;

    public ManageStaffPageVM()
    {
        Load();
        _staffView = CollectionViewSource.GetDefaultView(staffs);
        AddCommand = new RelayCommand(Add);
        UpdateCommand = new RelayCommand(Update);
        SearchCommand = new RelayCommand(Search);
        ClearCommand = new RelayCommand(Clear);
        textboxitem = CreateEmptyStaff();
    }

    private User CreateEmptyStaff()
    {
        return new User
        {
            Role = "Staff",
            Status = true
        };
    }

    private void Load()
    {
        using var context = new AppDbContext();
        var items = context.Users
            .Where(x => x.Role == "Staff")
            .OrderBy(x => x.UserId)
            .AsNoTracking()
            .ToList();

        staffs = new ObservableCollection<User>(items);
    }

    private User _textboxitem = null!;
    public User textboxitem
    {
        get => _textboxitem;
        set
        {
            _textboxitem = value;
            OnPropertyChanged(nameof(textboxitem));
        }
    }

    private string _searchtext = string.Empty;
    public string searchtext
    {
        get => _searchtext;
        set
        {
            _searchtext = value;
            OnPropertyChanged(nameof(searchtext));
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

    private User? _selecteditem;
    public User? selecteditem
    {
        get => _selecteditem;
        set
        {
            _selecteditem = value;
            OnPropertyChanged(nameof(selecteditem));

            if (_selecteditem != null)
            {
                textboxitem = (User)_selecteditem.Clone();
            }
        }
    }

    private bool IsValidForm(bool isUpdate)
    {
        if (string.IsNullOrWhiteSpace(textboxitem.Username))
        {
            errormessage = "Username is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(textboxitem.Password))
        {
            errormessage = "Password is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(textboxitem.FullName))
        {
            errormessage = "Full name is required.";
            return false;
        }

        using var context = new AppDbContext();
        bool duplicatedUsername = isUpdate
            ? context.Users.Any(x => x.Username == textboxitem.Username && x.UserId != textboxitem.UserId)
            : context.Users.Any(x => x.Username == textboxitem.Username);

        if (duplicatedUsername)
        {
            errormessage = "Username already exists.";
            return false;
        }

        errormessage = string.Empty;
        return true;
    }

    private void Add(object? obj)
    {
        textboxitem.Role = "Staff";

        if (!IsValidForm(false))
        {
            return;
        }

        using var context = new AppDbContext();
        var newStaff = new User
        {
            Username = textboxitem.Username.Trim(),
            Password = textboxitem.Password,
            FullName = textboxitem.FullName.Trim(),
            Role = "Staff",
            Status = textboxitem.Status ?? true,
            CreatedDate = DateTime.Now
        };

        context.Users.Add(newStaff);
        context.SaveChanges();

        staffs.Add((User)newStaff.Clone());
        Clear(null);
    }

    private void Update(object? obj)
    {
        if (_selecteditem == null)
        {
            errormessage = "Please select a staff account to update.";
            return;
        }

        textboxitem.Role = "Staff";
        textboxitem.UserId = _selecteditem.UserId;
        textboxitem.CreatedDate = _selecteditem.CreatedDate;

        if (!IsValidForm(true))
        {
            return;
        }

        using var context = new AppDbContext();
        var existingStaff = context.Users.FirstOrDefault(x => x.UserId == _selecteditem.UserId && x.Role == "Staff");

        if (existingStaff == null)
        {
            errormessage = "Staff account not found.";
            return;
        }

        existingStaff.Username = textboxitem.Username.Trim();
        existingStaff.Password = textboxitem.Password;
        existingStaff.FullName = textboxitem.FullName.Trim();
        existingStaff.Status = textboxitem.Status ?? true;
        existingStaff.Role = "Staff";

        context.SaveChanges();

        int index = staffs.IndexOf(_selecteditem);
        if (index >= 0)
        {
            staffs[index] = (User)existingStaff.Clone();
        }

        Clear(null);
    }

    private void Search(object? obj)
    {
        if (string.IsNullOrWhiteSpace(searchtext))
        {
            _staffView.Filter = null;
            return;
        }

        _staffView.Filter = item =>
        {
            var staff = item as User;
            if (staff == null)
            {
                return false;
            }

            return (staff.Username?.Contains(searchtext, StringComparison.OrdinalIgnoreCase) ?? false)
                || (staff.FullName?.Contains(searchtext, StringComparison.OrdinalIgnoreCase) ?? false)
                || ((staff.Status == true ? "active" : "locked").Contains(searchtext, StringComparison.OrdinalIgnoreCase));
        };
    }

    private void Clear(object? obj)
    {
        textboxitem = CreateEmptyStaff();
        selecteditem = null;
        errormessage = string.Empty;
    }
}
