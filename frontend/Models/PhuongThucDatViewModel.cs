using System.Collections.ObjectModel;

namespace QuanLyBar.Client.Models
{
    public class PhuongThucDatViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ParentId { get; set; }
        public string ParentDir { get; set; }
        public string SortOrder { get; set; }
        
        public ObservableCollection<PhuongThucDatViewModel> Children { get; set; } = new ObservableCollection<PhuongThucDatViewModel>();
    }
}
