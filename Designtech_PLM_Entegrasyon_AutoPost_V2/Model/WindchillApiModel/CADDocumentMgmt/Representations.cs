using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Model.WindchillApiModel.CADDocumentMgmt
{
	public class AdditionalFile
	{
		public string URL { get; set; }
		public string Label { get; set; }
		public int FileSize { get; set; }
		public string MimeType { get; set; }
		public string Format { get; set; }
		public string ID { get; set; }
		public string FileName { get; set; }
		public DateTime LastModified { get; set; }
		public DateTime CreatedOn { get; set; }
		public string Description { get; set; }
		public string Comments { get; set; }
	}

	public class CreoViewURL
	{
		public string URL { get; set; }
		public string Label { get; set; }
	}

	public class TwoDThumbnailURL
	{
		public string URL { get; set; }
		public string Label { get; set; }
		public int FileSize { get; set; }
		public string MimeType { get; set; }
		public string Format { get; set; }
		public string ID { get; set; }
		public string FileName { get; set; }
		public DateTime LastModified { get; set; }
		public DateTime CreatedOn { get; set; }
		public string Description { get; set; }
		public string Comments { get; set; }
	}

	public class Value
	{
		public List<AdditionalFile> AdditionalFiles { get; set; }
		public List<double> BoundingBox { get; set; }
		public DateTime CreatedOn { get; set; }
		public CreoViewURL CreoViewURL { get; set; }
		public bool DefaultRepresentation { get; set; }
		public string Description { get; set; }
		public string FormatName { get; set; }
		public bool HasBoundingBox { get; set; }
		public bool HasOctree { get; set; }
		public string ID { get; set; }
		public DateTime LastModified { get; set; }
		public string Name { get; set; }
		public bool OutOfDate { get; set; }
		public object ThreeDThumbnailURL { get; set; }
		public TwoDThumbnailURL TwoDThumbnailURL { get; set; }
	}

	public class AdditionalFileValue
	{
		public List<Value> Value { get; set; }
	}
}
