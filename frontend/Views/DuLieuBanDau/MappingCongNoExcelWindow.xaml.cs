using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace QuanLyBar.Client.Views.DuLieuBanDau
{
    public class CongNoColumnMappingItem : INotifyPropertyChanged
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

    public partial class MappingCongNoExcelWindow : Window
    {
        public ObservableCollection<CongNoColumnMappingItem> MappingList { get; set; }

        private readonly List<string> _systemFields = new List<string>
        {
            "",
            "Mã đối tác",
            "Công nợ đầu"
        };

        public MappingCongNoExcelWindow(List<string> excelColumns)
        {
            InitializeComponent();

            MappingList = new ObservableCollection<CongNoColumnMappingItem>();
            foreach (var col in excelColumns)
            {
                MappingList.Add(new CongNoColumnMappingItem
                {
                    ExcelColumn = col,
                    AvailableFields = _systemFields,
                    MappedField = ""
                });
            }

            DgMapping.ItemsSource = MappingList;
            AutoMap();
        }

        private void AutoMap()
        {
            foreach (var item in MappingList)
            {
                string c = item.ExcelColumn?.Trim().ToLower() ?? "";
                if (c == "mã" || c == "ma" || c.Contains("mã") || c.Contains("ma") || c.Contains("code") || c.Contains("mã đối tác") || c.Contains("mã khách"))
                {
                    item.MappedField = "Mã đối tác";
                }
                else if (c.Contains("công nợ đầu") || c.Contains("cong no dau") || c.Contains("công nợ") || c.Contains("cong no") || c.Contains("tiền") || c.Contains("tien") || c.Contains("số tiền") || c.Contains("so tien") || c.Contains("nợ"))
                {
                    item.MappedField = "Công nợ đầu";
                }
                else
                {
                    item.MappedField = "";
                }
            }
        }

        private void BtnTuDongChon_Click(object sender, RoutedEventArgs e)
        {
            AutoMap();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
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
