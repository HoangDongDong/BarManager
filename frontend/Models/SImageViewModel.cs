using System.Windows.Media;

namespace QuanLyBar.Client.Models
{
    public class SImageViewModel
    {
        public int Id { get; set; }
        public byte[] ImageBytes { get; set; }
        public ImageSource ImageSource { get; set; }
    }
}
