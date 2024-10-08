using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModulu.WTPart.State
{
	public interface IStateService
	{
		Task getReleasedData(IConfiguration _configuration, IDbConnection conn, string catalogValue,string state, string apiFullUrl, string apiURL, string sourceApi, string endPoint,string CSRF_NONCE, string WindchillServerName,string ServerName,string BasicUsername,string BasicPassword);
		Task getCancelledData(IConfiguration _configuration, IDbConnection conn, string catalogValue,string state, string apiFullUrl, string apiURL, string sourceApi, string endPoint, string CSRF_NONCE, string WindchillServerName, string ServerName, string BasicUsername, string BasicPassword);
		Task getInworkData(IConfiguration _configuration, IDbConnection conn, string catalogValue,string state, string apiFullUrl, string apiURL, string sourceApi, string endPoint, string CSRF_NONCE, string WindchillServerName, string ServerName, string BasicUsername, string BasicPassword);
	}
}
