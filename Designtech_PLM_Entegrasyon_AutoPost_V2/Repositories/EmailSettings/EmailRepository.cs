using Designtech_PLM_Entegrasyon_AutoPost.Model.WindchillApiModel;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EmailSettings;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.EmailSettings
{
	public class EmailRepository :IEmailService
	{
		public async Task EmailControlString(WTUsers emailControlString)
		{
			try
			{

				string directoryPath = "Configuration";
				string fileName = "EmailController.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);

				// Klasör yoksa oluştur
				if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath)))
				{
					Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath));
				}

				// Dosya varsa oku
				string jsonData = "";
				if (System.IO.File.Exists(filePath))
				{
					jsonData = File.ReadAllText(filePath);
				}

				// JSON verisini kontrol et ve gerekirse düzenle
				JObject jsonObject;
				if (string.IsNullOrWhiteSpace(jsonData))
				{
					// JSON verisi boşsa yeni bir nesne oluştur
					jsonObject = new JObject
					{
						["FromEmail"] = "",
						["FromEmailPassword"] = "",
						["PortNumber"] = "",
						["EmmilClearSettings"] = new JObject
						{
							["ClearHours"] = 1,
						},

						["EmailList"] = new JArray()
					};
				}
				else
				{
					// JSON verisi mevcutsa, onu bir nesneye çevir
					jsonObject = JObject.Parse(jsonData);
				}

				// EmailList dizisine kullanıcıları ekle
				JArray emailList = (JArray)jsonObject["EmailList"];
				foreach (var user in emailControlString.Users)
				{
					var existingUser = emailList.FirstOrDefault(u => (string)u["ID"] == user.ID);
					if (existingUser != null)
					{
						existingUser["Name"] = user.Name;
						existingUser["EMail"] = user.EMail;
						existingUser["FullName"] = user.FullName;
					}
					else
					{
						// Aynı ID'ye sahip bir kullanıcı yoksa, yeni kullanıcı olarak ekle
						JObject newUser = new JObject
						{
							["ID"] = user.ID,
							["Name"] = user.Name,
							["EMail"] = user.EMail,
							["FullName"] = user.FullName,
							["Durum"] = false // Varsayılan olarak Durum false olarak ayarlanabilir
						};
						emailList.Add(newUser);
					}


				}

				// JSON dosyasındaki kullanıcıları kontrol et ve eşleşmeyenleri kaldır
				foreach (var email in emailList.ToList())
				{
					// JSON'da olan bir kullanıcı ID'si, API'den gelen kullanıcılar listesinde yoksa, kaldır
					if (!emailControlString.Users.Any(u => u.ID == (string)email["ID"]))
					{
						email.Remove();
					}
				}

				// JSON nesnesini dosyaya geri yaz
				File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonObject, Formatting.Indented));


			}
			catch (Exception ex)
			{
				MessageBox.Show("HATA !", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

		}

	}
}
