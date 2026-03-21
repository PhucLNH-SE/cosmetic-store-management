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
                o => !string.IsNullOrWhiteSpace(textboxitem.ProductName)
                    && !string.IsNullOrWhiteSpace(textboxitem.Volume)
                    && textboxitem.Price > 0
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
                                    p.ImagePath
                                }).ToList();

                var displayList = new List<ProductItemDisplay>();
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                foreach (var item in rawData)
                {
                    string dbPath = item.ImagePath ?? string.Empty;
                    string autoPath = Path.Combine(baseDir, "Images", "Products", $"product_{item.ProductId}.png");

                    string finalDisplayPath = string.Empty;

                   
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
               
                textboxitem.ImagePath = op.FileName;
            }
        }

       
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

            if (string.IsNullOrWhiteSpace(textboxitem.Volume))
            {
                MessageBox.Show("Vui lòng nhập dung tích/loại sản phẩm!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new AppDbContext())
            {
                try
                {
                    string normalizedName = textboxitem.ProductName.Trim().ToLower();
                    if (context.Products.Any(p =>
                        p.ProductName.ToLower() == normalizedName &&
                        p.BrandId == textboxitem.BrandId &&
                        p.CategoryId == textboxitem.CategoryId))
                    {
                        MessageBox.Show("Sản phẩm này đã tồn tại trong cùng thương hiệu và danh mục!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var newP = new Product
                    {
                        ProductName = textboxitem.ProductName.Trim(),
                        BrandId = textboxitem.BrandId,
                        CategoryId = textboxitem.CategoryId,
                        IsActive = true
                    };
                    context.Products.Add(newP);
                    context.SaveChanges();

                    string safeImagePath = SaveImageSafely(textboxitem.ImagePath, newP.ProductId);
                    if (!string.IsNullOrWhiteSpace(safeImagePath))
                    {
                        newP.ImagePath = safeImagePath;
                    }

                    var newV = new ProductVariant
                    {
                        ProductId = newP.ProductId,
                        Volume = textboxitem.Volume.Trim(),
                        Price = textboxitem.Price,
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
                string normalizedVolume = textboxitem.Volume?.Trim().ToLower() ?? string.Empty;
                bool isExist = context.ProductVariants.Any(v =>
                    v.ProductId == selecteditem.ProductId &&
                    v.Volume != null &&
                    v.Volume.ToLower() == normalizedVolume);
                if (isExist)
                {
                    MessageBox.Show("Dung tích này đã tồn tại!");
                    return;
                }

                var v = new ProductVariant
                {
                    ProductId = selecteditem.ProductId,
                    Volume = textboxitem.Volume.Trim(),
                    Price = textboxitem.Price,
                    IsActive = true,
                    StockQuantity = 0
                };
                context.ProductVariants.Add(v);

                if (!string.IsNullOrWhiteSpace(textboxitem.ImagePath))
                {
                    var product = context.Products.Find(selecteditem.ProductId);
                    if (product != null)
                    {
                        string safeImagePath = SaveImageSafely(textboxitem.ImagePath, product.ProductId);
                        if (!string.IsNullOrWhiteSpace(safeImagePath))
                        {
                            product.ImagePath = safeImagePath;
                        }
                    }
                }

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
                    if (string.IsNullOrWhiteSpace(textboxitem.ProductName) || string.IsNullOrWhiteSpace(textboxitem.Volume))
                    {
                        MessageBox.Show("Tên sản phẩm và dung tích không được để trống!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string normalizedName = textboxitem.ProductName.Trim().ToLower();
                    bool duplicatedProduct = context.Products.Any(prod =>
                        prod.ProductId != p.ProductId &&
                        prod.ProductName.ToLower() == normalizedName &&
                        prod.BrandId == textboxitem.BrandId &&
                        prod.CategoryId == textboxitem.CategoryId);
                    if (duplicatedProduct)
                    {
                        MessageBox.Show("Đã tồn tại sản phẩm khác cùng tên, thương hiệu và danh mục!", "Trùng dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string normalizedVolume = textboxitem.Volume.Trim().ToLower();
                    bool duplicatedVariant = context.ProductVariants.Any(variant =>
                        variant.VariantId != v.VariantId &&
                        variant.ProductId == p.ProductId &&
                        variant.Volume != null &&
                        variant.Volume.ToLower() == normalizedVolume);
                    if (duplicatedVariant)
                    {
                        MessageBox.Show("Dung tích này đã tồn tại cho sản phẩm hiện tại!", "Trùng dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    p.ProductName = textboxitem.ProductName.Trim();
                    p.CategoryId = textboxitem.CategoryId;
                    p.BrandId = textboxitem.BrandId;
                    v.Volume = textboxitem.Volume.Trim();
                    v.Price = textboxitem.Price;

                    string safeImagePath = SaveImageSafely(textboxitem.ImagePath, p.ProductId);
                    if (!string.IsNullOrEmpty(safeImagePath))
                    {
                        p.ImagePath = safeImagePath;
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
