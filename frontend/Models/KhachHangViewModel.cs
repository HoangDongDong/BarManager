using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace QuanLyBar.Client.Models
{
    public class KhachHangViewModel : INotifyPropertyChanged
    {
        public int Stt { get; set; }
        public string Id { get; set; }
        public string Makhach { get; set; }
        public string Name { get; set; }
        public string Diachi { get; set; }
        public string Dienthoai { get; set; }
        public string Email { get; set; }
        public string DnhomkhachhangId { get; set; }
        public string TenNhomKhachHang { get; set; }
        public string Masothue { get; set; }
        public string TenNhanVien { get; set; }
        public string TinhThanh { get; set; }
        public string Facebook { get; set; }
        public string TheTraTruoc { get; set; }
        public int Status { get; set; }
        public DateTime? Ngaysinh { get; set; }
        public decimal Diemtichluy { get; set; }
        public string Note { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    public class NhomKhachHangTreeItem : INotifyPropertyChanged
    {
        private string _name;
        private bool _isEditing;

        public string Id { get; set; }
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    OnPropertyChanged(nameof(IsEditing));
                }
            }
        }

        public bool IsExpanded { get; set; } = true;
        public bool IsSelected { get; set; }
        public string Icon { get; set; } = "📁";
        public string IconColor { get; set; } = "#f0ad4e";
        public string ParentId { get; set; }
        public int ItemType { get; set; } // 0: All, 1: Unassigned, 2: Group, 3: Trash
        public ObservableCollection<NhomKhachHangTreeItem> Children { get; set; } = new ObservableCollection<NhomKhachHangTreeItem>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
