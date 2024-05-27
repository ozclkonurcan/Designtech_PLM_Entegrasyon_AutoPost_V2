using Azure;
using Designtech_PLM_Entegrasyon_AutoPost.Helper;
using Designtech_PLM_Entegrasyon_AutoPost.Model.WindchillApiModel;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
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
        public async Task<string> PostDataAsync(string apiFullUrl, string apiURL, string endpoint,string jsonContent,string LogJsonContent)
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
                            return await response.Content.ReadAsStringAsync();

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
                LogService logService = new LogService(_configuration);
                logService.CreateJsonFileLogError(LogJsonContent, ex.Message.ToString() + "Parça gönderilmedi - (API istek sınıfı, beklenen formatla uyuşmuyor. Lütfen kontrol edin!)" + apiFullUrl+"/"+endpoint);
				//MessageBox.Show($"API istek sınıfı, beklenen formatla uyuşmuyor. Lütfen kontrol edin!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                throw;
            }
            catch (Exception ex)
            {
                var message = ex is ArgumentException ? ex.Message :" Hata mesajı : "+ ex.Message;
                LogService logService = new LogService(_configuration);
                logService.CreateJsonFileLogError(LogJsonContent, "Parça gönderilmedi  - UYGULAMA HATASI - ('"+ message +"') - API HATASI ('"+ errorContent + "') - " + apiFullUrl +"/"+ endpoint);
				//MessageBox.Show($"Hata:  {message} ", "UYGULAMA HATASI", MessageBoxButtons.OK, MessageBoxIcon.Information);

				throw;
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
}
