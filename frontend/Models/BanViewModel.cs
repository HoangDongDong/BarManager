using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuanLyBar.Client.Models
{
    public class BanViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Stt { get; set; } // Số thứ tự
        public string SttDisplay => Stt.ToString("D2");
        public string Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string KhuVucName { get; set; }
        public string NhomHienThiName { get; set; }
        public string LoaiPhongName { get; set; }
        public string BanggiaName { get; set; }
        public decimal? Dongia { get; set; }
        public decimal? Tienmoban { get; set; }
        public DateTime? Timecreated { get; set; }
        public string UsercreatedId { get; set; }
        public string UsercreatedName { get; set; }
        public DateTime? Timemodified { get; set; }
        public string UsermodifiedId { get; set; }
        public string UsermodifiedName { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
