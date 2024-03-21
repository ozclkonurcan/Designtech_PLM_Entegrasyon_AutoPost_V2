using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"HTTP isteği hatası: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Beklenmeyen bir hata oluştu: {ex.Message}");
                throw;
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
