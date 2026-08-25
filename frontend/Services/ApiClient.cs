using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyBar.Client.Services
{
    public static class ApiClient
    {
        private static readonly HttpClient _httpClient;

        static ApiClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5000/api/")
            };
            // Thêm header Accept JSON mặc định
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Gắn Token vào Header trước khi gọi API (nếu đã có Token)
        /// </summary>
        private static void AttachToken()
        {
            if (SessionContext.IsLoggedIn)
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", SessionContext.CurrentToken);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public static async Task<T> GetAsync<T>(string endpoint)
        {
            AttachToken();
            var response = await _httpClient.GetAsync(endpoint);
            return await ProcessResponse<T>(response);
        }

        public static async Task<T> PostAsync<T>(string endpoint, object data = null)
        {
            AttachToken();
            var content = CreateJsonContent(data);
            var response = await _httpClient.PostAsync(endpoint, content);
            return await ProcessResponse<T>(response);
        }

        public static async Task<T> PutAsync<T>(string endpoint, object data = null)
        {
            AttachToken();
            var content = CreateJsonContent(data);
            var response = await _httpClient.PutAsync(endpoint, content);
            return await ProcessResponse<T>(response);
        }

        public static async Task<T> DeleteAsync<T>(string endpoint)
        {
            AttachToken();
            var response = await _httpClient.DeleteAsync(endpoint);
            return await ProcessResponse<T>(response);
        }

        private static HttpContent CreateJsonContent(object data)
        {
            if (data == null) return null;
            var json = JsonConvert.SerializeObject(data);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private static async Task<T> ProcessResponse<T>(HttpResponseMessage response)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                // Thử bóc tách lỗi từ JSON (nếu có cấu trúc { success: false, message: '...' })
                try
                {
                    var errorObj = JsonConvert.DeserializeAnonymousType(responseString, new { message = "" });
                    if (!string.IsNullOrEmpty(errorObj?.message))
                    {
                        throw new Exception(errorObj.message);
                    }
                }
                catch (JsonException) { /* Bỏ qua nếu không phải JSON */ }
                
                throw new Exception($"Lỗi từ Server ({response.StatusCode}): {responseString}");
            }

            return JsonConvert.DeserializeObject<T>(responseString);
        }
    }
}
