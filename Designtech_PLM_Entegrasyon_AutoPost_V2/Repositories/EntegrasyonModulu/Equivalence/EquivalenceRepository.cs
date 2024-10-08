using Dapper;
using Designtech_PLM_Entegrasyon_AutoPost.ApiServices;
using Designtech_PLM_Entegrasyon_AutoPost.Helper;
using Designtech_PLM_Entegrasyon_AutoPost.Model.WindchillApiModel;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModulu.Equivalence;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Model.Entity;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Humanizer.On;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.EntegrasyonModulu.Equivalence
{
	public class EquivalenceRepository : IEquivalenceService
	{
		

		public async Task getEquivalenceData(IConfiguration _configuration, IDbConnection conn, string catalogValue, string apiFullUrl, string apiURL, string sourceApi, string endPoint)
		{
			try
			{

		
			var SQL_Equivalence = $"SELECT * FROM {catalogValue}.Des_EquivalenceLink_LogTable WHERE [EntegrasyonDurum] = 1";

			var responseData = await conn.QueryAsync<dynamic>(SQL_Equivalence);

			var dataList = responseData.ToList();

			

			if (responseData != null)
			{
				//Parallel.ForEach(dataList, item =>
				//{ });

					foreach ( var item in dataList ) {
					var SQL_WTPart = $"SELECT * FROM {catalogValue}.Des_WTPart_LogTable WHERE [ParcaPartID] = {item.idA3A5} OR [ParcaPartID] = {item.idA3B5}";
					var responseDataWTPart = await conn.QueryAsync<dynamic>(SQL_WTPart);
					if (responseDataWTPart.Count() == 0)
					{
						var equPart = new EquAnaParca
						{
							Number = item.ApartWtNumber,

							// equEsParcallar'ı bir liste olarak başlatıyoruz
							Alternates = new List<EquEsParcallar>
					{
						 new EquEsParcallar
						{
							AlternatePart = new EquEsParca
							{
								Number = item.BpartWtNumber,
								isCancel = false
							}
						}
					}
						};
						// API servisi oluşturuluyor
						var jsonData2 = JsonConvert.SerializeObject(equPart);
						ApiService _apiService = new ApiService();
						dynamic dataResponse = null;
							try
							{

							
						dataResponse = _apiService.PostDataAsync(apiFullUrl, apiURL, endPoint, jsonData2, jsonData2);
						LogService logService = new LogService(_configuration);
						logService.CreateJsonFileLog(jsonData2, $"Ana parça: {item.ApartWtNumber} - Eş parça: {item.BpartWtNumber} arasın da Equivalence bağlantısı kuruldu. " + dataResponse?.message);

						var deleteQuery = $"DELETE FROM {catalogValue}.Des_EquivalenceLink_LogTable WHERE ID = @ID";
						await conn.ExecuteAsync(deleteQuery, new { ID = item.ID });
							}
							catch (Exception ex)
							{
								// API çağrısında hata olduğunda, hatalı veriyi 'Des_AlternateLink_LogTable_Error' tablosuna ekleyin.
								var insertErrorQuery = $@"
                            INSERT INTO {catalogValue}.Des_EquivalenceLink_LogTable_Error
                            SELECT * FROM {catalogValue}.Des_EquivalenceLink_LogTable WHERE LogID = @LogID";

								await conn.ExecuteAsync(insertErrorQuery, new { LogID = item.LogID });




								var deleteQuery = $"DELETE FROM {catalogValue}.Des_EquivalenceLink_LogTable WHERE LogID = @LogID";
								await conn.ExecuteAsync(deleteQuery, new { LogID = item.LogID });

								// Hata kaydını loglayın
								LogService logService = new LogService(_configuration);
								logService.CreateJsonFileLog(jsonData2, $"API çağrısı başarısız oldu: {ex.Message}. Hatalı veri hata tablosuna aktarıldı." + dataResponse.message);

							}
						}
				}
			}
		


			}
			catch (Exception ex)
			{

			}

		}
	
	}

	public class EquAnaParca:BaseEntity
	{
		public string Number { get; set; }
		public List<EquEsParcallar> Alternates { get; set; }
	}
	public class EquEsParcallar
	{
		public EquEsParca AlternatePart { get; set; }
	}
	public class EquEsParca:BaseEntity
	{
		[Key]
		public string? Number { get; set; }
		public bool isCancel { get; set; }
	}
}
