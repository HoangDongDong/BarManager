using QuanLyBar.Client.Models;

namespace QuanLyBar.Client
{
    public static class SessionContext
    {
        public static string CurrentToken { get; set; }
        public static UserProfile CurrentUser { get; set; }

        public static bool IsLoggedIn => !string.IsNullOrEmpty(CurrentToken);

        public static void Clear()
        {
            CurrentToken = null;
            CurrentUser = null;
        }
    }
}
