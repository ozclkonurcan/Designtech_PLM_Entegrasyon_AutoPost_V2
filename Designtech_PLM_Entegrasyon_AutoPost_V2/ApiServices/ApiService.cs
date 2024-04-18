using Azure;
using Designtech_PLM_Entegrasyon_AutoPost.Helper;
using Microsoft.Extensions.Configuration;
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

    public class ApiService
    {

        private readonly IConfiguration _configuration;
        public async Task<string> PostDataAsync(string apiFullUrl, string apiURL, string endpoint,string jsonContent)
        {
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
                        VeriDepo.JsonVeriListesi.Add("");
                        return await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        // Günlük için yanıt içeriğini kaydet
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Hata Yanıt İçeriği: {errorContent}");
                        throw new HttpRequestException($"HTTP isteği, durum kodu {response.StatusCode} ile başarısız oldu");
                    }
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                LogService logService = new LogService(_configuration);
                logService.CreateJsonFileLog(jsonContent,ex.Message.ToString() + "Parça gönderilmedi - (API istek sınıfı, beklenen formatla uyuşmuyor. Lütfen kontrol edin!)" + apiFullUrl+"/"+endpoint);
                    MessageBox.Show("API istek sınıfı, beklenen formatla uyuşmuyor. Lütfen kontrol edin!");
                throw;
            }
            catch (Exception ex)
            {
                var message = ex is ArgumentException ? ex.Message : "Beklenmeyen bir hata oluştu";
                LogService logService = new LogService(_configuration);
                logService.CreateJsonFileLog(jsonContent, "HATA " + "Parça gönderilmedi - ('"+message+"') - " + apiFullUrl +"/"+ endpoint);
                MessageBox.Show($"Hata: {message}");
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
