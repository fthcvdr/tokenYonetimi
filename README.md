TokenManager Sınıfı:
GetTokenAsync: Token'ı kontrol eder. Eğer mevcut token geçerli değilse ya da süresi dolmuşsa yeni token alır. Ayrıca token isteği sınırını kontrol eder ve gerektiğinde bekler.
GetNewTokenAsync: Yeni bir token almak için API'ye istek atar. Dönen yanıtı işleyerek token bilgilerini saklar.
OrderService Sınıfı:
GetOrdersAsync: Token'ı aldıktan sonra siparişler için API'ye istek gönderir. Bu işlemde HttpClient ile API'ye istek yapılır ve yanıt alınır.
Program Sınıfı:
Main: Asenkron olarak OrderService sınıfının GetOrdersAsync metodunu çalıştırır.