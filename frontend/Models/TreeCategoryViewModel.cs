using System.Collections.ObjectModel;

namespace QuanLyBar.Client.Models
{
    public class TreeCategoryViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ParentId { get; set; }
        public string ParentDir { get; set; }
        public string SortOrder { get; set; }
        
        public string Note { get; set; }
        public string SimageId { get; set; }
        public System.Windows.Media.ImageSource ImageSource { get; set; }
        
        public ObservableCollection<TreeCategoryViewModel> Children { get; set; } = new ObservableCollection<TreeCategoryViewModel>();
    }
}
