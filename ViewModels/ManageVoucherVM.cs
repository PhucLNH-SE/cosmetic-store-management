using CosmeticStoreManagement.Data;
using CosmeticStoreManagement.Helpers;
using CosmeticStoreManagement.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace CosmeticStoreManagement.ViewModels
{
    public class ManageVoucherVM : BaseViewModel
    {
        AppDbContext context = new AppDbContext();

        public ObservableCollection<Voucher> vouchers { get; set; }

        public ICollectionView voucherView { get; set; }

        private Voucher _textboxitem = new Voucher();
        public Voucher textboxitem
        {
            get => _textboxitem;
            set
            {
                _textboxitem = value;
                OnPropertyChanged();
            }
        }

        private Voucher _selecteditem;
        public Voucher selecteditem
        {
            get => _selecteditem;
            set
            {
                _selecteditem = value;
                OnPropertyChanged();

                if (_selecteditem != null)
                {
                    textboxitem = new Voucher
                    {
                        VoucherId = _selecteditem.VoucherId,
                        VoucherCode = _selecteditem.VoucherCode,
                        DiscountType = _selecteditem.DiscountType,
                        DiscountValue = _selecteditem.DiscountValue,
                        StartDate = _selecteditem.StartDate,
                        EndDate = _selecteditem.EndDate,
                        IsActive = _selecteditem.IsActive
                    };

                    OnPropertyChanged(nameof(textboxitem));
                }
            }
        }

        public RelayCommand AddCommand { get; set; }
        public RelayCommand UpdateCommand { get; set; }
        public RelayCommand DeleteCommand { get; set; }
        public RelayCommand ClearCommand { get; set; }

        public ManageVoucherVM()
        {
            Load();

            AddCommand = new RelayCommand(
                p => AddVoucher(),
                p => true
            );

            UpdateCommand = new RelayCommand(
                p => UpdateVoucher(),
                p => selecteditem != null
            );

            DeleteCommand = new RelayCommand(
                p => DeleteVoucher(),
                p => selecteditem != null
            );

            ClearCommand = new RelayCommand(
                p => ClearForm(),
                p => true
            );
        }

        void Load()
        {
            var list = context.Vouchers.ToList();

            vouchers = new ObservableCollection<Voucher>(list);

            voucherView = CollectionViewSource.GetDefaultView(vouchers);

            OnPropertyChanged(nameof(voucherView));
        }

        void AddVoucher()
        {
            if (selecteditem != null)
            {
                MessageBox.Show("Please clear the form before adding a new voucher.");
                return;
            }
            if (!ValidateVoucher())
                return;

            Voucher v = new Voucher
            {
                VoucherCode = textboxitem.VoucherCode,
                DiscountType = textboxitem.DiscountType,
                DiscountValue = textboxitem.DiscountValue,
                StartDate = textboxitem.StartDate,
                EndDate = textboxitem.EndDate,
                IsActive = textboxitem.IsActive
            };

            context.Vouchers.Add(v);
            context.SaveChanges();

            vouchers.Add(v);

            ClearForm();
        }

        void UpdateVoucher()
        {
            if (!ValidateVoucher())
                return;

            var voucher = context.Vouchers
                .FirstOrDefault(v => v.VoucherId == textboxitem.VoucherId);

            if (voucher != null)
            {
                voucher.VoucherCode = textboxitem.VoucherCode;
                voucher.DiscountType = textboxitem.DiscountType;
                voucher.DiscountValue = textboxitem.DiscountValue;
                voucher.StartDate = textboxitem.StartDate;
                voucher.EndDate = textboxitem.EndDate;
                voucher.IsActive = textboxitem.IsActive;

                context.SaveChanges();

                selecteditem.VoucherCode = textboxitem.VoucherCode;
                selecteditem.DiscountType = textboxitem.DiscountType;
                selecteditem.DiscountValue = textboxitem.DiscountValue;
                selecteditem.StartDate = textboxitem.StartDate;
                selecteditem.EndDate = textboxitem.EndDate;
                selecteditem.IsActive = textboxitem.IsActive;

                voucherView.Refresh();
            }
        }

        void DeleteVoucher()
        {
            var voucher = context.Vouchers
                .FirstOrDefault(v => v.VoucherId == selecteditem.VoucherId);

            if (voucher != null)
            {
                var confirm = MessageBox.Show(
                    "Delete this voucher?",
                    "Confirm",
                    MessageBoxButton.YesNo
                );

                if (confirm == MessageBoxResult.Yes)
                {
                    context.Vouchers.Remove(voucher);
                    context.SaveChanges();

                    vouchers.Remove(selecteditem);

                    ClearForm();
                }
            }
        }

        void ClearForm()
        {
            textboxitem = new Voucher();
            selecteditem = null;

            OnPropertyChanged(nameof(textboxitem));
        }

        bool ValidateVoucher()
        {
            if (string.IsNullOrWhiteSpace(textboxitem.VoucherCode))
            {
                MessageBox.Show("Voucher code is required.");
                return false;
            }

            if (context.Vouchers.Any(v => v.VoucherCode == textboxitem.VoucherCode
                && v.VoucherId != textboxitem.VoucherId))
            {
                MessageBox.Show("Voucher code already exists.");
                return false;
            }

            if (textboxitem.DiscountValue <= 0)
            {
                MessageBox.Show("Discount value must be positive.");
                return false;
            }

            if (textboxitem.DiscountType == "PERCENT")
            {
                if (textboxitem.DiscountValue < 1 || textboxitem.DiscountValue > 100)
                {
                    MessageBox.Show("Percent discount must be between 1 and 100.");
                    return false;
                }
            }

            if (textboxitem.StartDate == null || textboxitem.EndDate == null)
            {
                MessageBox.Show("Start date and end date are required.");
                return false;
            }

            DateTime today = DateTime.Today;

            if (textboxitem.StartDate < today)
            {
                MessageBox.Show("Start date must be today or later.");
                return false;
            }

            if (textboxitem.EndDate < today)
            {
                MessageBox.Show("End date must be today or later.");
                return false;
            }

            if (textboxitem.StartDate >= textboxitem.EndDate)
            {
                MessageBox.Show("Start date must be earlier than end date.");
                return false;
            }

            return true;
        }
    }
}