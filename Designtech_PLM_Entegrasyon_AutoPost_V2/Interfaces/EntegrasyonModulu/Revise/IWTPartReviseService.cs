using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModulu.Revise
{
	public interface IWTPartReviseService
	{
		Task ProcessReviseAsync(string state, string catalogValue, SqlConnection conn);
	}
}
