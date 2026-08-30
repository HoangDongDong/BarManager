using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace QuanLyBar.Client.Views
{
    public class ColumnMappingViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public string ExcelColumn { get; set; }
        
        private string _mappedField;
        public string MappedField 
        { 
            get => _mappedField; 
            set 
            {
                _mappedField = value;
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(MappedField)));
            }
        }

        public List<string> AvailableFields { get; set; }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }

    public partial class MappingExcelWindow : Window
    {
        public ObservableCollection<ColumnMappingViewModel> MappingList { get; set; }

        // Mảng các trường có sẵn trong hệ thống (như trong Cột Dữ liệu)
        private List<string> _systemFields = new List<string>
        {
            "", // Cho phép bỏ trống (không map)
            "Ghi chú",
            "Tên mặt hàng",
            "Nhóm mặt hàng",
            "Giá bán",
            "Đơn vị tính",
            "Mã hàng",
            "Giá nhập",
            "Tồn tối thiểu",
            "Tồn tối đa",
            "Ảnh",
            "Hoa hồng",
            "Giá vốn",
            "Đơn vị tính chẵn",
            "Quy đổi",
            "Giá bán chẵn",
            "Đối tác ký gửi",
            "Mặc định giảm giá",
            "Mặc định giảm tiền",
            "Tạm khóa",
            "Loại mặt hàng",
            "Giá theo thời giá"
        };

        public MappingExcelWindow(List<string> excelColumns, List<string> customSystemFields = null)
        {
            InitializeComponent();
            
            if (customSystemFields != null)
            {
                _systemFields = customSystemFields;
                // Add empty mapping option if not present
                if (!_systemFields.Contains(""))
                {
                    _systemFields.Insert(0, "");
                }
            }
            
            MappingList = new ObservableCollection<ColumnMappingViewModel>();
            
            foreach (var col in excelColumns)
            {
                MappingList.Add(new ColumnMappingViewModel
                {
                    ExcelColumn = col,
                    AvailableFields = _systemFields,
                    MappedField = ""
                });
            }

            DgMapping.ItemsSource = MappingList;
        }

        private void BtnTuDongChon_Click(object sender, RoutedEventArgs e)
        {
            // Tự động map dựa trên tên giống nhau
            foreach (var mapping in MappingList)
            {
                if (_systemFields.Contains(mapping.ExcelColumn))
                {
                    mapping.MappedField = mapping.ExcelColumn;
                }
                else
                {
                    // Thử tìm mapping tương đối
                    var match = _systemFields.FirstOrDefault(f => f.ToLower() == mapping.ExcelColumn.ToLower());
                    if (match != null)
                    {
                        mapping.MappedField = match;
                    }
                }
            }
        }

        private void BtnChapNhan_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
