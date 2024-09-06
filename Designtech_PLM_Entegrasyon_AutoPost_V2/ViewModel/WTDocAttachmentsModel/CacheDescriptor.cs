using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.ViewModel.WTDocAttachmentsModel
{
	using System;
	using System.Collections.Generic;
	using Newtonsoft.Json;

	public class CacheDescriptor
	{
		[JsonProperty("@odata.context")]
		public string ODataContext { get; set; }

		public List<CacheDescriptorItem> Value { get; set; }

		[JsonProperty("@PTC.AppliedContainerContext.LocalTimeZone")]
		public string TimeZone { get; set; }
	}

	public class CacheDescriptorItem
	{
		public List<string> FileNames { get; set; }
		public int FolderId { get; set; }
		public int? ID { get; set; }  // Nullable int
		public string MasterUrl { get; set; }
		public string ReplicaUrl { get; set; }
		public List<int> StreamIds { get; set; }
		public int VaultId { get; set; }
	}

}
