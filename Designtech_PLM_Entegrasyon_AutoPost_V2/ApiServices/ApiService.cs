using Azure;
using Designtech_PLM_Entegrasyon_AutoPost.Helper;
using Designtech_PLM_Entegrasyon_AutoPost.Model.WindchillApiModel;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Designtech_PLM_Entegrasyon_AutoPost.ApiServices
{

    public class VeriDepo
    {
        public static List<string> JsonVeriListesi { get; set; } = new List<string>();
    }

    public class HataliResponse
    {
        public static List<string> JsonVeriListesi { get; set; } = new List<string>();
    }

    public class ApiErrorResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
        // Diğer gerekli özellikler
    }
    

    public class ApiService
    {
        private async Task<ApiErrorResponse> DeserializeResponse(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiErrorResponse>(content); // Json.NET kullanılıyor.
        }


        private readonly IConfiguration _configuration;

        private readonly ConcurrentQueue<ApiIstekBilgisi> _hataKuyrugu = new ConcurrentQueue<ApiIstekBilgisi>();
        private readonly System.Threading.Timer _timer;

        //public ApiService(IConfiguration configuration)
        //{
        //    _configuration = configuration;

        //    // Her saat başı çalışacak zamanlayıcıyı başlat
        //    _timer = new System.Threading.Timer(KuyruguIsle, null, TimeSpan.Zero, TimeSpan.FromHours(1));
        //}

        public async Task<ApiErrorResponse> PostDataAsync(string apiFullUrl, string apiURL, string endpoint,string jsonContent,string LogJsonContent)
        {
                var errorContent = "";
            try
            {


				using (var client = new HttpClient())
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    //var request = new HttpRequestMessage(HttpMethod.Post, $"{apiURL}/{endpoint}");
                    var request = new HttpRequestMessage(HttpMethod.Post, $"{apiFullUrl}/{endpoint}");
                    var content = new StringContent(jsonContent.ToString(), Encoding.UTF8, "application/json");
                    request.Content = content;

                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var apiResponse = await DeserializeResponse(response);
                        VeriDepo.JsonVeriListesi.Add("");

                        if (apiResponse.success == true)
                        {
                            VeriDepo.JsonVeriListesi.Add("");

                            //LogService logService = new LogService(_configuration);
                            //logService.CreateJsonFileLog(jsonContent, "CAD Döküman bilgileri gönderildi."+ apiResponse.message);

                            return apiResponse;

                        }
                        else
                        {
                            // Başarısız durum işleme alınacak
                            errorContent =$"API başarısız yanıt: {apiResponse.message}";
                            throw new HttpRequestException($"API başarısız yanıt: {apiResponse.message}");
                        }
                    }
                    else
                    {
                        // Hata durumunda veriyi kuyruğa ekle
                        var istekBilgisi = new ApiIstekBilgisi
                        {
                            ApiFullUrl = apiFullUrl,
                            ApiURL = apiURL,
                            Endpoint = endpoint,
                            JsonContent = jsonContent,
                            LogJsonContent = LogJsonContent
                        };
                        _hataKuyrugu.Enqueue(istekBilgisi);
                        // Günlük için yanıt içeriğini kaydet
                        errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Hata Yanıt İçeriği: {errorContent}");
                        
						//MessageBox.Show($"HTTP isteği, durum kodu {response.StatusCode} ile başarısız oldu. Hata Mesajı : {errorContent}", "API HATASI", MessageBoxButtons.OK, MessageBoxIcon.Information);
						throw new HttpRequestException($"HTTP isteği, durum kodu {response.StatusCode} ile başarısız oldu");
                    }
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                var istekBilgisi = new ApiIstekBilgisi
                {
                    ApiFullUrl = apiFullUrl,
                    ApiURL = apiURL,
                    Endpoint = endpoint,
                    JsonContent = jsonContent,
                    LogJsonContent = LogJsonContent
                };
                _hataKuyrugu.Enqueue(istekBilgisi);
                LogService logService = new LogService(_configuration);
                logService.CreateJsonFileLogError(LogJsonContent, ex.Message.ToString() + "Parça gönderilmedi - (API istek sınıfı, beklenen formatla uyuşmuyor. Lütfen kontrol edin!)" + apiFullUrl+"/"+endpoint);
				//MessageBox.Show($"API istek sınıfı, beklenen formatla uyuşmuyor. Lütfen kontrol edin!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                throw;
            }
            catch (Exception ex)
            {
                var istekBilgisi = new ApiIstekBilgisi
                {
                    ApiFullUrl = apiFullUrl,
                    ApiURL = apiURL,
                    Endpoint = endpoint,
                    JsonContent = jsonContent,
                    LogJsonContent = LogJsonContent
                };
                _hataKuyrugu.Enqueue(istekBilgisi);
                var message = ex is ArgumentException ? ex.Message :" Hata mesajı : "+ ex.Message;
                LogService logService = new LogService(_configuration);
                logService.CreateJsonFileLogError(LogJsonContent, "Parça gönderilmedi  - UYGULAMA HATASI - ('"+ message +"') - API HATASI ('"+ errorContent + "') - " + apiFullUrl +"/"+ endpoint);
				//MessageBox.Show($"Hata:  {message} ", "UYGULAMA HATASI", MessageBoxButtons.OK, MessageBoxIcon.Information);

				throw;
            }
        }
        private async void KuyruguIsle(object state)
        {
            while (_hataKuyrugu.TryDequeue(out var istekVerisi))
            {
                if (istekVerisi is ApiIstekBilgisi istekBilgisi)
                {
                    try
                {
                    // jsonData'yı ayrıştır ve gerekli bilgileri al
                    //var dataObject = JsonConvert.DeserializeObject<dynamic>(jsonData);
                    //string apiFullUrl = dataObject.ApiFullUrl; // Örneğin, jsonData'dan apiFullUrl'yi al
                    //string endpoint = dataObject.Endpoint;     // Örneğin, jsonData'dan endpoint'i al

                        // API'ye tekrar gönder
                        var response = await PostDataAsync(
                          istekBilgisi.ApiFullUrl,
                          istekBilgisi.ApiURL,
                          istekBilgisi.Endpoint,
                          istekBilgisi.JsonContent,
                          istekBilgisi.LogJsonContent
                      );

                        // Başarılı ise bir işlem yapmayın (kuyruktan zaten çıkarıldı)
                    }
                catch (Exception)
                {
                    // Hata durumunda jsonData'yı tekrar kuyruğa ekleyin
                    _hataKuyrugu.Enqueue(istekVerisi);

                    // Hata kaydı oluşturabilir veya başka bir işlem yapabilirsiniz
                }
                }
            }
        }
        //İnternet bağlantısnın kontrolünü yapıyoruz burada.
        bool IsConnectedToInternet()
        {
            using (var ping = new Ping())
            {
                var reply = ping.Send("www.google.com", 1000);
                return reply.Status == IPStatus.Success;
            }
        }

        //public async Task<string> PostDataAsync(string apiURL,string endpoint, string jsonContent)
        //{
        //    // Windows servisinde ortam adını belirleme
        //    //var apiUrl = ConfigurationManager.AppSettings["ApiUrl"];

        //    try
        //    {


        //    using (var client = new HttpClient())
        //    {
        //        var request = new HttpRequestMessage(HttpMethod.Post, $"{apiURL}/{endpoint}");
        //        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        //        request.Content = content;

        //        var response = await client.SendAsync(request);
        //        response.EnsureSuccessStatusCode();

        //        VeriDepo.JsonVeriListesi.Add("");

        //        return await response.Content.ReadAsStringAsync();
        //    }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("HATA!" + ex.Message);
        //        throw;
        //    }
        //}




    }
    public class ApiIstekBilgisi
    {
        public string ApiFullUrl { get; set; }
        public string ApiURL { get; set; }
        public string Endpoint { get; set; }
        public string JsonContent { get; set; }
        public string LogJsonContent { get; set; }
    }
}
