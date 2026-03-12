using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using CosmeticStoreManagement.Data;
using CosmeticStoreManagement.Helpers;
using CosmeticStoreManagement.Models;

namespace CosmeticStoreManagement.ViewModels;

public class ManageBrandPageVM : BaseViewModel
{
    private ICollectionView _brandView;
    private Brand _textboxitem;
    private Brand? _selecteditem;
    private string _searchtext = string.Empty;
    private string _statusmessage = string.Empty;

    public ManageBrandPageVM()
    {
        brands = new ObservableCollection<Brand>();
        _brandView = CollectionViewSource.GetDefaultView(brands);
        _textboxitem = new Brand { Status = true };

        AddCommand = new RelayCommand(Add);
        UpdateCommand = new RelayCommand(Update);
        DeleteCommand = new RelayCommand(Delete);
        SearchCommand = new RelayCommand(Search);
        ClearCommand = new RelayCommand(ClearForm);

        Load();
    }

    public ObservableCollection<Brand> brands { get; set; }

    public ICollectionView BrandView
    {
        get => _brandView;
        set
        {
            _brandView = value;
            OnPropertyChanged(nameof(BrandView));
        }
    }

    public Brand textboxitem
    {
        get => _textboxitem;
        set
        {
            _textboxitem = value;
            OnPropertyChanged(nameof(textboxitem));
        }
    }

    public Brand? selecteditem
    {
        get => _selecteditem;
        set
        {
            _selecteditem = value;
            OnPropertyChanged(nameof(selecteditem));

            if (_selecteditem != null)
            {
                textboxitem = (Brand)_selecteditem.Clone();
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
    public ICommand DeleteCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand ClearCommand { get; }

    private void Load()
    {
        using var context = new AppDbContext();
        var list = context.Brands.OrderBy(x => x.BrandId).ToList();

        brands = new ObservableCollection<Brand>(list);
        OnPropertyChanged(nameof(brands));

        BrandView = CollectionViewSource.GetDefaultView(brands);
        ApplyFilter();
    }

    private void Add(object? parameter)
    {
        if (string.IsNullOrWhiteSpace(textboxitem.BrandName))
        {
            statusmessage = "Brand name is required.";
            return;
        }

        var brandName = textboxitem.BrandName.Trim();
        var country = NormalizeString(textboxitem.Country);
        var status = textboxitem.Status ?? true;

        using var context = new AppDbContext();
        var duplicated = context.Brands.Any(x => x.BrandName.ToLower() == brandName.ToLower());
        if (duplicated)
        {
            statusmessage = "Brand name already exists.";
            return;
        }

        var newBrand = new Brand
        {
            BrandName = brandName,
            Country = country,
            Status = status
        };

        context.Brands.Add(newBrand);
        context.SaveChanges();

        brands.Add(newBrand);
        ApplyFilter();

        textboxitem = new Brand { Status = true };
        statusmessage = "Create brand successfully.";
    }

    private void Update(object? parameter)
    {
        if (selecteditem == null)
        {
            statusmessage = "Select one brand to update.";
            return;
        }

        if (string.IsNullOrWhiteSpace(textboxitem.BrandName))
        {
            statusmessage = "Brand name is required.";
            return;
        }

        using var context = new AppDbContext();
        var brandInDb = context.Brands.FirstOrDefault(x => x.BrandId == selecteditem.BrandId);
        if (brandInDb == null)
        {
            statusmessage = "Brand not found.";
            return;
        }

        var brandName = textboxitem.BrandName.Trim();
        var duplicated = context.Brands.Any(x => x.BrandId != brandInDb.BrandId && x.BrandName.ToLower() == brandName.ToLower());
        if (duplicated)
        {
            statusmessage = "Brand name already exists.";
            return;
        }

        brandInDb.BrandName = brandName;
        brandInDb.Country = NormalizeString(textboxitem.Country);
        brandInDb.Status = textboxitem.Status ?? true;

        context.SaveChanges();

        var oldItem = brands.FirstOrDefault(x => x.BrandId == brandInDb.BrandId);
        if (oldItem != null)
        {
            var index = brands.IndexOf(oldItem);
            brands[index] = (Brand)brandInDb.Clone();
        }

        ApplyFilter();
        statusmessage = "Update brand successfully.";
    }

    private void Delete(object? parameter)
    {
        if (selecteditem == null)
        {
            statusmessage = "Select one brand to delete.";
            return;
        }

        using var context = new AppDbContext();
        var brandInDb = context.Brands.FirstOrDefault(x => x.BrandId == selecteditem.BrandId);
        if (brandInDb == null)
        {
            statusmessage = "Brand not found.";
            return;
        }

        brandInDb.Status = false;
        context.SaveChanges();

        var localBrand = brands.FirstOrDefault(x => x.BrandId == brandInDb.BrandId);
        if (localBrand != null)
        {
            localBrand.Status = false;
        }

        selecteditem = null;
        textboxitem = new Brand { Status = true };

        ApplyFilter();
        statusmessage = "Delete brand successfully.";
    }

    private void Search(object? parameter)
    {
        ApplyFilter();
    }

    private void ClearForm(object? parameter)
    {
        selecteditem = null;
        textboxitem = new Brand { Status = true };
        statusmessage = string.Empty;
    }

    private void ApplyFilter()
    {
        var keyword = searchtext?.Trim() ?? string.Empty;

        BrandView.Filter = item =>
        {
            if (item is not Brand brand)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(keyword))
            {
                return true;
            }

            return brand.BrandId.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase)
                   || (!string.IsNullOrWhiteSpace(brand.BrandName)
                       && brand.BrandName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                   || (!string.IsNullOrWhiteSpace(brand.Country)
                       && brand.Country.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        };

        BrandView.Refresh();
    }

    private static string? NormalizeString(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}

