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
        public string Value { get; set; } = "30";
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
        public string? Description { get; set; } = "NULL Attr";
        public DateTime? ModifiedOn { get; set; }
        public DateTime? AuthorizationDate { get; set; }
        public string? ModifiedBy { get; set; }
        public int state { get; set; } = 30;
        public string name { get; set; }
        public string? content { get; set; }
        public string projectCode { get; set; }
        public List<RelatedParts> relatedParts { get; set; }
    }
        public class Ent_EPMDocStateModel
    {
        public long Ent_ID { get; set; }
        public long EPMDocID { get; set; }
        public string? StateDegeri { get; set; }
        public string? idA3masterReference { get; set; }
        public string? CadName { get; set; }
        public string? name { get; set; }
        public string? docNumber { get; set; }
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
