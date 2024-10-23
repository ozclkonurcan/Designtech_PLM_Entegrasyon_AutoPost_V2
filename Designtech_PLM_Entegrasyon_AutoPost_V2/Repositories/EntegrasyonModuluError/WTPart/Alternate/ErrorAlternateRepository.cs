using Dapper;
using Designtech_PLM_Entegrasyon_AutoPost.ApiServices;
using Designtech_PLM_Entegrasyon_AutoPost.Helper;
using Designtech_PLM_Entegrasyon_AutoPost.Model.WindchillApiModel;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModuluError.WTPart.Alternate;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.WindchillApiSettings;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Model.Entity;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.EntegrasyonModuluError.WTPart.Alternate
{
	public class ErrorAlternateRepository : IErrorAlternateService
	{
		private readonly IGetWindchillApiServices _getWindchillApiServices;

		public ErrorAlternateRepository(IGetWindchillApiServices getWindchillApiServices)
		{
			_getWindchillApiServices = getWindchillApiServices;
		}

		public async Task getErrorAlternateData(IConfiguration _configuration, IDbConnection conn, string catalogValue, string apiFullUrl, string apiURL, string sourceApi, string endPoint)
		{
			try
			{


				var SQL_Alternate = $"SELECT * FROM {catalogValue}.Des_AlternateLink_LogTable_Error WHERE [EntegrasyonDurum] = 1";

				var responseData = await conn.QueryAsync<dynamic>(SQL_Alternate);

				var dataList = responseData.ToList();






				if (responseData != null)
				{
					foreach (var item in dataList)
					{

						var json = await _getWindchillApiServices.GetApiData($"ProdMgmt/Parts('OR:wt.part.WTPart:{item.AnaParcaPartID}')?$expand=Alternates($expand=AlternatePart)");
						var response = JsonConvert.DeserializeObject<Part>(json);
						var muadilPart = new MuadilPart
							{
								Number = item.AnaParcaNumber,
								Alternates = new List<Alternates2>
			{
				new Alternates2
				{
					AlternatePart = new AlternatePart2
					{
						Number = item.MuadilParcaNumber,
						isCancel = false
					}
				}
			}
							};

							var jsonData2 = JsonConvert.SerializeObject(muadilPart);
							var jsonDataResponse = JsonConvert.SerializeObject(response);
							ApiService _apiService = new ApiService();
							dynamic dataResponse = null;
							try
							{
								dataResponse = await _apiService.PostDataAsync(apiFullUrl, apiURL, endPoint, jsonData2, jsonData2); // await eklendi
								LogService logService = new LogService(_configuration);
								logService.CreateJsonFileLog(dataResponse, $"Ana parça: {item.AnaParcaNumber} - Muadil parça: {item.MuadilParcaNumber} ile ilişkilendirildi. " + dataResponse.message);

								var deleteQuery = $"DELETE FROM {catalogValue}.Des_AlternateLink_LogTable_Error WHERE LogID = @LogID";
								await conn.ExecuteAsync(deleteQuery, new { LogID = item.LogID }); // await eklendi
							}
							catch (Exception ex)
							{
						




								var deleteQuery = $"DELETE FROM {catalogValue}.Des_AlternateLink_LogTable_Error WHERE LogID = @LogID";
								await conn.ExecuteAsync(deleteQuery, new { LogID = item.LogID });


							}


						
					}
				}



			}
			catch (Exception ex)
			{
			}

		}

		public async Task getErrorRemovedAlternateData(IConfiguration _configuration, IDbConnection conn, string catalogValue, string apiFullUrl, string apiURL, string sourceApi, string endPoint)
		{
			try
			{


				var SQL_Alternate = $"SELECT * FROM {catalogValue}.Des_AlternateLinkRemoved_LogTable_Error";

				var responseData = await conn.QueryAsync<dynamic>(SQL_Alternate);

				var dataList = responseData.ToList();






				if (responseData != null)
				{
					foreach (var item in dataList)
					{


						var json = await _getWindchillApiServices.GetApiData($"ProdMgmt/Parts('OR:wt.part.WTPart:{item.AnaParcaPartID}')?$expand=Alternates($expand=AlternatePart)");
						var response = JsonConvert.DeserializeObject<Part>(json);
						var muadilPart = new MuadilPart
						{
							Number = item.AnaParcaNumber,
							Alternates = new List<Alternates2>
			{
				new Alternates2
				{
					AlternatePart = new AlternatePart2
					{
						Number = item.MuadilParcaNumber,
						isCancel = true
					}
				}
			}
						};

						var jsonData2 = JsonConvert.SerializeObject(muadilPart);
						var jsonDataResponse = JsonConvert.SerializeObject(response);
						ApiService _apiService = new ApiService();

						dynamic dataResponse = null;
						try
						{
							dataResponse = await _apiService.PostDataAsync(apiFullUrl, apiURL, endPoint, jsonData2, jsonData2); // await eklendi
							LogService logService = new LogService(_configuration);
							logService.CreateJsonFileLog(dataResponse, $"Ana parça: {item.AnaParcaNumber} - Muadil parça: {item.MuadilParcaNumber} muadil ilişkisi kaldırıldı." + dataResponse.message);

							var deleteQuery = $"DELETE FROM {catalogValue}.Des_AlternateLinkRemoved_LogTable_Error WHERE LogID = @LogID";
							await conn.ExecuteAsync(deleteQuery, new { LogID = item.LogID }); // await eklendi
						}
						catch (Exception ex)
						{
							var deleteQuery = $"DELETE FROM {catalogValue}.Des_AlternateLinkRemoved_LogTable_Error WHERE LogID = @LogID";
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


	public class MuadilPart : BaseEntity
	{
		public string Number { get; set; }
		public List<Alternates2> Alternates { get; set; }
	}
	public class Alternates2
	{
		public AlternatePart2 AlternatePart { get; set; }
	}
	public class AlternatePart2 : BaseEntity
	{
		[Key]
		public string? Number { get; set; }
		public bool isCancel { get; set; }
	}
}
