using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.ViewModel.WTDocAttachmentsModel
{
	
	public class WTDocumentRoot
	{
		public List<Item> Items { get; set; }
	}

	public class Item
	{
		public string Id { get; set; }
		public string TypeId { get; set; }
	}

}
