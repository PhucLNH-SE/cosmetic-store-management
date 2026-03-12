using CosmeticStoreManagement.Data;
using CosmeticStoreManagement.Helpers;
using CosmeticStoreManagement.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace CosmeticStoreManagement.ViewModels.admin
{
    public class ProductItemDisplay : BaseViewModel
    {
        public int ProductId { get; set; }
        public int VariantId { get; set; }

        private string _productName = string.Empty;
        public string ProductName { get => _productName; set { _productName = value; OnPropertyChanged(); } }

        private string _volume = string.Empty;
        public string Volume { get => _volume; set { _volume = value; OnPropertyChanged(); } }

        private decimal _price;
        public decimal Price { get => _price; set { _price = value; OnPropertyChanged(); } }

        private string _imagePath = string.Empty;
        public string ImagePath { get => _imagePath; set { _imagePath = value; OnPropertyChanged(); } }

        private bool? _isActive;
        public bool? IsActive { get => _isActive; set { _isActive = value; OnPropertyChanged(); } }

        public int CategoryId { get; set; }
        private string _categoryName = string.Empty;
        public string CategoryName { get => _categoryName; set { _categoryName = value; OnPropertyChanged(); } }

        public int BrandId { get; set; }
        private string _brandName = string.Empty;
        public string BrandName { get => _brandName; set { _brandName = value; OnPropertyChanged(); } }
    }

    public class ManageProductPageVM : BaseViewModel
    {
        #region Properties & Commands
        private ObservableCollection<ProductItemDisplay> _products;
        public ObservableCollection<ProductItemDisplay> products
        {
            get => _products;
            set { _products = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Category> _categoriesList;
        public ObservableCollection<Category> CategoriesList
        {
            get => _categoriesList;
            set { _categoriesList = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Brand> _brandsList;
        public ObservableCollection<Brand> BrandsList
        {
            get => _brandsList;
            set { _brandsList = value; OnPropertyChanged(); }
        }

        private ProductItemDisplay _textboxitem;
        public ProductItemDisplay textboxitem
        {
            get => _textboxitem;
            set { _textboxitem = value; OnPropertyChanged(); }
        }

        private ProductItemDisplay _selecteditem;
        public ProductItemDisplay selecteditem
        {
            get => _selecteditem;
            set
            {
                _selecteditem = value;
                OnPropertyChanged();
                if (_selecteditem != null)
                {
                    textboxitem = new ProductItemDisplay
                    {
                        ProductId = _selecteditem.ProductId,
                        VariantId = _selecteditem.VariantId,
                        ProductName = _selecteditem.ProductName,
                        Volume = _selecteditem.Volume,
                        Price = _selecteditem.Price,
                        ImagePath = _selecteditem.ImagePath,
                        IsActive = _selecteditem.IsActive,
                        BrandId = _selecteditem.BrandId,
                        BrandName = _selecteditem.BrandName,
                        CategoryId = _selecteditem.CategoryId,
                        CategoryName = _selecteditem.CategoryName
                    };
                }
            }
        }

        private string _searchtext = string.Empty;
        public string searchtext
        {
            get => _searchtext;
            set { _searchtext = value; OnPropertyChanged(); }
        }

        // --- ĐÂY LÀ PHẦN SỬA LỖI ADD XONG TÀNG HÌNH ---
        private ICollectionView _productsView;
        public ICollectionView ProductsView
        {
            get => _productsView;
            set { _productsView = value; OnPropertyChanged(); } // Báo cho UI biết để F5
        }
        // ----------------------------------------------

        public ICommand AddCommand { get; set; }
        public ICommand AddTypeCommand { get; set; }
        public ICommand UpdateCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand ChooseImageCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        #endregion

        public ManageProductPageVM()
        {
            textboxitem = new ProductItemDisplay();
            LoadData();

            ChooseImageCommand = new RelayCommand(o => ChooseImage());
            ClearCommand = new RelayCommand(o => { textboxitem = new ProductItemDisplay(); });
            SearchCommand = new RelayCommand(o => SearchProduct());

            AddCommand = new RelayCommand(
                o => AddProduct(),
                o => !string.IsNullOrEmpty(textboxitem.ProductName) && textboxitem.Price > 0
            );

            AddTypeCommand = new RelayCommand(
                o => AddOnlyVariant(),
                o => selecteditem != null && !string.IsNullOrEmpty(textboxitem.Volume)
            );

            UpdateCommand = new RelayCommand(
                o => UpdateProduct(),
                o => textboxitem.ProductId > 0
            );

            DeleteCommand = new RelayCommand(
                o => ToggleProductStatus(),
                o => textboxitem.ProductId > 0
            );
        }

        public void LoadData()
        {
            using (var context = new AppDbContext())
            {
                CategoriesList = new ObservableCollection<Category>(context.Categories.Where(c => c.Status == true).ToList());
                BrandsList = new ObservableCollection<Brand>(context.Brands.Where(b => b.Status == true).ToList());
                OnPropertyChanged(nameof(CategoriesList));
                OnPropertyChanged(nameof(BrandsList));

                var rawData = (from p in context.Products
                               join v in context.ProductVariants on p.ProductId equals v.ProductId
                               join c in context.Categories on p.CategoryId equals c.CategoryId
                               join b in context.Brands on p.BrandId equals b.BrandId
                               select new
                               {
                                   p.ProductId,
                                   v.VariantId,
                                   p.ProductName,
                                   v.Volume,
                                   Price = v.Price ?? 0,
                                   IsActive = p.IsActive,
                                   p.BrandId,
                                   BrandName = b.BrandName,
                                   p.CategoryId,
                                   CategoryName = c.CategoryName,
                                   v.ImagePath
                               }).ToList();

                var displayList = new List<ProductItemDisplay>();
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                foreach (var item in rawData)
                {
                    string dbPath = item.ImagePath ?? string.Empty;
                    string autoPath = Path.Combine(baseDir, "Images", "Products", $"product_{item.ProductId}.png");

                    string finalDisplayPath = string.Empty;

                    // KIỂM TRA ĐƯỜNG DẪN: Ưu tiên ảnh mới lưu trong DB, nếu không có thì xài autoPath của 100 tấm cũ
                    if (File.Exists(dbPath))
                        finalDisplayPath = dbPath;
                    else if (File.Exists(autoPath))
                        finalDisplayPath = autoPath;

                    displayList.Add(new ProductItemDisplay
                    {
                        ProductId = item.ProductId,
                        VariantId = item.VariantId,
                        ProductName = item.ProductName,
                        Volume = item.Volume,
                        Price = item.Price,
                        ImagePath = finalDisplayPath,
                        IsActive = item.IsActive,
                        BrandId = item.BrandId,
                        BrandName = item.BrandName,
                        CategoryId = item.CategoryId,
                        CategoryName = item.CategoryName
                    });
                }

                products = new ObservableCollection<ProductItemDisplay>(displayList);
                ProductsView = CollectionViewSource.GetDefaultView(products);
            }
        }

        private void SearchProduct()
        {
            if (ProductsView != null)
            {
                if (string.IsNullOrWhiteSpace(searchtext)) ProductsView.Filter = null;
                else
                {
                    ProductsView.Filter = obj =>
                    {
                        var item = obj as ProductItemDisplay;
                        return item != null && item.ProductName.ToLower().Contains(searchtext.ToLower());
                    };
                }
            }
        }

        private void ChooseImage()
        {
            OpenFileDialog op = new OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg" };
            if (op.ShowDialog() == true)
            {
                // Chỉ lấy đường dẫn hiện lên UI, chưa vội copy khóa file
                textboxitem.ImagePath = op.FileName;
            }
        }

        // --- HÀM XỬ LÝ ẢNH TRÁNH LỖI WPF KHÓA FILE ---
        private string SaveImageSafely(string sourcePath, int productId)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath)) return string.Empty;

            string destFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Products");

            // Nếu ảnh đã nằm trong thư mục project rồi thì không cần copy nữa
            if (sourcePath.Contains(destFolder)) return sourcePath;

            try
            {
                if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);

                string ext = Path.GetExtension(sourcePath);
                if (string.IsNullOrEmpty(ext)) ext = ".png";

                // Thêm Ticks để tên file luôn duy nhất (Tránh 100% vụ khóa file trùng tên)
                string fileName = $"prod_{productId}_{DateTime.Now.Ticks}{ext}";
                string destPath = Path.Combine(destFolder, fileName);

                File.Copy(sourcePath, destPath, true);
                return destPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi copy ảnh: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }

        private void AddProduct()
        {
            if (textboxitem.CategoryId == 0 || textboxitem.BrandId == 0)
            {
                MessageBox.Show("Vui lòng chọn Danh mục và Thương hiệu!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new AppDbContext())
            {
                try
                {
                    if (context.Products.Any(p => p.ProductName.ToLower() == textboxitem.ProductName.ToLower()))
                    {
                        MessageBox.Show("Sản phẩm này đã tồn tại!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var newP = new Product
                    {
                        ProductName = textboxitem.ProductName,
                        BrandId = textboxitem.BrandId,
                        CategoryId = textboxitem.CategoryId,
                        IsActive = true
                    };
                    context.Products.Add(newP);
                    context.SaveChanges();

                    // Xử lý lưu ảnh an toàn
                    string safeImagePath = SaveImageSafely(textboxitem.ImagePath, newP.ProductId);

                    var newV = new ProductVariant
                    {
                        ProductId = newP.ProductId,
                        Volume = textboxitem.Volume,
                        Price = textboxitem.Price,
                        ImagePath = safeImagePath,
                        IsActive = true,
                        StockQuantity = 0
                    };
                    context.ProductVariants.Add(newV);
                    context.SaveChanges();

                    MessageBox.Show("Thêm mới thành công!");
                    textboxitem = new ProductItemDisplay();
                    LoadData();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi Database: " + ex.Message); }
            }
        }

        private void AddOnlyVariant()
        {
            using (var context = new AppDbContext())
            {
                bool isExist = context.ProductVariants.Any(v => v.ProductId == selecteditem.ProductId && v.Volume.ToLower() == textboxitem.Volume.ToLower());
                if (isExist)
                {
                    MessageBox.Show("Dung tích này đã tồn tại!");
                    return;
                }

                string safeImagePath = SaveImageSafely(textboxitem.ImagePath, selecteditem.ProductId);

                var v = new ProductVariant
                {
                    ProductId = selecteditem.ProductId,
                    Volume = textboxitem.Volume,
                    Price = textboxitem.Price,
                    ImagePath = safeImagePath,
                    IsActive = true,
                    StockQuantity = 0
                };
                context.ProductVariants.Add(v);
                context.SaveChanges();

                MessageBox.Show($"Thêm loại {textboxitem.Volume} thành công!");
                LoadData();
            }
        }

        private void UpdateProduct()
        {
            using (var context = new AppDbContext())
            {
                var p = context.Products.Find(textboxitem.ProductId);
                var v = context.ProductVariants.Find(textboxitem.VariantId);
                if (p != null && v != null)
                {
                    p.ProductName = textboxitem.ProductName;
                    p.CategoryId = textboxitem.CategoryId;
                    p.BrandId = textboxitem.BrandId;
                    v.Volume = textboxitem.Volume;
                    v.Price = textboxitem.Price;

                    string safeImagePath = SaveImageSafely(textboxitem.ImagePath, p.ProductId);
                    if (!string.IsNullOrEmpty(safeImagePath))
                    {
                        v.ImagePath = safeImagePath;
                    }

                    context.SaveChanges();
                    MessageBox.Show("Cập nhật thành công!");
                    LoadData();
                }
            }
        }

        private void ToggleProductStatus()
        {
            using (var context = new AppDbContext())
            {
                var p = context.Products.Find(textboxitem.ProductId);
                if (p != null)
                {
                    p.IsActive = !(p.IsActive ?? false);
                    context.SaveChanges();
                    MessageBox.Show(p.IsActive == true ? "Đã hiện sản phẩm!" : "Đã ẩn sản phẩm!");
                    LoadData();
                }
            }
        }
    }
}