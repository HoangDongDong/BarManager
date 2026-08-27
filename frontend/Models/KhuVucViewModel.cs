using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace QuanLyBar.Client.Models
{
    public class KhuVucViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ParentId { get; set; }
        public string ParentDir { get; set; }
        public string SortOrder { get; set; }
        public bool? Status { get; set; }
        
        public bool IsExpanded { get; set; }
        public bool IsSelected { get; set; }
        
        public System.Windows.Media.Imaging.BitmapImage ImageSource { get; set; }
        
        // Children for TreeView
        public ObservableCollection<KhuVucViewModel> Children { get; set; } = new ObservableCollection<KhuVucViewModel>();
    }
}
