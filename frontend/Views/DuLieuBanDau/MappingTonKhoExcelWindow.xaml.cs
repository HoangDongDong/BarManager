using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace QuanLyBar.Client.Views.DuLieuBanDau
{
    public class ColumnMappingTonKhoItem : INotifyPropertyChanged
    {
        public string ExcelColumn { get; set; } = "";

        private string _mappedField = "";
        public string MappedField
        {
            get => _mappedField;
            set
            {
                if (_mappedField != value)
                {
                    _mappedField = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MappedField)));
                }
            }
        }

        public List<string> AvailableFields { get; set; } = new List<string>();

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public partial class MappingTonKhoExcelWindow : Window
    {
        public Dictionary<string, string> FinalMappings { get; private set; } = new Dictionary<string, string>();
        private ObservableCollection<ColumnMappingTonKhoItem> _mappingList = new ObservableCollection<ColumnMappingTonKhoItem>();

        private readonly List<string> _dbFields = new List<string>
        {
            "",
            "Mã hàng hóa",
            "Tồn",
            "Giá vốn"
        };

        public MappingTonKhoExcelWindow(List<string> excelColumns)
        {
            InitializeComponent();

            foreach (var col in excelColumns)
            {
                var item = new ColumnMappingTonKhoItem
                {
                    ExcelColumn = col,
                    AvailableFields = _dbFields,
                    MappedField = "" // Ban đầu để trống
                };
                _mappingList.Add(item);
            }

            DgMapping.ItemsSource = _mappingList;
        }

        private string AutoDetectField(string colName)
        {
            string lower = colName.ToLower().Trim();
            if (lower == "mã hàng hóa" || lower == "mã hàng" || lower == "ma hang hoa" || lower == "ma hang" || lower == "mahang") return "Mã hàng hóa";
            if (lower == "tồn" || lower == "ton") return "Tồn";
            if (lower == "giá vốn" || lower == "gia von" || lower == "giavon") return "Giá vốn";
            return "";
        }

        private void BtnTuDongChon_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _mappingList)
            {
                item.MappedField = AutoDetectField(item.ExcelColumn);
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            FinalMappings = _mappingList
                .Where(x => !string.IsNullOrWhiteSpace(x.MappedField))
                .ToDictionary(x => x.ExcelColumn, x => x.MappedField);

            bool hasIdentifier = FinalMappings.Values.Any(v => v == "Mã hàng hóa");
            if (!hasIdentifier)
            {
                MessageBox.Show("Vui lòng chọn cột 'Mã hàng hóa'!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
