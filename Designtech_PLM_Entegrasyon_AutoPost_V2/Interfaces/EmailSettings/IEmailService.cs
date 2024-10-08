using Designtech_PLM_Entegrasyon_AutoPost.Model.WindchillApiModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EmailSettings
{
	public interface IEmailService
	{
		Task EmailControlString(WTUsers emailControlString);
	}
}
