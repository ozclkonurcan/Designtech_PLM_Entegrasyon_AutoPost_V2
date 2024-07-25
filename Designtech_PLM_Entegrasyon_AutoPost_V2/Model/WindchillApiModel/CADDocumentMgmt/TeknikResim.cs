using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Model.WindchillApiModel.CADDocumentMgmt
{
	public class TeknikResim
	{
        public string OdataContext { get; set; }
        public string OdataType { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ID { get; set; }
        public DateTime LastModified { get; set; }
        public string FileName { get; set; }
        public string ModifiedBy { get; set; }

        public string Name { get; set; }
        public string Number { get; set; }
        public string Description { get; set; }

        public string Revision { get; set; }

        public State State { get; set; }

        public string Version { get; set; }
        public string VersionID { get; set; }
        public List<Attachment> Attachments { get; set; }
	}


	
	public class Content
	{
		public string? URL { get; set; }
		public string? Label { get; set; }
	}

	public class Attachment
	{
		public Content? Content { get; set; }

	}

    
    public class PartDocAssociation
    {
		public string? ID { get; set; }

	}

	public class CADDocumentResponse
    {
        public List<PartDocAssociation> Value { get; set; }
    }
	
	

public class AdditionalFile
    {
        public string URL { get; set; }
        public string Label { get; set; }
        public string FileSize { get; set; }
        public string MimeType { get; set; }
        public string Format { get; set; }
        public string ID { get; set; }
        public string FileName { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Description { get; set; }
        public string Comments { get; set; }
    }



    public class Representation
    {
        public List<AdditionalFile> AdditionalFiles { get; set; }
        public DateTime CreatedOn { get; set; }

        public string ID { get; set; }
        public DateTime LastModified { get; set; }
        public string Name { get; set; }
    }


    public class RootObject
    {
        public string OdataContext { get; set; }
        public string OdataType { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ID { get; set; }
        public DateTime LastModified { get; set; }
        public string FileName { get; set; }
        public string ModifiedBy { get; set; }

        public string Name { get; set; }
        public string Number { get; set; }
        public string Description { get; set; }

        public string Revision { get; set; }

        public State State { get; set; }

        public string Version { get; set; }
        public string VersionID { get; set; }
        public List<Representation> Representations { get; set; }
    }

  


}
