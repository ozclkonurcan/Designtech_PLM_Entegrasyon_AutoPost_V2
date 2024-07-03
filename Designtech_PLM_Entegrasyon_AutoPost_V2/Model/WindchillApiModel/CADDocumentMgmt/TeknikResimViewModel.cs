using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Model.WindchillApiModel.CADDocumentMgmt
{
	public class TeknikResimViewModel
	{
        public string? TransferID { get; set; } = Guid.NewGuid().ToString();
		public string? Number { get; set; }
		public string? Revizyon { get; set; }
		public string? DocumentType { get; set; }
        public string? Description { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? AuthorizationDate { get; set; }
		public string? ModifiedBy { get; set; }
		public State? State { get; set; }
		public CADContent? CadContent { get; set; }
		public CAD_WTPART_Iliski? Cad_WTPART_Iliski { get; set; }
    }

	public class State
	{
        public string Value { get; set; } = "50";
		public string? Display { get; set; }

	}

	public class CADContent
	{
		public string? Name { get; set; }
		public byte[]? FileData { get; set; }
		//public string? Content { get; set; }

	}

	public class CAD_WTPART_Iliski
	{
		public string? RelatedPartNumber { get; set; }
		public string? RelatedPartName { get; set; }

	}



    public class TeknikResim2ViewModel
    {
        public string? TransferID { get; set; } = Guid.NewGuid().ToString();
        public string? Number { get; set; }
        public string? Revizyon { get; set; }
        public string? DocumentType { get; set; }
        public string? Description { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? AuthorizationDate { get; set; }
        public string? ModifiedBy { get; set; }
        public int state { get; set; } = 0;
        public string name { get; set; }
        public byte[]? content { get; set; }
        public string projectCode { get; set; }
        public RelatedParts relatedParts { get; set; }
    }


    public class RelatedParts
    {
        public string? RelatedPartNumber { get; set; }
        public string? RelatedPartName { get; set; }
        public bool? isUpdateAndDelete { get; set; }

    }

    public class TeknikResimCancel
    {
        public string? TransferID { get; set; } = Guid.NewGuid().ToString();
        public string? Number { get; set; }
        public string? Revizyon { get; set; }
    }
}
