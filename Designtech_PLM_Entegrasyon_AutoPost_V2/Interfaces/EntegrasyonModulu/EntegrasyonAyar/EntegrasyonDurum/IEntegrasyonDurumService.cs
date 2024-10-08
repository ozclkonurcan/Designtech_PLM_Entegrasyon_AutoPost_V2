using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModulu.EntegrasyonAyar.EntegrasyonDurum
{
	public interface IEntegrasyonDurumService
	{
		Task EntegrasyonDurumUpdate(string state, long idA2A2);
		Task EntegrasyonHataDurumUpdate(long idA2A2);
		Task EntegrasyonDurumRESET(string state, long idA2A2);
	}
}
