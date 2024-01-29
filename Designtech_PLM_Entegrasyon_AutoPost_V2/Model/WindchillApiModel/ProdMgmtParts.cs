using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost.Model.WindchillApiModel
{
	public class ProdMgmtParts
	{
		public string Context { get; set; } = string.Empty;
		public List<Part> Value { get; set; } 
	}

	public class Part
	{
        public DateTime CreatedOn { get; set; }
        public string ID { get; set; }
        public DateTime LastModified { get; set; }
        public string AlternateNumber { get; set; }

        /// Test Amaçlı
        public string AssemblyModeValue { get; set; }
        public string AssemblyModeDisplay { get; set; }
        /// Test Amaçlı
        //[NotMapped]
        //public AssemblyMode AssemblyMode { get; set; }
        //public string BOMType { get; set; }
        public string CADName { get; set; }
        //public string CLASSIFICATION { get; set; }
        public string CabinetName { get; set; }
        //public string ChangeStatus { get; set; }
        public string CheckOutStatus { get; set; }
        public string CheckoutState { get; set; }
        public string Comments { get; set; }
        public string ComponentType { get; set; }
        /// Test Amaçlı
        public string ConfigurableModuleValue { get; set; }
        public string ConfigurableModuleDisplay { get; set; }
        /// Test Amaçlı
        //[NotMapped]
        //public ConfigurableModule ConfigurableModule { get; set; }
        public string CreatedBy { get; set; }
        /// Test Amaçlı
        public string DefaultTraceCodeValue { get; set; }
        public string DefaultTraceCodeDisplay { get; set; }
        /// Test Amaçlı
        //[NotMapped]
        //public DefaultTraceCode DefaultTraceCode { get; set; }
        /// Test Amaçlı
        public string DefaultUnitValue { get; set; }
        public string DefaultUnitDisplay { get; set; }
        /// Test Amaçlı
        //[NotMapped]
        //public DefaultUnit DefaultUnit { get; set; }
        public string DenemeNX { get; set; }
        public string Description { get; set; }
        public bool EndItem { get; set; }
        public string FolderLocation { get; set; }
        public string FolderName { get; set; }
        public bool GatheringPart { get; set; }
        public string GeneralStatus { get; set; }
        public string Identity { get; set; }
        public string KaleKod { get; set; }
        public string Kaleargenumber { get; set; }
        public bool Latest { get; set; }
        public string Length { get; set; }
        public string LifeCycleTemplateName { get; set; }
        //public double Mass { get; set; }
        public string Material { get; set; }
        public string ModifiedBy { get; set; }
        public string NAME10 { get; set; }
        public string NAME20 { get; set; }
        public string NAME201_PTCC_MultipleAliasAttributeValues { get; set; }
        public string NAME201 { get; set; }
        public string Name { get; set; }
        public string Name30 { get; set; }
        public string Number { get; set; }


        //		[NotMapped]
        //public List<string> OEMPartSourcingStatus { get; set; }
        public string ObjectType { get; set; }
        public string OrganizationReference { get; set; }
        public string PARCAADI { get; set; }
        public string PTCWMNAME { get; set; }
        public bool PhantomManufacturingPart { get; set; }
        public string Revision { get; set; }
        //public string ShareStatus { get; set; }

        /// Test Amaçlı
        public string SourceValue { get; set; }
        public string SourceDisplay { get; set; }
        /// Test Amaçlı
        //[NotMapped]
        //public Source Source { get; set; }
        public string SourceDuplicate { get; set; }
        public string Standard { get; set; }
        /// Test Amaçlı
        public string StateValue { get; set; }
        public string StateDisplay { get; set; }
        /// Test Amaçlı
        //[NotMapped]
        public State State { get; set; }
        public string Supersedes { get; set; }
        public string Supplier { get; set; }
        //public List<string> TalepEden { get; set; }
        public string Thickness { get; set; }
        /// Test Amaçlı
        public string TypeIconPath { get; set; }
        public string TypeIconTooltip { get; set; }
        /// Test Amaçlı
        //[NotMapped]
        //public TypeIcon TypeIcon { get; set; }
        //public string UretimYeri { get; set; }
        public string Version { get; set; }
        public string VersionID { get; set; }
        public string View { get; set; }
        /// Test Amaçlı
        public string WorkInProgressStateValue { get; set; }
        public string WorkInProgressStateDisplay { get; set; }
        /// Test Amaçlı
        //[NotMapped]
        //public WorkInProgressState WorkInProgressState { get; set; }
    }

    public class AssemblyMode
	{
		public string Value { get; set; } = string.Empty;
		public string Display { get; set; } = string.Empty;
	}

	public class ConfigurableModule
	{
		public string Value { get; set; } = string.Empty;
		public string Display { get; set; } = string.Empty;
	}

	public class DefaultTraceCode
	{
		public string Value { get; set; } = string.Empty;
		public string Display { get; set; } = string.Empty;
	}

	public class DefaultUnit
	{
		public string Value { get; set; } = string.Empty;
		public string Display { get; set; } = string.Empty;
	}

	public class Source
	{
		public string Value { get; set; } = string.Empty;
		public string Display { get; set; } = string.Empty;
	}

	public class State
	{
		public string Value { get; set; } = string.Empty;
		public string Display { get; set; } = string.Empty;
	}

	public class TypeIcon
	{
		public string Path { get; set; } = string.Empty;
		public string Tooltip { get; set; } = string.Empty;
	}

	public class WorkInProgressState
	{
		public string Value { get; set; } = string.Empty;
		public string Display { get; set; } = string.Empty;
	}
}
