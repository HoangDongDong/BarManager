namespace QuanLyBar.Client.Models
{
    public class UserProfile
    {
        public string Id { get; set; }
        public string TenDangNhap { get; set; }
        public string VaiTro { get; set; }
    }

    public class AuthResponseData
    {
        public UserProfile User { get; set; }
        public string Token { get; set; }
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public AuthResponseData Data { get; set; }
    }
}
