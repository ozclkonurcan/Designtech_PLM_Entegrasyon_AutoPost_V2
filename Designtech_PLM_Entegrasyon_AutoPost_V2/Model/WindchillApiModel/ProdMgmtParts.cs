using Designtech_PLM_Entegrasyon_AutoPost_V2.Model.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost.Model.WindchillApiModel
{
	public class ProdMgmtParts
	{
		public string Context { get; set; } = string.Empty;
		public List<Part> Value { get; set; } 
	}


	public class Part : BaseEntity
	{
	
		[Key]
		public string? ID { get; set; }
		public string? Number { get; set; }
		public string? Name { get; set; }
		public string? Description { get; set; }
		public State? State { get; set; }
		public string? MuhasebeKodu { get; set; } = "0000000";
		public string? MuhasebeAdi { get; set; }
		public string? BirimAdi { get; set; }
		public string? BirimKodu { get; set; }

		public string? PlanlamaTipiKodu { get; set; } = "P";
        public string? Fai { get; set; }
		public string? PLM { get; set; } = "E";
        public CLASSIFICATION? CLASSIFICATION { get; set; }
        public string? EntegrasyonDurumu { get; set; }
        public string? EntegrasyonTarihi { get; set; }
        public List<Alternates>? Alternates { get; set; }


		public DateTime? CreatedOn { get; set; }
		public DateTime? LastModified { get; set; }
		public string? Version { get; set; }
		public string? VersionID { get; set; }
	}

    public class PartPDF : BaseEntity
    {

        [Key]
        public string? ID { get; set; }
        public string? Number { get; set; }
        public string? Name { get; set; }
        public List<ProjeKodu>? ProjeKodu { get; set; }
    
    }

    public class ProjeKodu {
        public string? Value { get; set; }
        public string? Display { get; set; }
    }
    public class Creator
    {
        [Key]
        public string? ID { get; set; }
        public string? Identity { get; set; }
        public string? Name { get; set; }
    }
    public class WTUsers
    {
        [JsonProperty("value")]
        public List<User> Users { get; set; }
    }

    public class User
    {

        [JsonProperty("ID")]
        public string ID { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("EMail")]
        public string EMail { get; set; }

        [JsonProperty("FullName")]
        public string FullName { get; set; }

    }


    public class AnaPart : BaseEntity
    {

        [Key]
        public string? ID { get; set; }
        public string? Number { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public State? State { get; set; }
        public string? MuhasebeKodu { get; set; } = "0000000";
        public string? BirimKodu { get; set; }

        public string? PlanlamaTipiKodu { get; set; } = "P";
        public string? Fai { get; set; } = "H";
        public string? PLM { get; set; } = "E";
        public CLASSIFICATION? CLASSIFICATION { get; set; }


    }
    public class AnaPartCancelled : BaseEntity
    {

        [Key]
        public string? Number { get; set; }
        public State? State { get; set; }
    }
    public class AnaPartCancelledLOG : BaseEntity
    {

        [Key]
        public string? Number { get; set; }
        public string? Name { get; set; }
        public State? State { get; set; }
    }
    public class MuadilPart : BaseEntity
    {

        public string? Number { get; set; }
        public List<Alternates2>? Alternates { get; set; }

    }
    public class RemovePart : BaseEntity
    {

        public string? Number { get; set; }
        public string? MuadilPartNumber { get; set; }
    }

    #region CLASSTIFICATION CLASS

    public class CLASSIFICATION
	{
		private string classificationHierarchy;

		public string ClfNodeHierarchyDisplayName
		{
			get { return classificationHierarchy; }
			set
			{
				classificationHierarchy = value;
				// Ayrıca ClassificationHierarchy'yi güncelle
				ClassificationHierarchy = value;
			}
		}

        public string ClassificationHierarchy { get; set; } = "NULL parça";
	}


	//public class CLASSIFICATION
	//{
	//	public string ClfNodeInternalName { get; set; }
	//	public string ClfNodeDisplayName { get; set; }
	//	public string ClfNodeHierarchyDisplayName { get; set; }
	//	public List<ClassificationAttribute> ClassificationAttributes { get; set; }
	//}

	public class ClassificationAttribute
	{
		[Key]
		public string InternalName { get; set; }
		public string DisplayName { get; set; }
		public string Value { get; set; }
		public string DisplayValue { get; set; }
	}
	#endregion



	#region Alternates Classı

	public class Alternates
	{
		[Key]
		public string? ID { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? LastModified { get; set; }
        public string? ObjectType { get; set; }
        public AlternatePart? AlternatePart { get; set; }
	}

	public class ReplacementType
	{
		[Key]
		public string Value { get; set; }
		public string Display { get; set; }
	}

	public class AlternatePart:BaseEntity
	{

		[Key]
		public string? ID { get; set; }
		public string? Number { get; set; }
		public string? Name { get; set; }
		public string? Description { get; set; }
		public string? Version { get; set; }
		public State? State { get; set; }

		public string? MuhasebeAdi { get; set; }
		public string? MuhasebeKodu { get; set; } = "0000000";
		public string? BirimAdi { get; set; }
		public string? BirimKodu { get; set; }
        public DateTime? LastModified { get; set; }
        public string? PlanlamaTipiKodu { get; set; } = "P";
        public string? Fai { get; set; }
        public string? PLM { get; set; } = "E";
        public CLASSIFICATION? CLASSIFICATION { get; set; }





	}





    //Yedek ALternates

    public class Alternates2
    {
        [Key]
        public AlternatePart2? AlternatePart { get; set; }
    }

   

    public class AlternatePart2 : BaseEntity
    {

        [Key]
        public string? Number { get; set; }
        public bool isCancel { get; set; }

    }
    #endregion


    //public class Part
    //{
    //	//public string TransferID { get; set; } = MD5.Create().ComputeHash(Guid.NewGuid().ToByteArray()).ToString().Substring(0, 10);
    //	//public string TransferID { get; set; } = Guid.NewGuid();
    //       public DateTime CreatedOn { get; set; }
    //       public string? ID { get; set; }
    //       public DateTime LastModified { get; set; }
    //	//public List<AlternateNumber>? AlternateNumber { get; set; }
    //	public string? AlternateNumber { get; set; }

    //	/// Test Amaçlı
    //	public string? AssemblyModeValue { get; set; }
    //       public string? AssemblyModeDisplay { get; set; }
    //       /// Test Amaçlı
    //       //[NotMapped]
    //       public AssemblyMode? AssemblyMode { get; set; }
    //       public List<BomType>? BOMType { get; set; }
    //       public string? CADName { get; set; }
    //       //public CLASSIFICATION? CLASSIFICATION { get; set; }
    //       public string? CabinetName { get; set; }
    //       //public string ChangeStatus { get; set; }
    //       public string? CheckOutStatus { get; set; }
    //       public string? CheckoutState { get; set; }
    //       public string? Comments { get; set; }
    //       public string? ComponentType { get; set; }
    //       /// Test Amaçlı
    //       public string? ConfigurableModuleValue { get; set; }
    //       public string? ConfigurableModuleDisplay { get; set; }
    //       /// Test Amaçlı
    //       //[NotMapped]
    //       //public ConfigurableModule ConfigurableModule { get; set; }
    //       public string? CreatedBy { get; set; }
    //       /// Test Amaçlı
    //       public string? DefaultTraceCodeValue { get; set; }
    //       public string? DefaultTraceCodeDisplay { get; set; }
    //       /// Test Amaçlı
    //       //[NotMapped]
    //       //public DefaultTraceCode DefaultTraceCode { get; set; }
    //       /// Test Amaçlı
    //       public string? DefaultUnitDisplay { get; set; }
    //       public string? DefaultUnitValue { get; set; }
    //       /// Test Amaçlı
    //       //[NotMapped]
    //       //public DefaultUnit DefaultUnit { get; set; }
    //       public string? DenemeNX { get; set; }
    //       public string? Description { get; set; }
    //       public bool EndItem { get; set; }
    //       public string? FolderLocation { get; set; }
    //       public string? FolderName { get; set; }
    //       public bool GatheringPart { get; set; }
    //       //public string GeneralStatus { get; set; }
    //       public string? Identity { get; set; }
    //       public string? KaleKod { get; set; }
    //       public string? Kaleargenumber { get; set; }
    //       public bool Latest { get; set; }
    //       public string? Length { get; set; }
    //       public string? LifeCycleTemplateName { get; set; }
    //       //public double Mass { get; set; }
    //       public string? Material { get; set; }
    //       public string? ModifiedBy { get; set; }
    //       public string? NAME10 { get; set; }
    //       public string? NAME20 { get; set; }
    //       public string? NAME201_PTCC_MultipleAliasAttributeValues { get; set; }
    //       public string? NAME201 { get; set; }
    //       public string? Name { get; set; }
    //       public string? Name30 { get; set; }
    //       public string? Number { get; set; }


    //       //		[NotMapped]
    //       //public List<string> OEMPartSourcingStatus { get; set; }
    //       public string? stringType { get; set; }
    //       public string? OrganizationReference { get; set; }
    //       public string? PARCAADI { get; set; }
    //       public string? PTCWMNAME { get; set; }
    //       public bool PhantomManufacturingPart { get; set; }
    //       public string? Revision { get; set; }
    //       //public string ShareStatus { get; set; }

    //       /// Test Amaçlı
    //       public string? SourceValue { get; set; }
    //       public string? SourceDisplay { get; set; }
    //       /// Test Amaçlı
    //       //[NotMapped]
    //       //public Source Source { get; set; }
    //       public string? SourceDuplicate { get; set; }
    //       public string? Standard { get; set; }
    //       /// Test Amaçlı
    //       public string? StateValue { get; set; }
    //       public string? StateDisplay { get; set; }
    //       /// Test Amaçlı
    //       //[NotMapped]
    //       public State State { get; set; }
    //       public string? Supersedes { get; set; }
    //       public string? Supplier { get; set; }
    //       //public List<string> TalepEden { get; set; }
    //       public string? Thickness { get; set; }
    //       /// Test Amaçlı
    //       public string? TypeIconPath { get; set; }
    //       public string? TypeIconTooltip { get; set; }
    //       /// Test Amaçlı
    //       //[NotMapped]
    //       //public TypeIcon TypeIcon { get; set; }
    //       //public string UretimYeri { get; set; }
    //       public string? Version { get; set; }
    //       public string? VersionID { get; set; }
    //       public string? View { get; set; }
    //       /// Test Amaçlı
    //       public string? WorkInProgressStateValue { get; set; }
    //       public string? WorkInProgressStateDisplay { get; set; }

    //       /// Test Amaçlı
    //       //[NotMapped]
    //       //public WorkInProgressState WorkInProgressState { get; set; }
    //       public List<Alternates> Alternates { get; set; }
    //   }



    //#region CLASSTIFICATION CLASS
    //public class CLASSIFICATION
    //{
    //	public string ClfNodeInternalName { get; set; }
    //	public string ClfNodeDisplayName { get; set; }
    //	public string ClfNodeHierarchyDisplayName { get; set; }
    //	public List<ClassificationAttribute> ClassificationAttributes { get; set; }
    //}

    //public class ClassificationAttribute
    //{
    //	[Key]
    //	public string InternalName { get; set; }
    //	public string DisplayName { get; set; }
    //	public string Value { get; set; }
    //	public string DisplayValue { get; set; }
    //}
    //#endregion



    //#region Alternates Classı

    //public class Alternates 
    //   {
    //       [Key]
    //	public string? ID { get; set; }
    //	public string? CreatedOn { get; set; }
    //	public string? LastModified { get; set; }
    //	public string? stringType { get; set; }
    //	public ReplacementType? ReplacementType { get; set; }
    //	public bool TwoWay { get; set; }
    //	public TypeIcon? TypeIcon { get; set; }
    //	public AlternatePart? AlternatePart { get; set; }
    //}

    //public class ReplacementType
    //{
    //	[Key]
    //	public string Value { get; set; }
    //	public string Display { get; set; }
    //}

    //public class AlternatePart
    //{
    //	[Key]
    //	public string? ID { get; set; }
    //	public string? CreatedOn { get; set; }
    //	public string? LastModified { get; set; }
    //	public string? _15 { get; set; }
    //	public string? AlternateNumber { get; set; }
    //	public AssemblyMode? AssemblyMode { get; set; }
    //	public string? BOMType { get; set; }
    //	public string? CADName { get; set; }
    //	public string? CLASSIFICATION { get; set; }
    //	public string? CabinetName { get; set; }
    //	public string? ChangeStatus { get; set; }
    //	public string? CheckOutStatus { get; set; }
    //	public string? CheckoutState { get; set; }
    //	public string? Comments { get; set; }
    //	public string? ComponentType { get; set; }
    //	public ConfigurableModule? ConfigurableModule { get; set; }
    //	public string? CreatedBy { get; set; }
    //	public DefaultTraceCode? DefaultTraceCode { get; set; }
    //	public DefaultUnit? DefaultUnit { get; set; }
    //	public string? DenemeNX { get; set; }
    //	public string? Description { get; set; }
    //	public bool EndItem { get; set; }
    //	public string? EntegrasyonDurumu { get; set; }
    //	public string? EntegrasyonTarihi { get; set; }
    //	public string? FolderLocation { get; set; }
    //	public string? FolderName { get; set; }
    //	public bool GatheringPart { get; set; }
    //	public string? GeneralStatus { get; set; }
    //	public string? Identity { get; set; }
    //	public string? KaleKod { get; set; }
    //	public string? Kaleargenumber { get; set; }
    //	public bool Latest { get; set; }
    //	public string? Length { get; set; }
    //	public string? LifeCycleTemplateName { get; set; }
    //	public string? Mass { get; set; }
    //	public string? Material { get; set; }
    //	public string? ModifiedBy { get; set; }
    //	public string? MuhasebeKodu { get; set; }
    //	public string? NAME10 { get; set; }
    //	public string? NAME20 { get; set; }
    //	public string? NAME201 { get; set; }
    //	public string? Name { get; set; }
    //	public string? Name30 { get; set; }
    //	public string? Number { get; set; }
    //	public List<string>? OEMPartSourcingStatus { get; set; }
    //	public string? stringType { get; set; }
    //	public string? OrganizationReference { get; set; }
    //	public string? PARCAADI { get; set; }
    //	public string? PTCWMNAME { get; set; }
    //	public bool PhantomManufacturingPart { get; set; }
    //	public string? Revision { get; set; }
    //	public string? ShareStatus { get; set; }
    //	public Source? Source { get; set; }
    //	public string? SourceDuplicate { get; set; }
    //	public string? Standard { get; set; }
    //	public State? State { get; set; }
    //	public string? Supersedes { get; set; }
    //	public string? Supplier { get; set; }
    //	public List<string>? TalepEden { get; set; }
    //	public string? Thickness { get; set; }
    //	public TypeIcon? TypeIcon { get; set; }
    //	public string? UretimYeri { get; set; }
    //	public string? Version { get; set; }
    //	public string? VersionID { get; set; }
    //	public string? View { get; set; }
    //	public WorkInProgressState? WorkInProgressState { get; set; }
    //}

    //#endregion


    public class AlternateNumber
	{
		public string name { get; set; } = string.Empty;
		public string WTPartNumber { get; set; } = string.Empty;
		public string Version { get; set; } = string.Empty;
	} 
    public class AssemblyMode
	{
		[Key]
		public string Value { get; set; } = string.Empty;
		public string Display { get; set; } = string.Empty;
	}

	public class ConfigurableModule
	{
		[Key]
		public string Value { get; set; } = string.Empty;
		public string Display { get; set; } = string.Empty;
	}

	public class DefaultTraceCode
	{
		[Key]
		public string Value { get; set; } = string.Empty;
		public string Display { get; set; } = string.Empty;
	}

	public class DefaultUnit
	{
		[Key]
		public string Value { get; set; } = string.Empty;
		public string Display { get; set; } = string.Empty;
	}

	public class Source
	{
		[Key]
		public string Value { get; set; } = string.Empty;
		public string Display { get; set; } = string.Empty;
	}

	public class State
	{
		[Key]
		public string Value { get; set; } = string.Empty;
		public string Display { get; set; } = string.Empty;
	}

	public class TypeIcon
	{
		[Key]
		public string Path { get; set; } = string.Empty;
		public string Tooltip { get; set; } = string.Empty;
	}

	public class WorkInProgressState
	{
		[Key]
		public string Value { get; set; } = string.Empty;
		public string Display { get; set; } = string.Empty;
	}	
	
	public class BomType
	{
		[Key]
		public string Value { get; set; } = string.Empty;
		public string Display { get; set; } = string.Empty;
	}
}
