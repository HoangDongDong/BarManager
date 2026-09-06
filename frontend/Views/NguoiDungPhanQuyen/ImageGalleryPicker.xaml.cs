using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.NguoiDungPhanQuyen
{
    public partial class ImageGalleryPicker : UserControl
    {
        private static List<ThuVienAnhGroupViewModel> _cachedGroups;
        private static Dictionary<string, ImageSource> _cachedImageMap;
        private bool _isLoaded;

        public static readonly DependencyProperty SelectedSimageIdProperty =
            DependencyProperty.Register(
                nameof(SelectedSimageId),
                typeof(string),
                typeof(ImageGalleryPicker),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedSimageIdChanged));

        public string SelectedSimageId
        {
            get => (string)GetValue(SelectedSimageIdProperty);
            set => SetValue(SelectedSimageIdProperty, value);
        }

        public static readonly DependencyProperty SelectedImageSourceProperty =
            DependencyProperty.Register(
                nameof(SelectedImageSource),
                typeof(ImageSource),
                typeof(ImageGalleryPicker),
                new PropertyMetadata(null));

        public ImageSource SelectedImageSource
        {
            get => (ImageSource)GetValue(SelectedImageSourceProperty);
            set => SetValue(SelectedImageSourceProperty, value);
        }

        public event Action<string, ImageSource> ImageSelected;

        public ImageGalleryPicker()
        {
            InitializeComponent();
            Loaded += ImageGalleryPicker_Loaded;
        }

        private async void ImageGalleryPicker_Loaded(object sender, RoutedEventArgs e)
        {
            await EnsureLoadedAsync();
            UpdateSelectedImageSource();
        }

        private static void OnSelectedSimageIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageGalleryPicker picker)
            {
                picker.UpdateSelectedImageSource();
            }
        }

        private async Task EnsureLoadedAsync()
        {
            if (_isLoaded && _cachedGroups != null)
            {
                LstGroups.ItemsSource = _cachedGroups;
                return;
            }

            try
            {
                if (_cachedGroups == null)
                {
                    _cachedGroups = await LocalThuVienAnhService.GetGroupedImagesAsync();
                    _cachedImageMap = new Dictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase);

                    foreach (var g in _cachedGroups)
                    {
                        foreach (var item in g.Items)
                        {
                            if (!string.IsNullOrEmpty(item.Id) && item.ImageSource != null)
                            {
                                _cachedImageMap[item.Id] = item.ImageSource;
                            }
                        }
                    }
                }

                LstGroups.ItemsSource = _cachedGroups;
                _isLoaded = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error EnsureLoadedAsync: " + ex.Message);
            }
        }

        private void UpdateSelectedImageSource()
        {
            string id = SelectedSimageId;
            if (string.IsNullOrEmpty(id) || id == "0")
            {
                SelectedImageSource = null;
                return;
            }

            if (_cachedImageMap != null && _cachedImageMap.TryGetValue(id, out var src))
            {
                SelectedImageSource = src;
            }
            else
            {
                // Try async load if not in cache
                _ = LoadImageSourceAsync(id);
            }
        }

        private async Task LoadImageSourceAsync(string id)
        {
            await EnsureLoadedAsync();
            if (_cachedImageMap != null && _cachedImageMap.TryGetValue(id, out var src))
            {
                SelectedImageSource = src;
            }
        }

        private async void BtnToggle_Click(object sender, RoutedEventArgs e)
        {
            await EnsureLoadedAsync();
            PopupGallery.IsOpen = BtnToggle.IsChecked == true;
        }

        private void PopupGallery_Closed(object sender, EventArgs e)
        {
            BtnToggle.IsChecked = false;
        }

        private void BtnNone_Click(object sender, RoutedEventArgs e)
        {
            SelectedSimageId = null;
            SelectedImageSource = null;
            PopupGallery.IsOpen = false;
            ImageSelected?.Invoke(null, null);
        }

        private void BtnIcon_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ThuVienAnhItemViewModel item)
            {
                SelectedSimageId = item.Id;
                SelectedImageSource = item.ImageSource;
                PopupGallery.IsOpen = false;
                ImageSelected?.Invoke(item.Id, item.ImageSource);
            }
        }
    }
}
