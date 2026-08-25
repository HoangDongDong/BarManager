using System;
using System.Collections.ObjectModel;

namespace QuanLyBar.Client.Models
{
    public class NhomMatHangViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ParentId { get; set; }
        public string ParentDir { get; set; }
        public string SortOrder { get; set; }
        public bool? Status { get; set; }
        
        // Children for TreeView
        public ObservableCollection<NhomMatHangViewModel> Children { get; set; } = new ObservableCollection<NhomMatHangViewModel>();

        // Để hiển thị đẹp hơn nếu cần
        public bool IsExpanded { get; set; } = true;
    }
}
