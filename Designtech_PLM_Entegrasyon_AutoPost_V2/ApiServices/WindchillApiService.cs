using Designtech_PLM_Entegrasyon_AutoPost_V2.Model.WindchillApiModel;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Designtech_PLM_Entegrasyon_AutoPost.ApiServices
{
	public class WindchillApiService
	{
		private static readonly HttpClient client = new HttpClient();



		public async Task<string> GetApiVeriTasimaWTDoc(string baseUrl, string endpoint, string username, string password, string CSRF_NONCE)
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
						var request = new HttpRequestMessage(HttpMethod.Get, $"http://{baseUrl}/Windchill/servlet/rest/{endpoint}");

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


		public async Task<string> GetApiData(string baseUrl, string endpoint, string username, string password, string CSRF_NONCE)
		{
			int maxRetryCount = 5; // Maksimum deneme sayısı
			int retryDelayMilliseconds = 1000; // Tekrar deneme aralığı (milisaniye cinsinden)
											   //TimeSpan timeout = TimeSpan.FromSeconds(60); // İstek için zaman aşımı süresi

			for (int retryCount = 0; retryCount < maxRetryCount; retryCount++)
			{
				try
				{
					using (var client = new HttpClient())
					{
						client.Timeout = TimeSpan.FromSeconds(5);
						client.DefaultRequestHeaders.ConnectionClose = true;
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

					// Belirli bir süre bekleyip tekrar deneme
					Thread.Sleep(retryDelayMilliseconds);
				}
				catch (Exception ex)
				{
				}
			}


			return "Başarılı bir cevap alınamadı.";
		}



		public async Task<WrsToken> GetApiToken(string baseUrl, string username, string password)
        {
            int maxRetryCount = 3; // Maksimum deneme sayısı
            int retryDelayMilliseconds = 1000; // Tekrar deneme aralığı (milisaniye cinsinden)

            for (int retryCount = 0; retryCount < maxRetryCount; retryCount++)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = Timeout.InfiniteTimeSpan;
                        var request = new HttpRequestMessage(HttpMethod.Get, $"http://{baseUrl}/Windchill/servlet/odata/PTC/GetCSRFToken()");

                        string authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                        request.Headers.Add("Authorization", $"Basic {authHeader}");

                        var response = await client.SendAsync(request);
                        response.EnsureSuccessStatusCode();

                        var json = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<WrsToken>(json);
                    }
                }
                catch (HttpRequestException ex) when (ex.InnerException is TimeoutException)
                {
                    // Zaman aşımı hatası
                    Console.WriteLine($"Zaman aşımı hatası. Deneme {retryCount + 1}/{maxRetryCount}...");

                    // Belirli bir süre bekleyip tekrar deneme
                    await Task.Delay(retryDelayMilliseconds);
                }
                catch (Exception ex)
                {
                    // Diğer hatalar
                    throw new Exception(ex.Message);
                }
            }

            throw new Exception("Başarılı bir cevap alınamadı.");
        }





		#region WTDocumentPostAttachmentsSettings

		public async Task<string> WTDoc_ChekcOut(string baseUrl, string endpoint, string username, string password, string CSRF_NONCE)
		{


			try
			{
				using (var client = new HttpClient())
				{
					client.Timeout = Timeout.InfiniteTimeSpan;
					var request = new HttpRequestMessage(HttpMethod.Post, $"http://{baseUrl}/Windchill/servlet/odata/{endpoint}");


					request.Headers.Add("CSRF_NONCE", CSRF_NONCE);
					string authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
					request.Headers.Add("Authorization", $"Basic {authHeader}");

					var content = new StringContent("{\r\n  \"CheckOutNote\": \"\"\r\n}", null, "application/json");
					request.Content = content;


					var response = await client.SendAsync(request);
					response.EnsureSuccessStatusCode();
					return await response.Content.ReadAsStringAsync();
				}
			}
			catch (HttpRequestException ex) when (ex.InnerException is TimeoutException)
			{
				// Zaman aşımı hatası
				Console.WriteLine($"Zaman aşımı hatası. Deneme....");


			}
			catch (Exception ex)
			{
				// Diğer hatalar
				return ex.Message;
			}


			return "Başarılı bir cevap alınamadı.";
		}


		public async Task<string> WTDoc_PostData(string baseUrl, string endpoint, string username, string password, string CSRF_NONCE, string jsonContent)
		{
		
			
				try
				{
					using (var client = new HttpClient())
					{
						client.Timeout = Timeout.InfiniteTimeSpan;
						var request = new HttpRequestMessage(HttpMethod.Post, $"http://{baseUrl}/Windchill/servlet/odata/{endpoint}");


						request.Headers.Add("CSRF_NONCE", CSRF_NONCE);
						string authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
						request.Headers.Add("Authorization", $"Basic {authHeader}");

						var content = new StringContent(jsonContent.ToString(), null, "application/json");
						request.Content = content;


						var response = await client.SendAsync(request);
						response.EnsureSuccessStatusCode();
						return await response.Content.ReadAsStringAsync();
					}
				}
				catch (HttpRequestException ex) when (ex.InnerException is TimeoutException)
				{
					// Zaman aşımı hatası
					Console.WriteLine($"Zaman aşımı hatası. Deneme....");

				
				}
				catch (Exception ex)
				{
					// Diğer hatalar
					return ex.Message;
				}
			

			return "Başarılı bir cevap alınamadı.";
		}

		public async Task<string> WTDoc_ReplicaUrlPostData(string ReplicaUrl, string FileNames, int StreamIds, string baseUrl, string username, string password, string CSRF_NONCE, string fileContentBase64, string AttachFileName)
		{
			string masterUrl = "http://plm-1.designtech.com/Windchill/servlet/WindchillGW";
			byte[] fileBytes = Convert.FromBase64String(fileContentBase64);
			// ANSI'den UTF-8'e dönüştürme
			Encoding ansiEncoding = Encoding.GetEncoding(1252); // ANSI kodlaması
			string fileContentAnsi = ansiEncoding.GetString(fileBytes);
			byte[] utf8Bytes = Encoding.UTF8.GetBytes(fileContentAnsi);


			// Uygulama kök dizininde WTDocumentSettingsFolder klasörünü oluştur
			string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WTDocumentSettingsFolder");

			// Klasör yoksa oluştur
			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}


			// Dosyanın tam yolu
			string filePath = Path.Combine(folderPath, AttachFileName);
			try
			{


				string sBoundary = "---------------------------boundary";


				await File.WriteAllBytesAsync(filePath, utf8Bytes);

				var options = new RestClientOptions("http://plm-1.designtech.com")
				{
					MaxTimeout = -1,
				};
				var client = new RestClient(options);
				var request = new RestRequest(ReplicaUrl, Method.Post);
				request.AddHeader("Content-Type", "multipart/form-data"+ sBoundary);
				request.AddHeader("CSRF_NONCE", CSRF_NONCE);
				string authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
				request.AddHeader("Authorization", $"Basic {authHeader}");
				request.AlwaysMultipartFormData = true;
				request.AddParameter("Master_URL", "http://plm-1.designtech.com/Windchill/servlet/WindchillGW");
				request.AddParameter("CacheDescriptor_array", StreamIds+":"+ FileNames + ":"+ StreamIds);
				request.AddFile(FileNames, filePath);
				RestResponse response = await client.ExecuteAsync(request);

				return response.Content.ToString();
			}
			catch (HttpRequestException ex) when (ex.InnerException is TimeoutException)
			{
				// Timeout error
				Console.WriteLine("Timeout error. Retrying...");
				return "Timeout error.";
			}
			catch (Exception ex)
			{
				// Other errors
				return ex.Message;
			}
			finally
			{
				if (File.Exists(filePath))
				{
					File.Delete(filePath); // Geçici dosyayı sil
				}
			}
		}



		public async Task<string> WTDoc_ReplicaUrlPostData2(string ReplicaUrl, string FileNames, int StreamIds, string baseUrl, string username, string password, string CSRF_NONCE, string fileContentBase64, string AttachFileName,string masterUrl)
		{
			byte[] fileBytes = Convert.FromBase64String(fileContentBase64);
			// ANSI'den UTF-8'e dönüştürme
			//Encoding ansiEncoding = Encoding.GetEncoding(1254); 
			//string fileContentAnsi = ansiEncoding.GetString(fileBytes);
			//byte[] utf8Bytes = Encoding.UTF8.GetBytes(fileContentAnsi);



			string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WTDocumentSettingsFolder");

			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}

		

			string filePath = Path.Combine(folderPath, AttachFileName);

			try
			{
				await File.WriteAllBytesAsync(filePath, fileBytes);
				//await File.WriteAllBytesAsync(filePath, utf8Bytes);


				using (HttpClient client = new HttpClient())
				{
					HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, ReplicaUrl);
					request.Headers.Add("CSRF_NONCE", CSRF_NONCE);
					string authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
					request.Headers.Add("Authorization", $"Basic {authHeader}");

					string sBoundary = "---------------------------boundary";
					var content = new MultipartFormDataContent(sBoundary);

					var Master_URL_Content = new StringContent(masterUrl);
					Master_URL_Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
					{
						Name = "\"Master_URL\""
					};
					content.Add(Master_URL_Content);

					string lstStreamIds = $"{StreamIds}:{FileNames}:{StreamIds}";
					var CacheDescriptor_array_Content = new StringContent(lstStreamIds);
					CacheDescriptor_array_Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
					{
						Name = "\"CacheDescriptor_array\""
					};
					content.Add(CacheDescriptor_array_Content);





					//string sContentType = "text/plain";
					string sContentType = "application/pdf"; 

					using (FileStream fileStream = System.IO.File.OpenRead(filePath))
					{
						var streamContent = new StreamContent(fileStream);
						streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
						{
							
							Name = FileNames,
							FileName = AttachFileName
						};
						streamContent.Headers.ContentType = new MediaTypeHeaderValue(sContentType);

						content.Headers.Remove("Content-Type");
						content.Headers.Add("Content-Type", "multipart/form-data; boundary=" + sBoundary);
						content.Add(streamContent);

						request.Content = content;

						HttpResponseMessage response = await client.SendAsync(request);
						string responseContent = await response.Content.ReadAsStringAsync();
						return responseContent;
					}
				}
			}
			catch (Exception)
			{
				return "";
			}
			finally
			{
				if (File.Exists(filePath))
				{
					File.Delete(filePath); // Geçici dosyayı sil
				}
			}
		}






		public async Task<string> WTDoc_Delete(string baseUrl, string endpoint, string username, string password, string CSRF_NONCE)
		{


			try
			{
				using (var client = new HttpClient())
				{
					var request = new HttpRequestMessage(HttpMethod.Delete, $"http://{baseUrl}/Windchill/servlet/odata/{endpoint}");


					request.Headers.Add("CSRF_NONCE", CSRF_NONCE);
					string authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
					request.Headers.Add("Authorization", $"Basic {authHeader}");

					var content = new StringContent("\n\n\n\n", null, "application/json");
					request.Content = content;


					var response = await client.SendAsync(request);
					response.EnsureSuccessStatusCode();
					return await response.Content.ReadAsStringAsync();
				}
			}
			catch (Exception ex)
			{
				// Diğer hatalar
				return ex.Message;
			}

		}


		#endregion



		#region Entegrasyon-Api-Ayarları
		public async Task<string> EntegrasyonDurumAPI(string baseUrl, string endpoint, string username, string password, string CSRF_NONCE,string jsonContent)
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
                        var request = new HttpRequestMessage(HttpMethod.Post, $"http://{baseUrl}/Windchill/servlet/odata/{endpoint}");


                        request.Headers.Add("CSRF_NONCE", CSRF_NONCE);
                        string authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                        request.Headers.Add("Authorization", $"Basic {authHeader}");

                        var content = new StringContent(jsonContent.ToString(), null, "application/json");
                        request.Content = content;


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
        public async Task<string> EntegrasyonDurumUpdateAPI(string baseUrl, string endpoint, string username, string password, string CSRF_NONCE, string jsonContent,string contentDate)
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
                        var request = new HttpRequestMessage(HttpMethod.Patch, $"http://{baseUrl}/Windchill/servlet/odata/{endpoint}");

                        //request.Headers.Add("CSRF_NONCE", "qWhBSqh2RWM43KBJy1ktOPFAcgcI78YF+hxwAvxPEldU79p6kTggHcA7CjsBktgCmA91e8ZdMl4C7Zd5m1t0c5hHcFEM6Zp8+DgxefgnFQlV7c0h4ik2EN8sN0h6meYChj0jc+oOHQgXsuUCyB12EMkjeA==");

                        request.Headers.Add("CSRF_NONCE", CSRF_NONCE);
                        string authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                        request.Headers.Add("Authorization", $"Basic {authHeader}");

                        //var content = new StringContent($"{{\r\n  \"EntegrasyonDurumu\": \"{jsonContent}\",\r\n  \"EntegrasyonTarihi\": \"{contentDate}\"\r\n}}", null, "application/json");
                        var content = new StringContent($"{{\r\n  \"EntegrasyonDurumu\": \"{jsonContent}\",\r\n  \"EntegrasyonTarihi\": \"{contentDate}\"\r\n}}", null, "application/json");
                        request.Content = content;


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
        #endregion



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
