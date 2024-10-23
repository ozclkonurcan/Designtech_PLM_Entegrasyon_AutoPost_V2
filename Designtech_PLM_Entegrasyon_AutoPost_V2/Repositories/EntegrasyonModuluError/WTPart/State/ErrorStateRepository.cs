using Dapper;
using Designtech_PLM_Entegrasyon_AutoPost.ApiServices;
using Designtech_PLM_Entegrasyon_AutoPost.Helper;
using Designtech_PLM_Entegrasyon_AutoPost.Model.WindchillApiModel;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModulu.EntegrasyonAyar.EntegrasyonDurum;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModulu.WTPart.State;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModuluError.WTPart.State;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.WindchillApiSettings;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.EntegrasyonModuluError.WTPart.State
{
	public class ErrorStateRepository : IErrorStateService
	{
		private readonly IConfiguration _configuration;
		private readonly IEntegrasyonDurumService _entegrasyonDurumService;
		private readonly IGetWindchillApiServices _getWindchillApiServices;
		public ErrorStateRepository(IEntegrasyonDurumService entegrasyonDurumService, IGetWindchillApiServices getWindchillApiServices)
		{
			_entegrasyonDurumService = entegrasyonDurumService;
			_getWindchillApiServices = getWindchillApiServices;
		}

		private readonly ApiService _apiService = new();
		private const int BatchSize = 2000;

		public async Task getErrorCancelledData(IConfiguration configuration, IDbConnection conn, string catalogValue, string state, string apiFullUrl, string apiURL, string sourceApi, string endPoint, string CSRF_NONCE, string windchillServerName, string serverName, string basicUsername, string basicPassword)
		{
			await ProcessStateAsync(configuration, conn, catalogValue, state, apiFullUrl, apiURL, sourceApi, endPoint, CSRF_NONCE, windchillServerName, serverName, basicUsername, basicPassword);
		}

		public async Task getErrorInworkData(IConfiguration configuration, IDbConnection conn, string catalogValue, string state, string apiFullUrl, string apiURL, string sourceApi, string endPoint, string CSRF_NONCE, string windchillServerName, string serverName, string basicUsername, string basicPassword)
		{
			await ProcessStateAsync(configuration, conn, catalogValue, state, apiFullUrl, apiURL, sourceApi, endPoint, CSRF_NONCE, windchillServerName, serverName, basicUsername, basicPassword);
		}

		public async Task getErrorReleasedData(IConfiguration configuration, IDbConnection conn, string catalogValue, string state, string apiFullUrl, string apiURL, string sourceApi, string endPoint, string CSRF_NONCE, string windchillServerName, string serverName, string basicUsername, string basicPassword)
		{
			await ProcessStateAsync(configuration, conn, catalogValue, state, apiFullUrl, apiURL, sourceApi, endPoint, CSRF_NONCE, windchillServerName, serverName, basicUsername, basicPassword);
		}

		private async Task ProcessStateAsync(IConfiguration configuration, IDbConnection conn, string catalogValue, string state, string apiFullUrl, string apiURL, string sourceApi, string endPoint, string CSRF_NONCE, string windchillServerName, string serverName, string basicUsername, string basicPassword)
		{
			try
			{
				var sql = $"SELECT * FROM {catalogValue}.Des_WTPart_LogTable_Error WHERE [ParcaState] = @ParcaState";
				//if (state == "INWORK")
				//	sql += " AND [ParcaVersion] NOT LIKE 'A%'";

				var offset = 0;
				var hasMoreRecords = true;
				var windchillApiService = new WindchillApiService();

				while (hasMoreRecords)
				{
					var query = $"{sql} ORDER BY ParcaPartID OFFSET @Offset ROWS FETCH NEXT @BatchSize ROWS ONLY";
					var resolvedItems = await conn.QueryAsync<dynamic>(query, new { ParcaState = state, Offset = offset, BatchSize });

					if (!resolvedItems.Any())
						hasMoreRecords = false;

					offset += BatchSize;
					var resolvedItemsList = resolvedItems.ToList();

					//await Parallel.ForEachAsync(resolvedItemsList, async (partItem, cancellationToken) =>
					//{ });


					foreach (var partItem in resolvedItemsList)
					{


						var json = await _getWindchillApiServices.GetApiData($"ProdMgmt/Parts('OR:wt.part.WTPart:{partItem.ParcaPartID}')?$expand=Alternates($expand=AlternatePart)");
						var response = JsonConvert.DeserializeObject<Part>(json);


						await ProcessResponse(response, state, conn, configuration, apiFullUrl, apiURL, endPoint, partItem.ParcaPartID, catalogValue);

						await conn.ExecuteAsync($"DELETE FROM [{catalogValue}].[Des_WTPart_LogTable_Error] WHERE [ParcaPartID] = @ParcaPartID", new { partItem.ParcaPartID });
					}


					await Task.Delay(1000);
				}
			}
			catch (Exception ex)
			{
				// Hata yönetimi
			}
		}

		private async Task ProcessResponse(Part response, string state, IDbConnection conn, IConfiguration configuration, string apiFullUrl, string apiURL, string endPoint, long ID, string catalogValue)
		{
			if (response.State.Value != state) return;

			string jsonDataAPI = state switch
			{
				"CANCELLED" => CreateCancelledPartJson(response),
				"RELEASED" => CreateReleasedPartJson(response),
				_ => string.Empty
			};

			dynamic dataResponse = null;
			

				if (state != "INWORK")
				{

					dataResponse = await _apiService.PostDataAsync(apiFullUrl, apiURL, endPoint, jsonDataAPI, jsonDataAPI);
				}
				await LogAndSaveData(response, state, conn, configuration, dataResponse, ID,catalogValue);
		
		}


	
		private static string CreateCancelledPartJson(Part response)
		{
			response.State.Value = "P";
			response.State.Display = "Pasif";
			var anaPartCancelled = new AnaPartCancelled
			{
				Number = response.Number,
				State = response.State,
			};
			return JsonConvert.SerializeObject(anaPartCancelled);
		}

		private static string CreateReleasedPartJson(Part response)
		{
			response.State.Value = "A";
			response.State.Display = "Aktif";
			var anaPart = new AnaPart
			{
				Number = response.Number,
				Name = response.Name,
				Fai = "H",
				MuhasebeKodu = "0000000",
				PlanlamaTipiKodu = "P",
				PLM = "E",
				State = response.State,
				TransferID = response.TransferID,
				Description = response.Description,
				BirimKodu = response.BirimKodu ?? "AD",
				CLASSIFICATION = response.CLASSIFICATION
			};
			return JsonConvert.SerializeObject(anaPart);
		}

		private async Task LogAndSaveData(Part response, string state, IDbConnection conn, IConfiguration configuration, ApiErrorResponse dataResponse, long ID,string catalogValue)
		{
			var logData = JsonConvert.SerializeObject(response);

			await conn.ExecuteAsync($@"
				INSERT INTO [{catalogValue}].[Change_Notice_LogTable] 
				([TransferID],[idA2A2], [ProcessTimestamp], [updateStampA2], [statestate], [name], [WTPartNumber],[Version],[VersionID]) 
				VALUES (@TransferID, @idA2A2, @ProcessTimestamp, @updateStampA2, @statestate, @name, @WTPartNumber, @Version, @VersionID)",
				new
				{
					response.TransferID,
					idA2A2 = response.ID.Split(':')[2],
					ProcessTimestamp = DateTime.UtcNow,
					updateStampA2 = response.LastModified,
					statestate = response.State.Value,
					name = response.Name,
					WTPartNumber = response.Number,
					response.Version,
					response.VersionID
				});

			LogService logService = new LogService(configuration);
			if (state == "RELEASED")
			{
				if (response.EntegrasyonDurumu is null or not "Parça entegre oldu" && state == "RELEASED" && !string.IsNullOrEmpty(response.BirimKodu))
				{
					await _entegrasyonDurumService.EntegrasyonDurumUpdate(state, ID);

				}

				logService.CreateJsonFileLog(logData, $"Parça gönderildi. {dataResponse.message}");
			}
			if (state == "CANCELLED")
			{
				if (response.EntegrasyonDurumu is null or not "Parça iptal oldu" && state == "CANCELLED")
				{
					await _entegrasyonDurumService.EntegrasyonDurumUpdate(state, ID);
				}
				logService.CreateJsonFileLog(logData, $"Parça iptal edildi. {dataResponse.message}");
			}
			if (state == "INWORK")
			{
				if (response.EntegrasyonDurumu is null or not "Parça devam ediyor" && state == "INWORK")
				{
					await _entegrasyonDurumService.EntegrasyonDurumUpdate(state, ID);
				}

				logService.CreateJsonFileLog(logData, $"Parça devam ediyor.");
			}
		}
	}

}
