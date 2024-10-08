using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModuluError.WTPart.Alternate
{
	public interface IErrorAlternateService
	{
		Task getErrorAlternateData(IConfiguration _configuration, IDbConnection conn, string catalogValue, string apiFullUrl, string apiURL, string sourceApi, string endPoint);
		Task getErrorRemovedAlternateData(IConfiguration _configuration, IDbConnection conn, string catalogValue, string apiFullUrl, string apiURL, string sourceApi, string endPoint);
	}
}
