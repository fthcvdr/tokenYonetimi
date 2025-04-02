using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TokenManagementExample
{
    public class TokenManager
    {
        private string token;
        private DateTime tokenExpirationTime;
        private DateTime lastTokenRequestTime;
        private int tokenRequestCount = 0;
        private const int TokenRequestLimit = 5; // Saatlik istek sınırı
        private DateTime tokenRequestResetTime;

        public TokenManager()
        {
            tokenRequestResetTime = DateTime.Today.AddHours(1); // Reset time her saatin başı
        }

        public async Task<string> GetTokenAsync()
        {
            // Token süresi dolmuşsa ya da hiç token yoksa yeni token al
            if (string.IsNullOrEmpty(token) || DateTime.Now >= tokenExpirationTime)
            {
                // Token isteği sınırını kontrol et
                if (tokenRequestCount >= TokenRequestLimit)
                {
                    // Sınıra ulaşmışsan, bir saat bekle
                    var waitTime = tokenRequestResetTime - DateTime.Now;
                    Console.WriteLine($"Sınır aşıldı, {waitTime.TotalSeconds} saniye bekleniyor...");
                    await Task.Delay(waitTime);  // Sınıra ulaşmışsan bekle
                    tokenRequestCount = 0;  // Sayaç sıfırlanacak
                    tokenRequestResetTime = DateTime.Today.AddHours(1);
                }

                return await GetNewTokenAsync();
            }
            else
            {
                return token;
            }
        }

        private async Task<string> GetNewTokenAsync()
        {
            using (var client = new HttpClient())
            {
                var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", "your_client_id"),
                    new KeyValuePair<string, string>("client_secret", "your_client_secret")
                });

                var response = await client.PostAsync("https://api.example.com/token", requestContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var tokenData = JsonConvert.DeserializeObject<TokenResponse>(responseBody);
                    token = tokenData.AccessToken;
                    tokenExpirationTime = DateTime.Now.AddSeconds(tokenData.ExpiresIn);
                    lastTokenRequestTime = DateTime.Now;
                    tokenRequestCount++;
                    return token;
                }
                else
                {
                    throw new Exception("Token alırken hata oluştu!");
                }
            }
        }

        public class TokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }
            
            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }
        }
    }

    public class OrderService
    {
        private readonly TokenManager tokenManager;

        public OrderService()
        {
            tokenManager = new TokenManager();
        }

        public async Task GetOrdersAsync()
        {
            // Token al
            var token = await tokenManager.GetTokenAsync();

            // Sipariş listesi almak için API'ye isteği gönder
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await client.GetAsync("https://api.example.com/orders");

                if (response.IsSuccessStatusCode)
                {
                    var orders = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Siparişler: " + orders);
                }
                else
                {
                    Console.WriteLine("Siparişler alınırken hata oluştu!");
                }
            }
        }
    }

    // Kullanıcı giriş noktası
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var orderService = new OrderService();
            await orderService.GetOrdersAsync();
        }
    }
}
