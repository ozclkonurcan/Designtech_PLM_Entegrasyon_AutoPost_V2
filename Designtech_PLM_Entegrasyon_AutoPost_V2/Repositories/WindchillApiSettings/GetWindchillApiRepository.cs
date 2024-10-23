using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.WindchillApiSettings;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.WindchillApiSettings
{
	public class GetWindchillApiRepository : IGetWindchillApiServices
	{

		private readonly string _csrfNonce;
		private readonly string _windchillServerName;
		private readonly string _basicUsername;
		private readonly string _basicPassword;


		public GetWindchillApiRepository()
		{
			var directoryPath = "Configuration";
			var fileName = "appsettings.json";
			var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);

			var jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;

			if (!string.IsNullOrEmpty(jsonData))
			{
				JObject jsonObject = JObject.Parse(jsonData);

				// Bilgileri değişkenlere atıyoruz
				_csrfNonce = jsonObject["APIConnectionINFO"]["CSRF_NONCE"]?.ToString();
				_windchillServerName = jsonObject["APIConnectionINFO"]["WindchillServerName"]?.ToString();
				_basicUsername = jsonObject["APIConnectionINFO"]["Username"]?.ToString();
				_basicPassword = jsonObject["APIConnectionINFO"]["Password"]?.ToString();
			}
			else
			{
				throw new FileNotFoundException("appsettings.json file not found.");
			}
		}

		public async Task<string> GetApiData(string endPoint)
		{
			const int maxRetryCount = 3;
			const int retryDelayMilliseconds = 1000;

			for (int retryCount = 0; retryCount < maxRetryCount; retryCount++)
			{
				try
				{
					using (var client = new HttpClient())
					{
						client.Timeout = TimeSpan.FromSeconds(10);
						client.DefaultRequestHeaders.ConnectionClose = true;
						var request = new HttpRequestMessage(HttpMethod.Get, $"http://{_windchillServerName}/Windchill/servlet/odata/{endPoint}");

						request.Headers.Add("CSRF_NONCE", _csrfNonce);
						string authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_basicUsername}:{_basicPassword}"));
						request.Headers.Add("Authorization", $"Basic {authHeader}");

						var response = await client.SendAsync(request);
						response.EnsureSuccessStatusCode();

						var content = await response.Content.ReadAsStringAsync();
						return content; // Direkt string olarak döndür 
					}
				}
				catch (HttpRequestException ex) when (ex.InnerException is TimeoutException)
				{
					// Zaman aşımı hatası
					// Belirli bir süre bekleyip tekrar deneme
					Thread.Sleep(retryDelayMilliseconds);
				}
				catch (Exception ex)
				{
					throw;
				}
			}

			// Burada, tüm denemeler başarısız olursa bir hata fırlatın
			throw new Exception("API isteği başarısız oldu.");
		}
		public async Task<string> GetApiToken()
		{
			throw new NotImplementedException();
		}
	}
}
