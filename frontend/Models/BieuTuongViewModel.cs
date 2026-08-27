using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace QuanLyBar.Client.Models
{
    public class BieuTuongViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public byte[] Anh { get; set; }
        public BitmapImage ImageSource { get; set; }
        
        // Children for grouping
        public ObservableCollection<BieuTuongViewModel> Children { get; set; } = new ObservableCollection<BieuTuongViewModel>();
    }
}
