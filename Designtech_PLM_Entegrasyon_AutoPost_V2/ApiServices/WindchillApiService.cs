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
	
		public async Task<string> GetApiData(string baseUrl, string endpoint)
		{
			

			try
			{

				using (var client = new HttpClient())
				{
					var request = new HttpRequestMessage(HttpMethod.Get, $"http://{baseUrl}/Windchill/servlet/odata/{endpoint}");

					request.Headers.Add("CSRF_NONCE", "qWhBSqh2RWM43KBJy1ktOPFAcgcI78YF+hxwAvxPEldU79p6kTggHcA7CjsBktgCmA91e8ZdMl4C7Zd5m1t0c5hHcFEM6Zp8+DgxefgnFQlV7c0h4ik2EN8sN0h6meYChj0jc+oOHQgXsuUCyB12EMkjeA==");
					request.Headers.Add("Authorization", "Basic UExNLTE6RGVzLjIzIVRlY2g=");
			
					//var content = new StringContent("{\r\n\"NoOfFiles\":1\r\n}", null, "application/json");
					//request.Content = content;


					var response = await client.SendAsync(request);
					response.EnsureSuccessStatusCode();

					return await response.Content.ReadAsStringAsync();
				}


			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}
	}
}
