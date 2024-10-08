using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.SqlSettigns
{
	public interface ISqlTriggerAndTableManagerService
	{
		Task CreateTableAndTrigger(string connectionString);
	}
}
