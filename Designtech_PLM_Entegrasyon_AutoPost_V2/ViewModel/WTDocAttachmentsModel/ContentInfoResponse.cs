using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.ViewModel.WTDocAttachmentsModel
{
	public class ContentInfoResponse
	{
		[JsonProperty("contentInfos")]
		public List<ContentInfo> ContentInfos { get; set; }
	}

	public class ContentInfo
	{
		[JsonProperty("streamId")]
		public string StreamId { get; set; }

		[JsonProperty("fileSize")]
		public long FileSize { get; set; } 

		[JsonProperty("encodedInfo")]
		public string EncodedInfo { get; set; }
	}
}
