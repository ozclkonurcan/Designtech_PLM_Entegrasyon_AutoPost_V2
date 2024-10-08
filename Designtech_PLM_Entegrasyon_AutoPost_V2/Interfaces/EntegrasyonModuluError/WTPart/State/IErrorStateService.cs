using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModuluError.WTPart.State
{
	public interface IErrorStateService
	{
		Task getErrorReleasedData(IConfiguration _configuration, IDbConnection conn, string catalogValue, string state, string apiFullUrl, string apiURL, string sourceApi, string endPoint, string CSRF_NONCE, string WindchillServerName, string ServerName, string BasicUsername, string BasicPassword);
		Task getErrorCancelledData(IConfiguration _configuration, IDbConnection conn, string catalogValue, string state, string apiFullUrl, string apiURL, string sourceApi, string endPoint, string CSRF_NONCE, string WindchillServerName, string ServerName, string BasicUsername, string BasicPassword);
		Task getErrorInworkData(IConfiguration _configuration, IDbConnection conn, string catalogValue, string state, string apiFullUrl, string apiURL, string sourceApi, string endPoint, string CSRF_NONCE, string WindchillServerName, string ServerName, string BasicUsername, string BasicPassword);
	}
}
