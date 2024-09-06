using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.ViewModel.WTDocAttachmentsModel
{
	public class ApplicationData
	{
		public string OdataContext { get; set; }
		public List<Value> Value { get; set; }
		public string LocalTimeZone { get; set; }
	}

	public class Value
	{
		public string OdataMediaContentType { get; set; }
		public string Comments { get; set; }
		public string CreatedBy { get; set; }
		public DateTime CreatedOn { get; set; }
		public string Description { get; set; }
		public FormatIcon FormatIcon { get; set; }
		public string ID { get; set; }
		public DateTime LastModified { get; set; }
		public string ModifiedBy { get; set; }
		public Content Content { get; set; }
		public string FileName { get; set; }
		public long FileSize { get; set; }
		public string Format { get; set; }
		public string MimeType { get; set; }
	}

	public class FormatIcon
	{
		public string Path { get; set; }
		public string Tooltip { get; set; }
	}

	public class Content
	{
		public string URL { get; set; }
		public string Label { get; set; }
	}
}
