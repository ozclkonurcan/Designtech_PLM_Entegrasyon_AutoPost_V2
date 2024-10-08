using Dapper;
using Designtech_PLM_Entegrasyon_AutoPost.ApiServices;
using Designtech_PLM_Entegrasyon_AutoPost.Helper;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModuluError.Equivalence;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Model.Entity;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.EntegrasyonModulu.Equivalence;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.EntegrasyonModuluError.Equivalence
{
	public class ErrorEquivalenceRepository : IErrorEquivalenceService
	{
		public async Task geErrorEquivalenceData(IConfiguration _configuration, IDbConnection conn, string catalogValue, string apiFullUrl, string apiURL, string sourceApi, string endPoint)
		{
			try
			{


				var SQL_Equivalence = $"SELECT * FROM {catalogValue}.Des_EquivalenceLink_LogTable_Error WHERE [EntegrasyonDurum] = 1";

				var responseData = await conn.QueryAsync<dynamic>(SQL_Equivalence);

				var dataList = responseData.ToList();



				if (responseData != null)
				{
					//Parallel.ForEach(dataList, item =>
					//{ });

					foreach (var item in dataList)
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

								var deleteQuery = $"DELETE FROM {catalogValue}.Des_EquivalenceLink_LogTable_Error WHERE ID = @ID";
								await conn.ExecuteAsync(deleteQuery, new { ID = item.ID });
							}
							catch (Exception ex)
							{
								
								var deleteQuery = $"DELETE FROM {catalogValue}.Des_EquivalenceLink_LogTable_Error WHERE LogID = @LogID";
								await conn.ExecuteAsync(deleteQuery, new { LogID = item.LogID });


							}
						
					}
				}



			}
			catch (Exception ex)
			{

			}

		}
	}



	public class EquAnaParca : BaseEntity
	{
		public string Number { get; set; }
		public List<EquEsParcallar> Alternates { get; set; }
	}
	public class EquEsParcallar
	{
		public EquEsParca AlternatePart { get; set; }
	}
	public class EquEsParca : BaseEntity
	{
		[Key]
		public string? Number { get; set; }
		public bool isCancel { get; set; }
	}
}
