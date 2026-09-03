using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace QuanLyBar.Client.Views.QuanLyNhapKho
{
    public class MauInMaVachItem
    {
        public int Id { get; set; }
        public string Icon { get; set; } = "📄";
        public string TenMau { get; set; } = "";
        public string BarcodeType { get; set; } = "CODE128";
        public int Columns { get; set; } = 2;
        public int RowsPerPage { get; set; } = 1;
        public bool IsPaperA4 { get; set; } = false;
        public string FontWeight { get; set; } = "Normal";
    }

    public partial class MauInMaVachWindow : Window
    {
        public ObservableCollection<MauInMaVachItem> MauList { get; set; } = new();
        public MauInMaVachItem SelectedMau { get; private set; }

        public MauInMaVachWindow()
        {
            InitializeComponent();
            LoadDefaultTemplates();
            LbMauIn.ItemsSource = MauList;
            if (MauList.Count > 0) LbMauIn.SelectedIndex = 0;
        }

        private void LoadDefaultTemplates()
        {
            MauList.Add(new MauInMaVachItem
            {
                Id = 1,
                Icon = "⭐",
                TenMau = "Máy in chuyên dụng 2 tem, mã CODE128",
                BarcodeType = "CODE128",
                Columns = 2,
                RowsPerPage = 1,
                FontWeight = "Bold"
            });

            MauList.Add(new MauInMaVachItem
            {
                Id = 2,
                Icon = "📄",
                TenMau = "Máy in chuyên dụng 3 cột, mã CODE128",
                BarcodeType = "CODE128",
                Columns = 3,
                RowsPerPage = 1
            });

            MauList.Add(new MauInMaVachItem
            {
                Id = 3,
                Icon = "📄",
                TenMau = "Máy in chuyên dụng 3 cột, mã EAN8",
                BarcodeType = "EAN8",
                Columns = 3,
                RowsPerPage = 1
            });

            MauList.Add(new MauInMaVachItem
            {
                Id = 4,
                Icon = "📄",
                TenMau = "Máy in laser giấy TOMMY A4, 5 cột x 13 dòng, mã EAN8",
                BarcodeType = "EAN8",
                Columns = 5,
                RowsPerPage = 13,
                IsPaperA4 = true
            });

            MauList.Add(new MauInMaVachItem
            {
                Id = 5,
                Icon = "📄",
                TenMau = "Máy in laser giấy TOMMY No.108, 5 cột x 8 dòng, mã EAN8",
                BarcodeType = "EAN8",
                Columns = 5,
                RowsPerPage = 8,
                IsPaperA4 = true
            });

            MauList.Add(new MauInMaVachItem
            {
                Id = 6,
                Icon = "📄",
                TenMau = "Máy in laser giấy TOMMYA4, 5 cột x 13 dòng, mã CODE128",
                BarcodeType = "CODE128",
                Columns = 5,
                RowsPerPage = 13,
                IsPaperA4 = true
            });
        }

        private void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng thêm mẫu in mã vạch mới đang được cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnChinhSua_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng chỉnh sửa thông số mẫu in mã vạch.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnXoaMau_Click(object sender, RoutedEventArgs e)
        {
            var sel = LbMauIn.SelectedItem as MauInMaVachItem;
            if (sel != null && MauList.Count > 1)
            {
                MauList.Remove(sel);
                if (MauList.Count > 0) LbMauIn.SelectedIndex = 0;
            }
        }

        private void LbMauIn_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BtnChapNhan_Click(sender, e);
        }

        private void BtnThietKe_Click(object sender, RoutedEventArgs e)
        {
            var sel = LbMauIn.SelectedItem as MauInMaVachItem ?? MauList[0];
            var designerWin = new ThietKeMauInMaVachWindow(sel);
            designerWin.Owner = this;
            designerWin.ShowDialog();
        }

        private void BtnChapNhan_Click(object sender, RoutedEventArgs e)
        {
            SelectedMau = LbMauIn.SelectedItem as MauInMaVachItem ?? MauList[0];
            DialogResult = true;
            Close();
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
