using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Model.WindchillApiModel.MfgProcMgmt
{
	public class State
	{
		public string Value { get; set; }
		public string Display { get; set; }
	}

	public class WorkInProgressState
	{
		public string Value { get; set; }
		public string Display { get; set; }
	}

	public class Material
	{
		public string ODataContext { get; set; }
		public string CabinetName { get; set; }
		public string CheckOutStatus { get; set; }
		public string CheckoutState { get; set; }
		public object Comments { get; set; }
		public string CreatedBy { get; set; }
		public DateTime CreatedOn { get; set; }
		public string FolderLocation { get; set; }
		public string FolderName { get; set; }
		public string ID { get; set; }
		public DateTime LastModified { get; set; }
		public bool Latest { get; set; }
		public string LifeCycleTemplateName { get; set; }
		public string ModifiedBy { get; set; }
		public string Name { get; set; }
		public string Number { get; set; }
		public string Revision { get; set; }
        //public State State { get; set; }
        public string StateValue { get; set; }
        public string StateDisplay { get; set; }
        public string Version { get; set; }
		public string VersionID { get; set; }
		public string View { get; set; }
		//public WorkInProgressState WorkInProgressState { get; set; }
		public string WorkInProgressStateValue { get; set; }
		public string WorkInProgressStateDisplay { get; set; }
		public string PTC_AppliedContainerContext_LocalTimeZone { get; set; }
	}
}
