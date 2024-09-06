using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.ViewModel.WTDocAttachmentsModel
{
	public class ContentInfoStage3
	{
		public int StreamId { get; set; }
		public string EncodedInfo { get; set; }
		public string FileName { get; set; }
		public bool PrimaryContent { get; set; }
		public string MimeType { get; set; }
		public long FileSize { get; set; }
	}

	public class ContentInfoStage3RootObject
	{
		public List<ContentInfoStage3> ContentInfo { get; set; }
	}
}
