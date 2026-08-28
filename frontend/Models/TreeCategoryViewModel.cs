using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuanLyBar.Client.Models
{
    public class TreeCategoryViewModel : INotifyPropertyChanged
    {
        private string _name;
        private bool _isEditing;

        public string Id { get; set; }
        
        public string Name 
        { 
            get => _name; 
            set 
            { 
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            } 
        }

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ParentId { get; set; }
        public string ParentDir { get; set; }
        public string SortOrder { get; set; }
        
        public string Note { get; set; }
        public string SimageId { get; set; }
        public System.Windows.Media.ImageSource ImageSource { get; set; }
        
        public ObservableCollection<TreeCategoryViewModel> Children { get; set; } = new ObservableCollection<TreeCategoryViewModel>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
