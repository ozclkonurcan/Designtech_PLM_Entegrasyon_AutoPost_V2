using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.WindchillApiSettings
{
	public interface IGetWindchillApiServices
	{
		Task<string> GetApiData(string endPoint);
		Task<string> GetApiToken();
	}
}
