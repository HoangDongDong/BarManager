using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuanLyBar.Client.Models
{
    public class KiemDoItemViewModel : INotifyPropertyChanged
    {
        private decimal _slTra;

        public string Id { get; set; }
        public string MatHangId { get; set; }
        public int Stt { get; set; }
        public string MatHang { get; set; }
        public decimal DonGia { get; set; }
        public decimal ChietKhauPt { get; set; }
        public decimal SlGoi { get; set; }

        public decimal SlTra
        {
            get => _slTra;
            set
            {
                if (value < 0) value = 0;
                if (value > SlGoi) value = SlGoi;
                if (_slTra != value)
                {
                    _slTra = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SlSuDung));
                    OnPropertyChanged(nameof(ThanhTien));
                }
            }
        }

        public decimal SlSuDung => Math.Max(0, SlGoi - SlTra);

        public decimal ThanhTien => SlSuDung * DonGia * (1 - (ChietKhauPt / 100m));

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
