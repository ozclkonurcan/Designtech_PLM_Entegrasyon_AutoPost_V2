using Dapper;
using Designtech_PLM_Entegrasyon_AutoPost.ApiServices;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModulu.EntegrasyonAyar.EntegrasyonDurum;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Model.WindchillApiModel;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.EntegrasyonModulu.EntegrasyonAyar.EntegrasyonDurum
{
	using Dapper;
	using Microsoft.Extensions.Configuration;
	using System.Data.SqlClient;
	using System.Text.Json;
	using Newtonsoft.Json.Linq;
	using static Google.Cloud.Vision.V1.ProductSearchResults.Types;

	public class EntegrasyonDurumRepository : IEntegrasyonDurumService
	{

		#region UPDATE

		public async Task EntegrasyonDurumUpdate(string state, long KodidA2A2)
		{
			try
			{
				WindchillApiService windchillApiService = new WindchillApiService();

				string directoryPath = "Configuration";
				string fileName = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);



				if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath)))
				{
					Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath));
				}



				// (�nceki kodlar burada)

				string jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
				JObject jsonObject = JObject.Parse(jsonData);
				var catalogValue = jsonObject["DatabaseSchema"].ToString();
				var connectionString = jsonObject["ConnectionStrings"]["Plm"].ToString();
				var conn = new SqlConnection(connectionString);
				var CSRF_NONCE = jsonObject["APIConnectionINFO"]["CSRF_NONCE"].ToString();

				var ServerName = jsonObject["ServerName"].ToString();
				var WindchillServerName = jsonObject["APIConnectionINFO"]["WindchillServerName"].ToString();
				var BasicUsername = jsonObject["APIConnectionINFO"]["Username"].ToString();
				var BasicPassword = jsonObject["APIConnectionINFO"]["Password"].ToString();


				// Windchill API token'ını al
				WrsToken apiToken = await windchillApiService.GetApiToken(WindchillServerName, BasicUsername, BasicPassword);

				// Entegrasyon Durumu ve Entegrasyon Tarihi tanımlarını al
				var stringDefinitionId = await GetStringDefinitionId(conn, catalogValue, "Entegrasyon Durumu");
				var timestampDefinitionId = await GetTimestampDefinitionId(conn, catalogValue, "Entegrasyon Tarihi");

				// Mevcut değeri kontrol et
				var existingRecord = await conn.QueryFirstOrDefaultAsync(
					$"SELECT * FROM [{catalogValue}].[StringValue] WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = @stringDefinitionId",
					new { KodidA2A2, stringDefinitionId });
				var existingRecordTimeStamp = await conn.QueryFirstOrDefaultAsync(
				   $"SELECT * FROM [{catalogValue}].[TimestampValue] WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = @timestampDefinitionId",
				   new { KodidA2A2, timestampDefinitionId });

				// Duruma göre değerleri ayarla
				string value = "";
				string value2 = "";
				DateTime? controlDate2 = null;

				if (state == "RELEASED")
				{
					value = "PARCA ENTEGRE OLDU";
					value2 = "Parca entegre oldu";
					controlDate2 = DateTime.UtcNow;
				}
				else if (state == "CANCELLED")
				{
					value = "PARCA İPTAL OLDU";
					value2 = "Parca iptal oldu";
					controlDate2 = DateTime.UtcNow;
				}
				else if (state == "INWORK")
				{
					value = "";
					value2 = "";
					controlDate2 = DateTime.UtcNow;
				}

				// StringValue tablosunu güncelle veya ekle
				await UpdateOrInsertStringValue(conn, catalogValue, KodidA2A2, stringDefinitionId, value, value2, existingRecord);

				// TimestampValue tablosunu güncelle veya ekle
				await UpdateOrInsertTimestampValue(conn, catalogValue, KodidA2A2, timestampDefinitionId, controlDate2, existingRecordTimeStamp);
			}
			catch (System.Exception ex)
			{
				// Hata işleme
				Console.WriteLine(ex.Message);
			}
		}

		// StringDefinition ID'sini al
		private async Task<long> GetStringDefinitionId(SqlConnection conn, string catalogValue, string displayName)
		{
			return await conn.QueryFirstOrDefaultAsync<long>(
				$"SELECT [idA2A2] FROM [{catalogValue}].[StringDefinition] WHERE [displayName] = @displayName",
				new { displayName });
		}

		// TimestampDefinition ID'sini al
		private async Task<long> GetTimestampDefinitionId(SqlConnection conn, string catalogValue, string displayName)
		{
			return await conn.QueryFirstOrDefaultAsync<long>(
				$"SELECT [idA2A2] FROM [{catalogValue}].[TimestampDefinition] WHERE [displayName] = @displayName",
				new { displayName });
		}

		// StringValue tablosunu güncelle veya ekle
		private async Task UpdateOrInsertStringValue(SqlConnection conn, string catalogValue, long KodidA2A2, long stringDefinitionId, string value, string value2, dynamic existingRecord)
		{
			if (existingRecord != null)
			{
				// Güncelle
				await conn.ExecuteAsync(
					$"UPDATE [{catalogValue}].[StringValue] " +
					"SET " +
					"[value] = @value, " +
					"[value2] = @value2 " +
					"WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = @stringDefinitionId",
					new { value, value2, KodidA2A2, stringDefinitionId });
			}
			else
			{
				var IdSeq = $"SELECT TOP 1 [value] FROM {catalogValue}.id_sequence ORDER BY [value] DESC";
				var resolvedIdSeq = await conn.QueryFirstOrDefaultAsync<dynamic>(IdSeq);
				long newId = Convert.ToInt64(resolvedIdSeq.value) + 100;

				// Ekle
				//long newId = await GetNextIdSequence(conn, catalogValue);
				int result = await conn.ExecuteAsync(
					$"INSERT INTO [{catalogValue}].[StringValue] " +
					"([hierarchyIDA6], [idA2A2], [idA3A4], [classnameA2A2], [idA3A5], [idA3A6], [markForDeleteA2], [modifyStampA2], [updateCountA2], [updateStampA2], [createStampA2], [classnamekeyA4], [classnamekeyA6], [value], [value2]) " +
					"VALUES ('7058085483721066086', @newId, @KodidA2A2, 'wt.iba.value.StringValue', 0, @stringDefinitionId, 0, @modifyStampA2, 1, @updateStampA2, @createStampA2, 'wt.part.WTPart', 'wt.iba.definition.StringDefinition', @value, @value2)",
					new { newId, KodidA2A2, stringDefinitionId, modifyStampA2 = DateTime.Now.Date, updateStampA2 = DateTime.Now.Date, createStampA2 = DateTime.Now.Date, value, value2 });
				if (result == 1)
				{
					await conn.ExecuteAsync(
						$"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
				}
			}
		}

		// TimestampValue tablosunu güncelle veya ekle
		private async Task UpdateOrInsertTimestampValue(SqlConnection conn, string catalogValue, long KodidA2A2, long timestampDefinitionId, DateTime? value, dynamic existingRecordTimeStamp)
		{
			if (existingRecordTimeStamp != null)
			{
				// Güncelle
				await conn.ExecuteAsync(
					$"UPDATE [{catalogValue}].[TimestampValue] " +
					"SET " +
					"[value] = @value " +
					"WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = @timestampDefinitionId",
					new { value, KodidA2A2, timestampDefinitionId });
			}
			else if (value.HasValue)
			{
				// Ekle (sadece değer null değilse)
				var IdSeq = $"SELECT TOP 1 [value] FROM {catalogValue}.id_sequence ORDER BY [value] DESC";
				var resolvedIdSeq = await conn.QueryFirstOrDefaultAsync<dynamic>(IdSeq);
				long newId = Convert.ToInt64(resolvedIdSeq.value) + 100;
				//long newId = await GetNextIdSequence(conn, catalogValue);
				int result = await conn.ExecuteAsync(
					$"INSERT INTO [{catalogValue}].[TimestampValue] " +
					"([hierarchyIDA6], [idA2A2], [idA3A4], [classnameA2A2], [idA3A5], [idA3A6], [markForDeleteA2], [modifyStampA2], [updateCountA2], [updateStampA2], [createStampA2], [classnamekeyA4], [classnamekeyA6], [value]) " +
					"VALUES ('-148878178526147486', @newId, @KodidA2A2, 'wt.iba.value.TimestampValue', 0, @timestampDefinitionId, 0, @modifyStampA2, 1, @updateStampA2, @createStampA2, 'wt.part.WTPart', 'wt.iba.definition.TimestampDefinition', @value)",
					new { newId, KodidA2A2, timestampDefinitionId, modifyStampA2 = DateTime.Now.Date, updateStampA2 = DateTime.Now.Date, createStampA2 = DateTime.Now.Date, value });

				if (result == 1)
				{
					await conn.ExecuteAsync(
						$"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
				}
			}
		}

		// Sıradaki ID değerini al
		private async Task<long> GetNextIdSequence(SqlConnection conn, string catalogValue)
		{
			return await conn.QueryFirstOrDefaultAsync<long>($"SELECT NEXT VALUE FOR [{catalogValue}].[id_sequence]");
		}
		#endregion

		#region HataDurumUpdate

		public async Task EntegrasyonHataDurumUpdate(long KodidA2A2)
		{
			try
			{

				WindchillApiService windchillApiService = new WindchillApiService();

				string directoryPath = "Configuration";
				string fileName = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);



				if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath)))
				{
					Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath));
				}



				// (�nceki kodlar burada)

				string jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
				JObject jsonObject = JObject.Parse(jsonData);
				var catalogValue = jsonObject["DatabaseSchema"].ToString();
				var connectionString = jsonObject["ConnectionStrings"]["Plm"].ToString();
				var conn = new SqlConnection(connectionString);
				var CSRF_NONCE = jsonObject["APIConnectionINFO"]["CSRF_NONCE"].ToString();

				var ServerName = jsonObject["ServerName"].ToString();
				var WindchillServerName = jsonObject["APIConnectionINFO"]["WindchillServerName"].ToString();
				var BasicUsername = jsonObject["APIConnectionINFO"]["Username"].ToString();
				var BasicPassword = jsonObject["APIConnectionINFO"]["Password"].ToString();

				WrsToken apiToken = await windchillApiService.GetApiToken(WindchillServerName, BasicUsername, BasicPassword);

				//var sql = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2] FROM {catalogValue}.WTPart WHERE [statestate] = 'RELEASED' and [latestiterationInfo] = 1 and statecheckoutInfo = 'wrk'";
				//var resolvedItems = await conn.QueryFirstAsync<dynamic>(sql);


				var IdSeq = $"SELECT [value] FROM {catalogValue}.id_sequence ORDER BY [value] DESC";
				var resolvedIdSeq = await conn.QueryFirstOrDefaultAsync<dynamic>(IdSeq);
				long respIdSeq = Convert.ToInt64(resolvedIdSeq.value) + 100;

				var message = "";
				var existingRecordStringDefinitionSeq = $"SELECT * FROM [{catalogValue}].[StringDefinition] WHERE [displayName] = 'Entegrasyon Durumu'";
				var existingRecordStringDefinition = await conn.QueryFirstOrDefaultAsync<dynamic>(existingRecordStringDefinitionSeq);

				var existingRecordTimestampDefinitionSeq = $"SELECT * FROM [{catalogValue}].[TimestampDefinition] WHERE [displayName] = 'Entegrasyon Tarihi'";
				var existingRecordTimestampDefinition = await conn.QueryFirstOrDefaultAsync<dynamic>(existingRecordTimestampDefinitionSeq);



				var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
				var content = $"{{\r\n  \"EntegrasyonDurumu\": \"Parca entegre edilemedi\",\r\n  \"EntegrasyonTarihi\": \"{currentDate}\"\r\n}}";








				// �ncelikle, mevcut kayd� kontrol edin
				var existingRecord = await conn.QueryFirstOrDefaultAsync(
			 $"SELECT * FROM [{catalogValue}].[StringValue] WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordStringDefinition.idA2A2}",
			 new { KodidA2A2 });
				var existingRecordTimeStamp = await conn.QueryFirstOrDefaultAsync(
			   $"SELECT * FROM [{catalogValue}].[TimestampValue] WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordTimestampDefinition.idA2A2}",
			   new { KodidA2A2 });




				if (existingRecord != null)
				{
					// Mevcut bir kay�t varsa, g�ncelleme i�lemi yap�n
					int result = await conn.ExecuteAsync(
						$"UPDATE [{catalogValue}].[StringValue] " +
						"SET " +
						"[hierarchyIDA6] = @hierarchyIDA6, " +
						"[idA2A2] = @idA2A2, " +
						"[classnameA2A2] = @classnameA2A2, " +
						"[idA3A5] = @idA3A5, " +
						"[idA3A6] = @idA3A6, " +
						"[markForDeleteA2] = @markForDeleteA2, " +
						"[modifyStampA2] = @modifyStampA2, " +
						"[updateCountA2] = @updateCountA2, " +
						"[updateStampA2] = @updateStampA2, " +
						"[createStampA2] = @createStampA2, " +
						"[classnamekeyA4] = @classnamekeyA4, " +
						"[classnamekeyA6] = @classnamekeyA6, " +
						"[value] = @value, " +
						"[value2] = @value2 " +
						$"WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordStringDefinition.idA2A2}",
						new
						{
							hierarchyIDA6 = "7058085483721066086",
							idA2A2 = respIdSeq,
							classnameA2A2 = "wt.iba.value.StringValue",
							idA3A5 = 0,
							idA3A6 = Convert.ToInt64(existingRecordStringDefinition.idA2A2),
							markForDeleteA2 = 0,
							modifyStampA2 = DateTime.Now.Date,
							updateCountA2 = 1,
							updateStampA2 = DateTime.Now.Date,
							createStampA2 = DateTime.Now.Date,
							classnamekeyA4 = "wt.part.WTPart",
							classnamekeyA6 = "wt.iba.definition.StringDefinition",
							value = "PARCA ENTEGRE EDILEMEDI",
							value2 = "Parca entegre edilemedi",
							KodidA2A2
						});




					if (result == 1)
					{
						await conn.ExecuteAsync(
							$"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
					}
				}
				else
				{
					// Mevcut bir kay�t yoksa, yeni bir kay�t ekleyin
					int result = await conn.ExecuteAsync(
						$"INSERT INTO [{catalogValue}].[StringValue] " +
						"([hierarchyIDA6], [idA2A2], [idA3A4], [classnameA2A2], [idA3A5], [idA3A6], [markForDeleteA2], [modifyStampA2], [updateCountA2], [updateStampA2], [createStampA2], [classnamekeyA4], [classnamekeyA6], [value], [value2]) " +
						"VALUES (@hierarchyIDA6, @idA2A2, @idA3A4, @classnameA2A2, @idA3A5, @idA3A6, @markForDeleteA2, @modifyStampA2, @updateCountA2, @updateStampA2, @createStampA2, @classnamekeyA4, @classnamekeyA6, @value, @value2)",
						new
						{
							hierarchyIDA6 = "7058085483721066086",
							idA2A2 = Convert.ToInt64(resolvedIdSeq.value) + 100,
							idA3A4 = Convert.ToInt64(KodidA2A2),
							classnameA2A2 = "wt.iba.value.StringValue",
							idA3A5 = 0,
							idA3A6 = Convert.ToInt64(existingRecordStringDefinition.idA2A2),
							markForDeleteA2 = 0,
							modifyStampA2 = DateTime.Now.Date,
							updateCountA2 = 1,
							updateStampA2 = DateTime.Now.Date,
							createStampA2 = DateTime.Now.Date,
							classnamekeyA4 = "wt.part.WTPart",
							classnamekeyA6 = "wt.iba.definition.StringDefinition",
							value = "PARCA ENTEGRE EDILEMEDI",
							value2 = "Parca entegre edilemedi"
						});





					if (result == 1)
					{
						await conn.ExecuteAsync(
							$"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
					}
				}

				var controlDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
				DateTime controlDate2 = Convert.ToDateTime(controlDate);
				if (existingRecordTimeStamp != null)
				{


					// TimestampValue tablosunu g�ncelleyin
					int result = await conn.ExecuteAsync(
						$"UPDATE [{catalogValue}].[TimestampValue] " +
						"SET " +
						"[hierarchyIDA6] = @hierarchyIDA6, " +
						"[idA2A2] = @idA2A2, " +
						"[classnameA2A2] = @classnameA2A2, " +
						"[idA3A5] = @idA3A5, " +
						"[idA3A6] = @idA3A6, " +
						"[markForDeleteA2] = @markForDeleteA2, " +
						"[modifyStampA2] = @modifyStampA2, " +
						"[updateCountA2] = @updateCountA2, " +
						"[updateStampA2] = @updateStampA2, " +
						"[createStampA2] = @createStampA2, " +
						"[classnamekeyA4] = @classnamekeyA4, " +
						"[classnamekeyA6] = @classnamekeyA6, " +
						"[value] = @value " +
						$"WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordTimestampDefinition.idA2A2}",
						new
						{
							hierarchyIDA6 = "-148878178526147486",
							idA2A2 = Convert.ToInt64(resolvedIdSeq.value) + 100,
							classnameA2A2 = "wt.iba.value.TimestampValue",
							idA3A5 = 0,
							idA3A6 = Convert.ToInt64(existingRecordTimestampDefinition.idA2A2),
							markForDeleteA2 = 0,
							modifyStampA2 = DateTime.Now.Date,
							updateCountA2 = 1,
							updateStampA2 = DateTime.Now.Date,
							createStampA2 = DateTime.Now.Date,
							classnamekeyA4 = "wt.part.WTPart",
							classnamekeyA6 = "wt.iba.definition.TimestampDefinition",
							value = controlDate2,
							KodidA2A2
						});

					// G�ncelleme i�lemi ba�ar�l�ysa, id_sequence tablosuna dummy = 'x' �eklinde ekleme i�lemi ger�ekle�tirin
					if (result == 1)
					{
						await conn.ExecuteAsync(
							$"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
					}
				}
				else
				{


					int result = await conn.ExecuteAsync(
$"INSERT INTO [{catalogValue}].[TimestampValue] " +
"([hierarchyIDA6], [idA2A2], [idA3A4], [classnameA2A2], [idA3A5], [idA3A6], [markForDeleteA2], [modifyStampA2], [updateCountA2], [updateStampA2], [createStampA2], [classnamekeyA4], [classnamekeyA6], [value]) " +
"VALUES (@hierarchyIDA6, @idA2A2, @idA3A4, @classnameA2A2, @idA3A5, @idA3A6, @markForDeleteA2, @modifyStampA2, @updateCountA2, @updateStampA2, @createStampA2, @classnamekeyA4, @classnamekeyA6, @value)",
new
{
	hierarchyIDA6 = "-148878178526147486",
	idA2A2 = Convert.ToInt64(resolvedIdSeq.value) + 100,
	idA3A4 = Convert.ToInt64(KodidA2A2),
	classnameA2A2 = "wt.iba.value.TimestampValue",
	idA3A5 = 0,
	idA3A6 = Convert.ToInt64(existingRecordTimestampDefinition.idA2A2),
	markForDeleteA2 = 0,
	modifyStampA2 = DateTime.Now.Date,
	updateCountA2 = 1,
	updateStampA2 = DateTime.Now.Date,
	createStampA2 = DateTime.Now.Date,
	classnamekeyA4 = "wt.part.WTPart",
	classnamekeyA6 = "wt.iba.definition.TimestampDefinition",
	value = controlDate2,

});


					// Yeni ekleme i�lemi ba�ar�l�ysa, id_sequence tablosuna dummy = 'x' �eklinde ekleme i�lemi ger�ekle�tirin
					if (result == 1)
					{
						await conn.ExecuteAsync(
							$"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
					}
				}




			}
			catch (Exception)
			{

			}

		}
		#endregion

		#region RESET

		public async Task EntegrasyonDurumRESET(string state, long KodidA2A2)
		{
			try
			{
				// Configuration dosyasının yolunu oluşturun
				string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration");
				string filePath = Path.Combine(directoryPath, "appsettings.json");

				// Konfigürasyon dosyasını okuyun ve gerekli bilgileri alın
				if (!Directory.Exists(directoryPath))
				{
					Directory.CreateDirectory(directoryPath);
				}

				string jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
				JObject jsonObject = JObject.Parse(jsonData);

				string catalogValue = jsonObject["DatabaseSchema"].ToString();
				string connectionString = jsonObject["ConnectionStrings"]["Plm"].ToString();
				string CSRF_NONCE = jsonObject["APIConnectionINFO"]["CSRF_NONCE"].ToString();
				string ServerName = jsonObject["ServerName"].ToString();
				string WindchillServerName = jsonObject["APIConnectionINFO"]["WindchillServerName"].ToString();
				string BasicUsername = jsonObject["APIConnectionINFO"]["Username"].ToString();
				string BasicPassword = jsonObject["APIConnectionINFO"]["Password"].ToString();

				// Windchill API servisine bağlanın
				WindchillApiService windchillApiService = new WindchillApiService();
				WrsToken apiToken = await windchillApiService.GetApiToken(WindchillServerName, BasicUsername, BasicPassword);

				// Veritabanı bağlantısını oluşturun
				using (var conn = new SqlConnection(connectionString))
				{
					// Entegrasyon Durumu ve Entegrasyon Tarihi tanımlarını alın
					var existingRecordStringDefinition = await conn.QueryFirstOrDefaultAsync<dynamic>($@"
                SELECT * 
                FROM [{catalogValue}].[StringDefinition] 
                WHERE [displayName] = 'Entegrasyon Durumu'");

					var existingRecordTimestampDefinition = await conn.QueryFirstOrDefaultAsync<dynamic>($@"
                SELECT * 
                FROM [{catalogValue}].[TimestampDefinition] 
                WHERE [displayName] = 'Entegrasyon Tarihi'");

					// Son id_sequence değerini alın ve yeni bir değer oluşturun
					var resolvedIdSeq = await conn.QueryFirstOrDefaultAsync<dynamic>($@"
                SELECT TOP 1 [value]
                FROM {catalogValue}.id_sequence 
                ORDER BY [value] DESC");

					long respIdSeq = Convert.ToInt64(resolvedIdSeq.value) + 100;

					// Sadece state "INWORK" ise işlem yapın
					if (state == "INWORK")
					{
						// Mevcut StringValue ve TimestampValue kayıtlarını silin
						await conn.ExecuteAsync($@"
                    DELETE FROM [{catalogValue}].[StringValue] 
                    WHERE [idA3A4] = @KodidA2A2 
                        AND [idA3A6] = @StringDefinitionId",
							new { KodidA2A2, StringDefinitionId = existingRecordStringDefinition.idA2A2 });

						await conn.ExecuteAsync($@"
                    DELETE FROM [{catalogValue}].[TimestampValue] 
                    WHERE [idA3A4] = @KodidA2A2 
                        AND [idA3A6] = @TimestampDefinitionId",
							new { KodidA2A2, TimestampDefinitionId = existingRecordTimestampDefinition.idA2A2 });
					}
				}

				// Windchill API'sini kullanarak EntegrasyonDurumu değerini güncelleyin
				// (Önceki kodda yorum satırı olarak işaretlenmişti)
				// await windchillApiService.EntegrasyonDurumUpdateAPI(ServerName, "ProdMgmt/Parts('OR:wt.part.WTPart:" + resolvedItems.idA2A2 + "')/PTC.ProdMgmt.CheckOut", BasicUsername, BasicPassword, apiToken.NonceValue, "{\r\n  \"EntegrasyonDurumu\": \"Entegre oldu1\"\r\n}");
			}
			catch (System.Exception ex)
			{
				// Hata yönetimi ekleyin
				// Örneğin:
				// Loglama
				// Console.WriteLine($"Hata oluştu: {ex.Message}");
				// throw; // Hatayı yeniden fırlatın veya işleyin
			}
		}

	#endregion

	}
}
