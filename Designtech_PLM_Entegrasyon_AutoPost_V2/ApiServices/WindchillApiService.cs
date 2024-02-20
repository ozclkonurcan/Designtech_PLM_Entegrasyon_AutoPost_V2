using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost.ApiServices
{
	public class WindchillApiService
	{


		//public async Task GetApiToken()
		//{
		//http://192.168.1.11/Windchill/servlet/odata/PTC/GetCSRFToken()
		//}


		public async Task<string> GetApiData(string baseUrl, string endpoint,string username, string password,string CSRF_NONCE)
		{
			int maxRetryCount = 3; // Maksimum deneme sayısı
			int retryDelayMilliseconds = 1000; // Tekrar deneme aralığı (milisaniye cinsinden)
			//TimeSpan timeout = TimeSpan.FromSeconds(60); // İstek için zaman aşımı süresi

			for (int retryCount = 0; retryCount < maxRetryCount; retryCount++)
			{
				try
				{
					using (var client = new HttpClient())
					{
						client.Timeout = Timeout.InfiniteTimeSpan;
						var request = new HttpRequestMessage(HttpMethod.Get, $"http://{baseUrl}/Windchill/servlet/odata/{endpoint}");

						//request.Headers.Add("CSRF_NONCE", "qWhBSqh2RWM43KBJy1ktOPFAcgcI78YF+hxwAvxPEldU79p6kTggHcA7CjsBktgCmA91e8ZdMl4C7Zd5m1t0c5hHcFEM6Zp8+DgxefgnFQlV7c0h4ik2EN8sN0h6meYChj0jc+oOHQgXsuUCyB12EMkjeA==");

						request.Headers.Add("CSRF_NONCE", CSRF_NONCE);
						string authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
						request.Headers.Add("Authorization", $"Basic {authHeader}");
						//request.Headers.Add("Authorization", "Basic UExNLTE6RGVzLjIzIVRlY2g=");

						var response = await client.SendAsync(request);
						response.EnsureSuccessStatusCode();

						return await response.Content.ReadAsStringAsync();
					}
				}
				catch (HttpRequestException ex) when (ex.InnerException is TimeoutException)
				{
					// Zaman aşımı hatası
					Console.WriteLine($"Zaman aşımı hatası. Deneme {retryCount + 1}/{maxRetryCount}...");

					// Belirli bir süre bekleyip tekrar deneme
					Thread.Sleep(retryDelayMilliseconds);
				}
				catch (Exception ex)
				{
					// Diğer hatalar
					return ex.Message;
				}
			}

			return "Başarılı bir cevap alınamadı.";
		}




		//public async Task PostApiData()
		//{
		//}


		//public async Task PatchApiData()
		//{
		//}



		#region GetApiEski
		//public async Task<string> GetApiData(string baseUrl, string endpoint)
		//{


		//	try
		//	{

		//		using (var client = new HttpClient())
		//		{
		//			client.Timeout = Timeout.InfiniteTimeSpan;
		//			var request = new HttpRequestMessage(HttpMethod.Get, $"http://{baseUrl}/Windchill/servlet/odata/{endpoint}");

		//			request.Headers.Add("CSRF_NONCE", "qWhBSqh2RWM43KBJy1ktOPFAcgcI78YF+hxwAvxPEldU79p6kTggHcA7CjsBktgCmA91e8ZdMl4C7Zd5m1t0c5hHcFEM6Zp8+DgxefgnFQlV7c0h4ik2EN8sN0h6meYChj0jc+oOHQgXsuUCyB12EMkjeA==");
		//			request.Headers.Add("Authorization", "Basic UExNLTE6RGVzLjIzIVRlY2g=");

		//			//var content = new StringContent("{\r\n\"NoOfFiles\":1\r\n}", null, "application/json");
		//			//request.Content = content;


		//			var response = await client.SendAsync(request);
		//			response.EnsureSuccessStatusCode();

		//			return await response.Content.ReadAsStringAsync();
		//		}


		//	}
		//	catch (Exception ex)
		//	{
		//		return ex.Message;
		//	}
		//}

		#endregion


	}
}
