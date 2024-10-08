using Dapper;
using Designtech_PLM_Entegrasyon_AutoPost.ApiServices;
using Designtech_PLM_Entegrasyon_AutoPost.ViewModel;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModulu.EntegrasyonAyar.EntegrasyonDurum;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModulu.Revise;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.EntegrasyonModulu.Revise
{
	public class WTPartReviseRepository : IWTPartReviseService
	{
		private readonly IEntegrasyonDurumService _entegrasyonDurumService;

		public WTPartReviseRepository(IEntegrasyonDurumService entegrasyonDurumService)
		{
			_entegrasyonDurumService = entegrasyonDurumService;
		}

		public async Task ProcessReviseAsync(string state, string catalogValue, SqlConnection conn)
		{

			try
			{

				var SQL_ReviseAndSaveAs = $"SELECT * FROM {catalogValue}.Des_LogDataReviseAndSaveAsProcess WHERE [statestate] = 'INWORK'";

				var responseData = await conn.QueryAsync<dynamic>(SQL_ReviseAndSaveAs);

				var dataList = responseData.ToList();

				if(responseData.Count() > 0)
				{


				WindchillApiService windchillApiService = new WindchillApiService();
				foreach (var item in dataList)
				{
				await _entegrasyonDurumService.EntegrasyonDurumRESET(item.statestate, item.PartID);
						var deleteQuery = $"DELETE FROM {catalogValue}.Des_LogDataReviseAndSaveAsProcess WHERE LogID = @LogID";
						await conn.ExecuteAsync(deleteQuery, new { LogID = item.LogID });
					}
				}

			}
			catch (Exception)
			{
			}


		}
	}
}
